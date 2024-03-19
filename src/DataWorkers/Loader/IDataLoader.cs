namespace VRtask.DataWorkers.Loader
{
    public interface IDataLoader : IDisposable
    {
        Task StartLoad();
    }
}