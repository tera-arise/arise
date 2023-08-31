namespace Arise.Tools.Packer.Passes;

internal abstract class DataCenterPass
{
    public abstract void Run(DataCenterNode root, Action<DataCenterNode, string> error);
}
