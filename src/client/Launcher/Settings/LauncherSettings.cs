namespace Arise.Client.Launcher.Settings;

/// <summary>
/// The class containing the actual user settings. This will be (de)serialized and exposed by the <see cref="LauncherSettingsManager"/>.
/// </summary>
internal sealed class LauncherSettings
{
    public Uri? ServerAddress { get; set; }

    public bool IsMusicEnabled { get; set; } = true;

    public string LastLoggedInAccount { get; set; } = string.Empty;
}
