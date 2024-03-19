using NLog;
using System.Collections.Concurrent;

namespace VRtask.FileWorkers.FileQueue
{
    internal class FileProcessQueue : IFileProcessQueue
    {
        private const int MaxQueueSize = 100_000; // protect from memory leak

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly ConcurrentQueue<FileInfo> _queue = new();

        public void PushFilePath(string path)
        {
            var fileInfo = new FileInfo(path);

            if (fileInfo.Exists)
            {
                _queue.Enqueue(fileInfo);

                var queueOverhead = _queue.Count - MaxQueueSize;

                while (_queue.Count > MaxQueueSize)
                    if (!_queue.TryDequeue(out _))
                        break;

                if (queueOverhead > 0)
                    _logger.Error("Queue overhead {0} items", queueOverhead);
            }
            else
                FileNotFoundError(path);
        }

        public bool TryGetFileInfo(out FileInfo info)
        {
            while (_queue.TryDequeue(out info))
            {
                if (info.Exists)
                    return true;

                FileNotFoundError(info.FullName);
            }

            return false;
        }

        public void Dispose() => _queue.Clear();


        private void FileNotFoundError(string path) => _logger.Error("File not found {0}", path); // log template faster than string intorpalation
    }
}