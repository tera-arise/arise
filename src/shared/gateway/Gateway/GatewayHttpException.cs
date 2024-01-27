namespace Arise.Gateway;

public sealed class GatewayHttpException : Exception
{
    public GatewayHttpException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
