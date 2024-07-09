using System.Windows.Input;
using Arise.Client.Gateway;
using Arise.Client.Launcher.Media;
using Arise.Client.Launcher.Settings;
using Avalonia.Data;

namespace Arise.Client.Launcher.Controllers;

public sealed class MainController : LauncherController
{
    private readonly LauncherSettingsManager _launcherSettingsManager;
    private readonly GatewayClient _gatewayClient;
    private bool _isLoggedIn;
    private string _currentAccountName = "LOGIN";
    private string _username = string.Empty;
    private string _password = string.Empty;
    private bool _rememberMe;
    private bool _isModalVisible;
    private string _serverAddress;

    public bool IsLoggedIn
    {
        get => _isLoggedIn;
        set => this.RaiseAndSetIfChanged(ref _isLoggedIn, value);
    }

    public string CurrentAccountName
    {
        get => _currentAccountName;
        set => this.RaiseAndSetIfChanged(ref _currentAccountName, value);
    }

    public string Username
    {
        get => _username;
        set => this.RaiseAndSetIfChanged(ref _username, value);
    }

    public string Password
    {
        get => _password;
        set => this.RaiseAndSetIfChanged(ref _password, value);
    }

    public bool RememberMe
    {
        get => _rememberMe;
        set => this.RaiseAndSetIfChanged(ref _rememberMe, value);
    }

    public bool IsModalVisible
    {
        get => _isModalVisible;
        set => this.RaiseAndSetIfChanged(ref _isModalVisible, value);
    }

    public string ServerAddress
    {
        get => _serverAddress;
        set
        {
            try
            {
                var svc = Services.GetService<GatewayClient>()!;
                svc.BaseAddress = new Uri(value);
                _ = this.RaiseAndSetIfChanged(ref _serverAddress, value);
            }
            catch (Exception ex)
            {
                throw new DataValidationException($"{ex.Message}");
            }
        }
    }

    public ICommand LoginCommand { get; }

    public ICommand RecoverPasswordCommand { get; }

    public ICommand RegisterCommand { get; }

    public ICommand ShowAccountPopupCommand { get; }

    public ICommand CloseModalCommand { get; }

    private readonly MusicPlayer _musicPlayer;

    public MainController(IServiceProvider services, MusicPlayer musicPlayer, LauncherSettingsManager launcherSettingsManager)
        : base(services)
    {
        _musicPlayer = musicPlayer;

        _launcherSettingsManager = launcherSettingsManager;

        _gatewayClient = Services.GetService<GatewayClient>()!; // todo: inject this? it requires GatewayClient to be public tho
        _gatewayClient.BaseAddress = _launcherSettingsManager.Settings.ServerAddress;

        _serverAddress = _launcherSettingsManager.Settings.ServerAddress?.ToString() ?? string.Empty;

        LoginCommand = ReactiveCommand.Create(LoginAsync);
        RecoverPasswordCommand = ReactiveCommand.Create(RecoverPassword);
        RegisterCommand = ReactiveCommand.Create(Register);
        ShowAccountPopupCommand = ReactiveCommand.Create(ShowAccountPopup);
        CloseModalCommand = ReactiveCommand.Create(CloseModal);
    }

    private void CloseModal()
    {
        IsModalVisible = false; // todo: checks
    }

    private void ShowAccountPopup()
    {
        if (IsLoggedIn)
        {
            // todo: show logout
        }
        else
        {
            IsModalVisible = true;

            // todo: also set the template of the modal
        }
    }

    public void PlayMusic()
    {
        _musicPlayer.Play();
    }

    public void StopMusic()
    {
        _musicPlayer.Stop();
    }

    private void Register()
    {
        // todo
    }

    private void RecoverPassword()
    {
        // todo
    }

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
                var resp = await _gatewayClient.Rest.AuthenticateAccountAsync(Username, Password).ConfigureAwait(true);

                if (resp.IsSuccessStatusCode)
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
            }
        }
        else
        {
            // todo: warn?
        }
    }
}
