// SPDX-License-Identifier: AGPL-3.0-or-later
namespace Arise.Client.Launcher.Settings;

/// <summary>
/// This should handle load/save operations for settings and hold a reference to them.
/// </summary>
public sealed class LauncherSettingsManager
{
    private readonly string _path;

    internal LauncherSettings Settings { get; private set; } = new();

    public LauncherSettingsManager()
    {
        var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _path = Path.Combine(appdata, "Arise", "Launcher", "settings.json");

        Load();
    }

    /// <summary>
    /// Deserializes the settings as a <see cref="LauncherSettings"/> from the default path to the <see cref="Settings"/> property.
    /// </summary>
    private void Load()
    {
        if (File.Exists(_path))
        {
            // todo: handle read fail
            var file = File.ReadAllText(_path);

            Settings = JsonSerializer.Deserialize<LauncherSettings>(file)
                ?? throw new InvalidOperationException("Failed to deserialize user settings from JSON.");
        }
        else
        {
            // save default
            Save();
        }
    }

    /// <summary>
    /// Serializes the current <see cref="Settings"/> value to JSON and saves it to the default path.
    /// </summary>
    public void Save()
    {
        // todo: handle exceptions

        if (!Directory.Exists(Path.GetDirectoryName(_path)))
        {
            _ = Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        }

        File.WriteAllText(_path, JsonSerializer.Serialize(Settings));
    }
}
