// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Arise.Client.Launcher.Controllers.Modals;

internal sealed class AccountDeletionModalController : ModalController
{
    public AccountDeletionModalController(IServiceProvider services, MainController mainController)
        : base(services, mainController)
    {
    }
}
