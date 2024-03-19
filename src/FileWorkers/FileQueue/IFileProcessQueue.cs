namespace VRtask.FileWorkers.FileQueue
{
    public interface IFileProcessQueue : IDisposable
    {
        void PushFilePath(string path);

        bool TryGetFileInfo(out FileInfo info);
    }
}