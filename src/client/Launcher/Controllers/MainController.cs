using Arise.Client.Gateway;
using Arise.Client.Launcher.Media;
using Arise.Client.Launcher.Settings;

namespace Arise.Client.Launcher.Controllers;

internal sealed partial class MainController : LauncherController
{
    private readonly LauncherSettingsManager _launcherSettingsManager;
    private readonly MusicPlayer _musicPlayer;

    public GatewayClient Gateway { get; }

    private readonly UserSession _session;

    [ObservableProperty]
    private bool _isLoggedIn;

    [ObservableProperty]
    private bool _isVerified;

    [ObservableProperty]
    private string _currentAccountName = string.Empty;

    [ObservableProperty]
    private ViewController _currentContent;

    [ObservableProperty]
    private bool _isMusicEnabled;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsModalVisible))]
    private ModalController? _currentModalController;

    public bool IsModalVisible => CurrentModalController is not null;

    public IReadOnlyList<ViewController> Controllers { get; }

    public MainController(IServiceProvider services, MusicPlayer musicPlayer, LauncherSettingsManager launcherSettingsManager)
        : base(services)
    {
        _musicPlayer = musicPlayer;
        _currentContent = new DefaultController(services, this);
        _launcherSettingsManager = launcherSettingsManager;

        _session = services.GetService<UserSession>()!;
        _session.StatusChanged += OnSessionStatusChanged;

        Gateway = services.GetService<GatewayClient>()!;
        Gateway.BaseAddress = _launcherSettingsManager.Settings.ServerAddress;

        Controllers = new List<ViewController>
        {
            new DefaultController(services, this),
            new NewsController(services, this),
            new AccountManagementController(services, this),
            new SettingsController(services, launcherSettingsManager, this),
        }.AsReadOnly();

        IsMusicEnabled = _launcherSettingsManager.Settings.IsMusicEnabled;
    }

    private void OnSessionStatusChanged()
    {
        CurrentAccountName = _session.IsLoggedIn ? _session.AccountName! : "LOGIN";

        IsLoggedIn = _session.IsLoggedIn;
        IsVerified = _session.IsVerified && _session.IsLoggedIn;
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
    private void ShowAccountPopup()
    {
        if (_session.IsLoggedIn)
        {
            // todo: show logout
        }
        else
        {
            ShowLoginForm();

            // todo: also set the template of the modal
        }
    }

    public void StopMusic()
    {
        _musicPlayer.Stop();
    }

    public void ShowLoginForm()
    {
        CurrentModalController = new LoginModalController(Services, this);
    }

    public void ShowAccountVerificationForm()
    {
        CurrentModalController = new AccountVerificationModalController(Services, this);
    }

    [SuppressMessage("", "CA1822")]
    public void LaunchGame()
    {
    }

    [RelayCommand]
    private void Logout()
    {
        _session.Logout();
    }
}
