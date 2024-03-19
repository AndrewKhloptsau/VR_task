using NLog;

namespace VRtask.DataWorkers.Reader
{
    internal sealed class DataReader : IDataReader
    {
        private const int ReadBufferSize = 1 << 12; //4096

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();


        public async Task ReadData(FileInfo fileInfo, CancellationToken token)
        {
            if (!fileInfo.Exists)
                return;

            var filePath = fileInfo.FullName;

            try
            {
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, ReadBufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
                using var reader = new StreamReader(fileStream);

                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync(token);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Cannot process file {0}. Exception {1}", filePath, ex);
            }
        }
    }
}