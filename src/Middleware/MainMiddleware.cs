using NLog;
using VRtask.DataWorkers.Loader;
using VRtask.DataWorkers.Reader;
using VRtask.FileWorkers;
using VRtask.FileWorkers.FileQueue;

namespace VRtask.Middleware
{
    internal sealed class MainMiddleware : IDisposable
    {
        private const int ActiveLoadersCount = 2; //temp value for test data
        private const int MaxLoadersCount = 4; // machine overload protection
        
        private readonly List<IDataLoader> _loaders = new(1 << 4);

        private readonly IFileProcessQueue _fileQueue = new FileProcessQueue();
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private FileWatcher _watcher;


        public void Init(string targerFolder)
        {
            if (!TryInitWatcher(targerFolder))
                return;

            //you can add multiple loaders if it necessary
            for (int i = 0; i < ActiveLoadersCount; ++i)
                if (!TryInitLoader())
                    return;
        }

        public void Run()
        {
            foreach (var loader in _loaders)
                _ = loader.StartLoad();
        }

        public void Dispose()
        {
            foreach (var loader in _loaders)
                loader?.Dispose();

            _fileQueue?.Dispose();
            _watcher?.Dispose();
        }

        private bool TryInitWatcher(string targetFolder)
        {
            var folderInfo = new DirectoryInfo(targetFolder);

            if (folderInfo.Exists)
            {
                _watcher = new FileWatcher(targetFolder, _fileQueue);
                return true;
            }
            else
            {
                _logger.Error("Folder not exists {0}", targetFolder);
                return false;
            }
        }

        private bool TryInitLoader()
        {
            if (_loaders.Count > MaxLoadersCount)
            {
                _logger.Error("Loaders limit is reached. Max loaders count is {0}", MaxLoadersCount);
                return false;
            }

            var reader = new DataReader();
            var loader = new DataLoader(_fileQueue, reader);

            _loaders.Add(loader);
            _logger.Info("Loader #{0} has been added.", _loaders.Count);

            return true;
        }
    }
}
