using VRtask.DataWorkers.Reader;
using VRtask.FileWorkers.FileQueue;

namespace VRtask.DataWorkers.Loader
{
    internal sealed class DataLoader : IDataLoader
    {
        private const int ScanDelay = 1000; // 1 sec

        private readonly CancellationTokenSource _tokenSource = new();
        private readonly IFileProcessQueue _fileQueue;
        private readonly IDataReader _reader;


        internal DataLoader(IFileProcessQueue fileQueue, IDataReader reader)
        {
            _fileQueue = fileQueue;
            _reader = reader;
        }


        public async Task StartLoad()
        {
            var token = _tokenSource.Token;

            while (!_tokenSource.IsCancellationRequested)
            {
                if (_fileQueue.TryGetFileInfo(out var fileInfo))
                    await _reader.ReadData(fileInfo, token);
                else
                    await Task.Delay(ScanDelay, token);
            }
        }

        public void Dispose()
        {
            _tokenSource.Cancel();
        }
    }
}