using System.Windows.Input;
using Arise.Client.Launcher.Media;

namespace Arise.Client.Launcher.Controllers;

public sealed class MainController : LauncherController
{
    private bool _isLoggedIn;
    private string _currentAccountName = "LOGIN";
    private string _username = string.Empty;
    private string _password = string.Empty;
    private bool _rememberMe;
    private bool _isModalVisible;

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

    public ICommand LoginCommand { get; }

    public ICommand RecoverPasswordCommand { get; }

    public ICommand RegisterCommand { get; }

    public ICommand ShowAccountPopupCommand { get; }

    public ICommand CloseModalCommand { get; }

    private readonly MusicPlayer _musicPlayer;

    public MainController(IServiceProvider services, MusicPlayer musicPlayer)
        : base(services)
    {
        _musicPlayer = musicPlayer;

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
        if (!IsLoggedIn)
        {
            // todo: call GatewayClient

            IsLoggedIn = true;
            CurrentAccountName = "Foglio";
        }

        await Task.CompletedTask.ConfigureAwait(true);
    }
}
