namespace VRtask.DataWorkers.Reader
{
    public interface IDataReader
    {
        Task ReadData(FileInfo fileInfo, CancellationToken token);
    }
}