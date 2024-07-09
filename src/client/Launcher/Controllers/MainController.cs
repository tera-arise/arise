#define FORCE_LOGIN

using Arise.Client.Gateway;
using Arise.Client.Launcher.Media;
using Arise.Client.Launcher.Settings;

namespace Arise.Client.Launcher.Controllers;

public sealed partial class MainController : LauncherController
{
    private readonly LauncherSettingsManager _launcherSettingsManager;
    private readonly GatewayClient _gatewayClient;
    private readonly MusicPlayer _musicPlayer;

    [ObservableProperty]
    private bool _isLoggedIn;

    [ObservableProperty]
    private string _currentAccountName = "LOGIN";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _username = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _rememberMe;

    [ObservableProperty]
    private bool _isModalVisible;

    [ObservableProperty]
    private ViewController _currentContent;

    [ObservableProperty]
    private bool _isMusicEnabled;

    private bool CanExecuteLogin => !string.IsNullOrEmpty(Username)
                                 && !string.IsNullOrEmpty(Password);

    public IReadOnlyList<ViewController> Controllers { get; }

    public MainController(IServiceProvider services, MusicPlayer musicPlayer, LauncherSettingsManager launcherSettingsManager)
        : base(services)
    {
        _musicPlayer = musicPlayer;

        Controllers = new List<ViewController>
        {
            new DefaultController(services, this),
            new NewsController(services, this),
            new AccountManagementController(services, this),
            new SettingsController(services, launcherSettingsManager, this),
        }.AsReadOnly();

        _currentContent = new DefaultController(services, this);
        _launcherSettingsManager = launcherSettingsManager;

        _gatewayClient = Services.GetService<GatewayClient>()!; // todo: inject this? it requires GatewayClient to be public tho
        _gatewayClient.BaseAddress = _launcherSettingsManager.Settings.ServerAddress;

        IsMusicEnabled = _launcherSettingsManager.Settings.IsMusicEnabled;
    }

    partial void OnIsMusicEnabledChanged(bool value)
    {
        if (value)
            _musicPlayer.Play();
        else
            _musicPlayer.Stop();

        _launcherSettingsManager.Settings.IsMusicEnabled = value;
        _launcherSettingsManager.Save();
    }

    [RelayCommand]
    [SuppressMessage("", "CA1822")]
    private void OpenSettings()
    {
        // CurrentContent = new SettingsController(Services, _launcherSettingsManager);
    }

    [RelayCommand]
    private void CloseModal()
    {
        IsModalVisible = false; // todo: checks
    }

    [RelayCommand]
    private void ShowAccountPopup()
    {
        if (IsLoggedIn)
        {
            // todo: show logout
        }
        else
        {
            ShowLoginForm();

            // todo: also set the template of the modal
        }
    }

    [RelayCommand]
    [SuppressMessage("", "CA1822")]
    private void Register()
    {
        // todo
    }

    [RelayCommand]
    [SuppressMessage("", "CA1822")]
    private void RecoverPassword()
    {
        // todo
    }

    [RelayCommand(CanExecute = nameof(CanExecuteLogin))]
    private async Task LoginAsync()
    {
        // todo: add an IsLoggingIn state to signal the user that the request is being processed
        // maybe bind it to a spinning indicator

        // todo: use RememberMe setting (save it locally?)

        if (!IsLoggedIn)
        {
            // todo: catch something?
            try
            {
#if FORCE_LOGIN // find a better way of doing this when you don't have a backend
                if (await Task.FromResult(true).ConfigureAwait(true))
#else
                var resp = await _gatewayClient.Rest
                    .AuthenticateAccountAsync(Username, Password).ConfigureAwait(true);

                if (resp.IsSuccessStatusCode)
#endif
                {
                    IsLoggedIn = true;
                    CurrentAccountName = Username;

                    // todo: handle all the data in the response (ban, mail verification, etc)
                }
                else
                {
                    // todo: handle login fail
                }
            }
            finally
            {
                // clear the password as soon as it's been sent
                Password = string.Empty;
                IsModalVisible = false;
            }
        }
        else
        {
            // todo: warn?
        }
    }

    public void StopMusic()
    {
        _musicPlayer.Stop();
    }

    public void ShowLoginForm()
    {
        IsModalVisible = true;
    }

    [SuppressMessage("", "CA1822")]
    public void LaunchGame()
    {
    }
}
