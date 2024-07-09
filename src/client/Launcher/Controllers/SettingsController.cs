using Arise.Client.Launcher.Controls;
using Arise.Client.Launcher.Settings;
using Material.Icons;

namespace Arise.Client.Launcher.Controllers;

internal sealed partial class SettingsController : ViewController
{
    private readonly LauncherSettingsManager _settingsManager;

    [ObservableProperty]
    [UriValidation]
    private string _serverAddress;

    public static bool ShowServerUriSetting => ThisAssembly.ServerUris == null;

    public override MaterialIconKind IconKind => MaterialIconKind.Settings;

    public SettingsController(IServiceProvider services, LauncherSettingsManager launcherSettingsManager, MainController mainController)
        : base(services, mainController)
    {
        _settingsManager = launcherSettingsManager;
        _serverAddress = _settingsManager.Settings.ServerAddress?.ToString() ?? string.Empty;
    }

    [RelayCommand]
    [SuppressMessage("", "CA1822")]
    private void Cancel()
    {
        // todo: return without updating the settings
    }

    [RelayCommand]
    private void Apply()
    {
        // todo: commit changes to the manager

        _settingsManager.Save();
    }
}
