using Arise.Tools.Packer.Passes;

namespace Arise.Tools.Packer;

internal static class DataCenterPacker
{
    // Adapted from PackCommand in Novadrop.

    private static readonly XNamespace _xsi = (XNamespace)"http://www.w3.org/2001/XMLSchema-instance";

    public static async ValueTask PackAsync(PackerOptions options)
    {
        await Terminal.OutLineAsync($"Loading data sheets in '{options.DataDirectory}'...");

        var root = DataCenter.Create();
        var keyCache = new ConcurrentDictionary<(string?, string?, string?, string?), DataCenterKeys>();
        var nodes = new ConcurrentBag<(DataCenterNode Node, int Index)>();
        var schemaProblems =
            new ConcurrentBag<(FileInfo File, int Line, int Column, XmlSeverityType Severity, string Message)>();

        await Parallel.ForEachAsync(
            options
                .DataDirectory
                .EnumerateFiles("?*-?*.xml", SearchOption.AllDirectories)
                .OrderBy(static file => file.FullName, StringComparer.Ordinal)
                .Select(static (file, index) => (File: file, Index: index)),
            async (tup, ct) =>
            {
                var file = tup.File;
                var xmlSettings = new XmlReaderSettings
                {
                    Async = true,
                };

                using var reader = XmlReader.Create(file.FullName, xmlSettings);

                XDocument doc;

                try
                {
                    doc = await XDocument.LoadAsync(reader, LoadOptions.SetLineInfo, CancellationToken.None);
                }
                catch (XmlException ex)
                {
                    schemaProblems.Add((file, ex.LineNumber, ex.LinePosition, XmlSeverityType.Error, ex.Message));

                    return;
                }

                // We need to access type and key info from the schema during tree construction, so we do the
                // validation manually as we go rather than relying on validation support in XmlReader or
                // XDocument. (Notably, the latter also has a very broken implementation that does not respect
                // xsi:schemaLocation, even with an XmlUrlResolver set...)
                var validator = new XmlSchemaValidator(
                    reader.NameTable,
                    xmlSettings.Schemas,
                    new XmlNamespaceManager(reader.NameTable),
                    XmlSchemaValidationFlags.ProcessSchemaLocation | XmlSchemaValidationFlags.ReportValidationWarnings)
                {
                    XmlResolver = new XmlUrlResolver(),
                    SourceUri = new Uri(reader.BaseURI),
                    LineInfoProvider = doc,
                };

                validator.ValidationEventHandler += (_, e) =>
                {
                    var ex = e.Exception;

                    schemaProblems.Add((file, ex.LineNumber, ex.LinePosition, e.Severity, e.Message));
                };

                validator.Initialize();

                var info = new XmlSchemaInfo();

                DataCenterNode ElementToNode(XElement element, DataCenterNode parent)
                {
                    var name = element.Name;

                    validator.ValidateElement(
                        name.LocalName,
                        name.NamespaceName,
                        info,
                        null,
                        null,
                        parent == root ? (string?)element.Attribute(_xsi + "schemaLocation") : null,
                        null);

                    DataCenterNode current;

                    var locked = false;

                    // Multiple threads will be creating children on the root node so we need to lock.
                    if (parent == root)
                        Monitor.Enter(root, ref locked);

                    try
                    {
                        current = parent.CreateChild(name.LocalName);
                    }
                    finally
                    {
                        if (locked)
                            Monitor.Exit(root);
                    }

                    foreach (var attr in element
                        .Attributes()
                        .Where(static a => !a.IsNamespaceDeclaration && a.Name.Namespace == XNamespace.None))
                    {
                        var attrName = attr.Name;
                        var attrValue = validator.ValidateAttribute(
                            attrName.LocalName, attrName.NamespaceName, attr.Value, null)!;

                        current.AddAttribute(attr.Name.LocalName, attrValue switch
                        {
                            int i => i,
                            float f => f,
                            string s => s,
                            bool b => b,
                            _ => 42, // Dummy value in case of validation failure.
                        });
                    }

                    validator.ValidateEndOfAttributes(null);

                    if (info.SchemaElement?.ElementSchemaType?.UnhandledAttributes is [_, ..] unhandled)
                    {
                        var names = unhandled
                            .Where(static a =>
                                a.NamespaceURI == "https://tera-arise.io/dc" && a.LocalName == "keys")
                            .Select(static a =>
                                a.Value.Split(
                                    ' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                            .Select(static arr =>
                            {
                                Array.Resize(ref arr, 4);

                                return (arr[0], arr[1], arr[2], arr[3]);
                            })
                            .LastOrDefault();

                        if (names is not (null, null, null, null))
                            current.Keys = keyCache.GetOrAdd(
                                names, static names => new(names.Item1, names.Item2, names.Item3, names.Item4));
                    }

                    foreach (var node in element.Nodes())
                    {
                        switch (node)
                        {
                            case XElement child:
                                _ = ElementToNode(child, current);
                                break;
                            case XText text:
                                validator.ValidateText(text.Value);
                                break;
                            default:
                                throw new UnreachableException();
                        }
                    }

                    var value = validator.ValidateEndElement(null)?.ToString();

                    if (!string.IsNullOrEmpty(value))
                        current.Value = value;

                    return current;
                }

                var node = ElementToNode(doc.Root!, root);

                nodes.Add((node, tup.Index));
            });

        if (!schemaProblems.IsEmpty)
        {
            foreach (var (file, line, column, severity, message) in schemaProblems
                .OrderBy(static p => p.File.FullName)
                .ThenBy(static p => p.Line)
                .ThenBy(static p => p.Column)
                .ThenBy(static p => p.Severity))
                await Terminal.ErrorLineAsync($"{file}({line},{column}): {severity}: {message}");

            throw new InvalidDataException("Data sheets contained invalid data.");
        }

        var lookup = nodes.ToDictionary(static tup => tup.Node, static tup => tup.Index);

        // Since we process data sheets in parallel (i.e. non-deterministically), the data center we now have in memory
        // will not have the correct order for the immediate children of the root node. We fix that here.
        root.SortChildren(Comparer<DataCenterNode>.Create((x, y) => lookup[x].CompareTo(lookup[y])));

        var passErrors = new List<(DataCenterNode Node, string Message)>();

        foreach (var type in typeof(ThisAssembly)
            .Assembly
            .DefinedTypes
            .Where(static type => type.IsSubclassOf(typeof(DataCenterPass))))
        {
            await Terminal.OutLineAsync($"Applying '{type.Name}'...");

            Unsafe.As<DataCenterPass>(Activator.CreateInstance(type)!)
                .Run(root, (node, message) => passErrors.Add((node, message)));
        }

        if (passErrors.Count != 0)
        {
            foreach (var (node, message) in passErrors)
            {
                var sb = new StringBuilder(node.Name);

                // TODO: AncestorsAndSelf
                foreach (var ancestor in node.Ancestors().Reverse())
                    _ = sb.Append(CultureInfo.InvariantCulture, $"/{ancestor.Name}");

                await Terminal.OutLineAsync($"{sb}: error: {message}");
            }

            throw new InvalidDataException("Data sheets contained invalid data.");
        }

        await Terminal.OutLineAsync($"Writing data center to '{options.DataCenterFile}'...");

        await using var stream = options.DataCenterFile.OpenWrite();

        await DataCenter.SaveAsync(
            root,
            stream,
            new DataCenterSaveOptions()
                .WithRevision(options.DataCenterRevision)
                .WithKey(Convert.FromHexString(options.EncryptionKey))
                .WithIV(Convert.FromHexString(options.EncryptionIV)));
    }
}
