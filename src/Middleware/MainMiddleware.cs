using NLog;
using VRtask.Database;
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

        private readonly List<IDataLoader> _loaders = new(MaxLoadersCount);
        private readonly List<IDataReader> _readers = new(MaxLoadersCount);

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly IFileProcessQueue _fileQueue = new FileProcessQueue();
        private readonly IDatabaseContext _dbWorker = new DatabaseContext();

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
            foreach (var reader in _readers)
            {
                reader.SaveContentInfo -= _dbWorker.SaveContentInfo;
                reader.SaveBoxInfo -= _dbWorker.SaveBoxInfo;
            }

            foreach (var loader in _loaders)
                loader?.Dispose();

            _fileQueue?.Dispose();
            _watcher?.Dispose();
            _dbWorker?.Dispose();
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

            reader.SaveContentInfo += _dbWorker.SaveContentInfo;
            reader.SaveBoxInfo += _dbWorker.SaveBoxInfo;

            var loader = new DataLoader(_fileQueue, reader);

            _loaders.Add(loader);
            _readers.Add(reader);

            _logger.Info("Loader #{0} has been added.", _loaders.Count);

            return true;
        }
    }
}
