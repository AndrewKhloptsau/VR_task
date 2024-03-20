namespace VRtask.DataWorkers.Reader
{
    public interface IDataReader
    {
        Task ReadData(FileInfo fileInfo, CancellationToken token);

        event Action<string, string, int> SaveContentInfo;
        event Action<string, string> SaveBoxInfo;
    }
}