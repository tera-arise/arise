using System.Windows.Input;
using Arise.Client.Launcher.Media;

namespace Arise.Client.Launcher.Controllers;

public sealed class MainController : LauncherController
{
    private bool _isLoggedIn;

    public bool IsLoggedIn
    {
        get => _isLoggedIn;
        set => this.RaiseAndSetIfChanged(ref _isLoggedIn, value);
    }

    private string _currentAccountName = "LOGIN";

    public string CurrentAccountName
    {
        get => _currentAccountName;
        set => this.RaiseAndSetIfChanged(ref _currentAccountName, value);
    }

    private string _username = string.Empty;

    public string Username
    {
        get => _username;
        set => this.RaiseAndSetIfChanged(ref _username, value);
    }

    private string _password = string.Empty;

    public string Password
    {
        get => _password;
        set => this.RaiseAndSetIfChanged(ref _password, value);
    }

    private bool _rememberMe;

    public bool RememberMe
    {
        get => _rememberMe;
        set => this.RaiseAndSetIfChanged(ref _rememberMe, value);
    }

    public ICommand LoginCommand { get; }

    public ICommand RecoverPasswordCommand { get; }

    public ICommand RegisterCommand { get; }

    private readonly MusicPlayer _musicPlayer;

    public MainController(IServiceProvider services, MusicPlayer musicPlayer)
        : base(services)
    {
        _musicPlayer = musicPlayer;

        LoginCommand = ReactiveCommand.Create(LoginAsync);
        RecoverPasswordCommand = ReactiveCommand.Create(RecoverPassword);
        RegisterCommand = ReactiveCommand.Create(Register);
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
