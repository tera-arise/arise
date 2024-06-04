// SPDX-License-Identifier: AGPL-3.0-or-later
namespace Arise.Client.Launcher.Controllers.Modals;

internal sealed class NavigateModalMessage
{
    public Type? ModalType { get; set; }

    public NavigateModalMessage(Type? modalType)
    {
        ModalType = modalType;
    }
}
