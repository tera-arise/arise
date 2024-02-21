namespace Arise.Client.Launcher.Controllers;

internal sealed class NavigateModalMessage
{
    public Type? ModalType { get; set; }

    public NavigateModalMessage(Type? modalType)
    {
        ModalType = modalType;
    }
}
