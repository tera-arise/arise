namespace Arise.Client.Launcher.Controllers.Modals;

internal sealed class NavigateModalMessage
{
    public Type? ModalType { get; set; }

    public NavigateModalMessage(Type? modalType)
    {
        ModalType = modalType;
    }
}
