using Arise.Client.Launcher.Settings;

namespace Arise.Client.Launcher.Controllers;

public sealed partial class SettingsController : ViewController
{
    private readonly LauncherSettingsManager _settingsManager;

    [ObservableProperty]
    [UriValidation]
    private string _serverAddress = string.Empty;

    public SettingsController(IServiceProvider services, LauncherSettingsManager launcherSettingsManager)
        : base(services)
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
