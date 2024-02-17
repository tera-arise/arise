using dnlib.DotNet;

namespace Arise.Server.Bridge;

internal sealed class BridgeModulePatchingPass : BridgeModulePass
{
    [SuppressMessage("", "CA5394")]
    public override void Run(ModuleDefMD module, BridgeModuleKind kind, Random rng, GameOptions options)
    {
        foreach (var method in new MemberFinder().FindAll(module).MethodDefs.Keys)
        {
            if (method.CustomAttributes.Find(typeof(ObfuscationAttribute).FullName) == null)
                continue;

            var insns = method.Body.Instructions;

            void PatchStringMarker(string value)
            {
                var insn = insns.First(static insn => insn.OpCode.Code == Code.Ldstr && (string)insn.Operand == "xyz");

                insn.Operand = value;
            }

            void PatchInt32Marker(int value)
            {
                var insn = insns.First(static insn => insn.IsLdcI4() && insn.GetLdcI4Value() == 42);

                insn.OpCode = OpCodes.Ldc_I4;
                insn.Operand = value;
            }

            switch (((string)method.DeclaringType.Name, (string)method.Name))
            {
                case ("GameProtection", "Initialize") when kind == BridgeModuleKind.Normal:
                {
                    insns.Clear();
                    insns.Add(Instruction.Create(OpCodes.Ret));

                    break;
                }

                case ("GameProtection", "GetIssueTime"):
                {
                    PatchStringMarker(DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));

                    break;
                }

                case ("GameProtection", "GetValidDuration"):
                {
                    PatchStringMarker(options.ModuleValidityTime.ToTimeSpan().ToString());

                    break;
                }

                case ("GameProtection", "GetExitDelay"):
                {
                    PatchInt32Marker(rng.Next(1_000, 60_001));

                    break;
                }

                case ("GameProtection", "GetExitStatus"):
                {
                    PatchInt32Marker(rng.Next(int.MinValue, int.MaxValue));

                    break;
                }

                case ("GameProtectionTask", "GetCheckInterval"):
                {
                    PatchInt32Marker(rng.Next(60_000, 600_001));

                    break;
                }

                case ("PatchableBridgeDataComponent", "InitializeKey"):
                {
                    foreach (var b in ThisAssembly.DataCenterKey.Span)
                        PatchInt32Marker(b);

                    break;
                }

                case ("PatchableBridgeDataComponent", "InitializeIV"):
                {
                    foreach (var b in ThisAssembly.DataCenterIV.Span)
                        PatchInt32Marker(b);

                    break;
                }

                case ("PatchableBridgeProtocolComponent", "InitializeCodes"):
                {
                    var template = insns
                        .Where(static insn => insn.OpCode.Code != Code.Ret)
                        .ToArray();

                    var codes = new HashSet<int>();

                    foreach (var value in Enum.GetValues<GamePacketCode>())
                    {
                        var sequence = template
                            .Select(static insn => insn.Clone())
                            .ToArray();

                        var insn1 = sequence.First(
                            static insn => insn.IsLdcI4() && insn.GetLdcI4Value() == (int)default(GamePacketCode));

                        insn1.OpCode = OpCodes.Ldc_I4;
                        insn1.Operand = (int)value;

                        int code;

                        while (!codes.Add(code = rng.Next(ushort.MaxValue + 1)))
                        {
                            // Prevent duplicate packet codes.
                        }

                        var insn2 = sequence.First(static insn => insn.IsLdcI4() && insn.GetLdcI4Value() == 42);

                        insn2.OpCode = OpCodes.Ldc_I4;
                        insn2.Operand = code;

                        foreach (var insn in sequence)
                            insns.Add(insn.Clone());
                    }

                    insns.Add(Instruction.Create(OpCodes.Ret));

                    break;
                }
            }
        }
    }
}
