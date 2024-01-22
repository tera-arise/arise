using System.Windows.Input;
using Arise.Client.Launcher.Settings;
using Avalonia.Data;

namespace Arise.Client.Launcher.Controllers;

public sealed class SettingsController : LauncherController
{
    private readonly LauncherSettingsManager _settingsManager;
    private string _serverAddress = string.Empty;

    public ICommand ApplyCommand { get; }

    public ICommand CancelCommand { get; }

    public string ServerAddress
    {
        get => _serverAddress;
        set
        {
            try
            {
                using var mockClient = new HttpClient();
                mockClient.BaseAddress = new Uri(value);
                _ = this.RaiseAndSetIfChanged(ref _serverAddress, value);
            }
            catch (Exception ex)
            {
                throw new DataValidationException($"{ex.Message}");
            }
        }
    }

    public SettingsController(IServiceProvider services, LauncherSettingsManager launcherSettingsManager)
        : base(services)
    {
        _settingsManager = launcherSettingsManager;
        _serverAddress = _settingsManager.Settings.ServerAddress?.ToString() ?? string.Empty;

        ApplyCommand = ReactiveCommand.Create(Apply);
        CancelCommand = ReactiveCommand.Create(Cancel);
    }

    private void Cancel()
    {
        // todo: return without updating the settings
    }

    private void Apply()
    {
        // todo: commit changes to the manager

        _settingsManager.Save();
    }
}
