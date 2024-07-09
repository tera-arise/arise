// SPDX-License-Identifier: AGPL-3.0-or-later
using System.Collections.ObjectModel;
using Arise.Client.Gateway;
using Arise.Client.Launcher.Controllers.Modals;
using Arise.Client.Launcher.Media;
using Arise.Client.Launcher.Settings;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Messaging;

namespace Arise.Client.Launcher.Controllers;

internal sealed partial class MainController : LauncherController
{
    private readonly LauncherSettingsManager _launcherSettingsManager;
    private readonly MusicPlayer _musicPlayer;
    private readonly UserSession _session;

    [ObservableProperty]
    private bool _isLoggedIn;
    [ObservableProperty]
    private bool _isVerified;
    [ObservableProperty]
    private bool _isChangingEmail;
    [ObservableProperty]
    private string _currentAccountName = string.Empty;
    [ObservableProperty]
    private ViewController _currentContent;
    [ObservableProperty]
    private bool _isMusicEnabled;

    [ObservableProperty]
    private DateTime? _deletionDue;

    [NotifyPropertyChangedFor(nameof(IsModalVisible))]
    [ObservableProperty]
    private ModalController? _currentModalController;

    public ObservableCollection<ViewController> Controllers { get; }

    public GatewayClient Gateway { get; }

    public bool IsModalVisible => CurrentModalController is not null;

    public MainController(IServiceProvider services)
        : base(services)
    {
        _musicPlayer = services.GetService<MusicPlayer>()!;
        _launcherSettingsManager = services.GetService<LauncherSettingsManager>()!;

        _session = services.GetService<UserSession>()!;
        _session.StatusChanged += OnSessionStatusChanged;
        _session.LoggedIn += OnLogin;
        _session.LoggedOut += OnLogout;

        Gateway = services.GetService<GatewayClient>()!;
        Gateway.BaseAddress = _launcherSettingsManager.Settings.ServerAddress;

        Controllers =
        [
            new DefaultController(services, this),
            new NewsController(services, this),
            new SettingsController(services, _launcherSettingsManager, this),
        ];

        CurrentContent = Controllers[0];

        IsMusicEnabled = _launcherSettingsManager.Settings.IsMusicEnabled;

        WeakReferenceMessenger.Default.Register<NavigateModalMessage>(this, OnNavigateModalMessage);
    }

    [SuppressMessage("", "CA1822")]
    public async Task LaunchGameAsync()
    {
        // mock stuff ----

        var closebtn = new Button()
        {
            Content = "Ok",
            HorizontalAlignment = HorizontalAlignment.Center,
        };

        var sp = new StackPanel
        {
            Spacing = 20,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(20),
        };

        using var c = new HttpClient();

        var bytes = await c.GetByteArrayAsync(new Uri("https://i.imgur.com/TL58G2m.png"))
            .ConfigureAwait(true);

        sp.Children.Add(
            new Image
            {
                Source = new Bitmap(new MemoryStream(bytes)),
                VerticalAlignment = VerticalAlignment.Center,
                Stretch = Stretch.Uniform,
                MaxWidth = 300,
            });

        sp.Children.Add(closebtn);
        var w = new Window
        {
            Content = sp,
            SizeToContent = SizeToContent.WidthAndHeight,
        };

        closebtn.Click += (_, __) => w.Close();

        var lt = (IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!;
        await w.ShowDialog(lt!.MainWindow!).ConfigureAwait(false);
    }

    [RelayCommand]
    private void Logout()
    {
        _session.Logout();
        ModalController.CloseModal();
    }

    [RelayCommand]
    private void ManageAccount()
    {
        var acm = Controllers.FirstOrDefault(x => x is AccountManagementController)!;
        CurrentContent = acm;
    }

    private void OnLogin(string accountName)
    {
        _launcherSettingsManager.Settings.LastLoggedInAccount = accountName!;
        _launcherSettingsManager.Save();

        CurrentAccountName = accountName;
        IsLoggedIn = true;

        if (!Controllers.OfType<AccountManagementController>().Any())
            Controllers.Add(new AccountManagementController(Services, this));
    }

    private void OnLogout()
    {
        IsLoggedIn = false;
        CurrentAccountName = string.Empty;

        var acm = Controllers.FirstOrDefault(x => x is AccountManagementController)!;
        _ = Controllers.Remove(acm);
    }

    private void OnSessionStatusChanged()
    {
        IsVerified = _session.IsVerified && _session.IsLoggedIn;
        IsChangingEmail = _session.IsChangingEmail;
        DeletionDue = _session.DeletionDue;
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

    private void OnNavigateModalMessage(object recipient, NavigateModalMessage message)
    {
        CurrentModalController = message.ModalType is null
            ? null
            : (ModalController?)Activator.CreateInstance(message.ModalType, Services, this);
    }

    public void StopMusic()
    {
        _musicPlayer.Stop();
    }
}
