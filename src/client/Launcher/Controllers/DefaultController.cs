// SPDX-License-Identifier: AGPL-3.0-or-later
using Arise.Client.Launcher.Controllers.Modals;
using CommunityToolkit.Mvvm.Messaging;
using Material.Icons;

namespace Arise.Client.Launcher.Controllers;

internal sealed partial class DefaultController : ViewController
{
    private readonly UserSession _session;

    public override MaterialIconKind IconKind => MaterialIconKind.Home;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PlayCommand))]
    private bool _canPlay = true;

    public DefaultController(IServiceProvider services, MainController mainController)
        : base(services, mainController)
    {
        _session = services.GetService<UserSession>()!;

        WeakReferenceMessenger.Default.Register<NavigateModalMessage>(this, OnNavigateModalMessage);
    }

    private void OnNavigateModalMessage(object recipient, NavigateModalMessage message)
    {
        CanPlay = message.ModalType is null;
    }

    [RelayCommand(CanExecute = nameof(CanPlay))]
    private Task PlayAsync()
    {
        if (!_session.IsLoggedIn)
        {
            ModalController.NavigateTo<LoginModalController>();
        }
        else if (!_session.IsVerified)
        {
            ModalController.NavigateTo<AccountVerificationModalController>();
        }
        else
        {
            return MainController.LaunchGameAsync(); // todo: use message?
        }

        return Task.CompletedTask;
    }
}
