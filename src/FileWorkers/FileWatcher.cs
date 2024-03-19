using NLog;
using VRtask.FileWorkers.FileQueue;

namespace VRtask.FileWorkers
{
    internal sealed class FileWatcher : IDisposable
    {
        private const string DefaultFileFilter = "*.txt";

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IFileProcessQueue _fileQueue;
        private readonly FileSystemWatcher _watcher;


        internal FileWatcher(string targetPath, IFileProcessQueue fileQueue)
        {
            _fileQueue = fileQueue;

            _watcher = new FileSystemWatcher(targetPath, DefaultFileFilter)
            {
                EnableRaisingEvents = true
            };

            _watcher.Created += LoadNewFile;
            _watcher.Error += LogError;
            //also OnChanged handler can be implemented
        }


        private void LogError(object _, ErrorEventArgs e) => _logger.Error(e?.GetException());

        private void LoadNewFile(object sender, FileSystemEventArgs e)
        {
            if (e?.FullPath is null)
                _logger.Error("File path cannot be null");
            else
                _fileQueue.PushFilePath(e.FullPath);
        }

        public void Dispose()
        {
            _watcher.Created -= LoadNewFile;
            _watcher.Error -= LogError;

            _watcher.Dispose();
        }
    }
}