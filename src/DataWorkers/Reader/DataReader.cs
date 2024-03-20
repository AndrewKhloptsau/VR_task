using NLog;
using System.Collections.Frozen;

namespace VRtask.DataWorkers.Reader
{
    internal enum DataType : byte
    {
        Unknown = 0,
        Box,
        Content,
    }


    internal sealed class DataReader : IDataReader
    {
        private const int ReadBufferSize = 1 << 12; //4096

        private static readonly FrozenDictionary<string, DataType> _lineTypeMapper = new Dictionary<string, DataType>()
        {
            ["HDR"] = DataType.Box,
            ["LINE"] = DataType.Content,
        }.ToFrozenDictionary(); //fast lookup for immutable dictionary

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private string _lastSupplierId;

        public event Action<string, string, int> SaveContentInfo;
        public event Action<string, string> SaveBoxInfo;


        public async Task ReadData(FileInfo fileInfo, CancellationToken token)
        {
            if (!fileInfo.Exists)
                return;

            var filePath = fileInfo.FullName;

            try
            {
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, ReadBufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
                using var reader = new StreamReader(fileStream);

                int lineNumber = 0;

                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync(token);
                    lineNumber++;

                    if (!TryParseString(line, out var error))
                        _logger.Error("Parsing error on line #{0}: {1}", lineNumber, error);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Cannot process file {0}. Exception {1}", filePath, ex);
            }
        }


        private bool TryParseString(string str, out string error) =>
            GetLineType(str) switch
            {
                DataType.Content => TryParseAndSaveContentInfo(str, out error),
                DataType.Box => TryParseAndSaveBoxInfo(str, out error),

                _ => SetUnknownError(str, out error)
            };

        private bool TryParseAndSaveBoxInfo(string str, out string error)
        {
            var parts = GetParts(str);

            if (parts.Length != 3)
                return SetInvalidStringFormatError(str, out error);

            var supplierIdStr = parts[1];
            var boxIdStr = parts[2];

            if (!TryParseIdentifier(supplierIdStr, out _lastSupplierId))
                return SetInvalidIdError(supplierIdStr, out error);

            if (!TryParseIdentifier(boxIdStr, out var boxId))
                return SetInvalidIdError(boxIdStr, out error);

            SaveBoxInfo?.Invoke(_lastSupplierId, boxId);

            error = null;

            return true;
        }

        private bool TryParseAndSaveContentInfo(string str, out string error)
        {
            var parts = GetParts(str);

            if (parts.Length != 4)
                return SetInvalidStringFormatError(str, out error);

            var isbnStr = parts[2];
            var qtyStr = parts[3];

            if (!TryParseIdentifier(isbnStr, out var isbn))
                return SetInvalidIdError(isbnStr, out error);

            if (!int.TryParse(qtyStr, out var qty))
                return SetInvalidNumberError(qtyStr, out error);

            if (!string.IsNullOrEmpty(_lastSupplierId))
                SaveContentInfo?.Invoke(_lastSupplierId, isbn, qty);

            error = null;
            return true;
        }


        private static bool TryParseIdentifier(string str, out string id)
        {
            // custom checking for correct identifier (string length, correct chars etc.)

            id = str;
            return true;
        }

        private static bool SetInvalidIdError(string str, out string error)
        {
            error = $"Invalid identifier {str}";
            return false;
        }

        private static bool SetInvalidNumberError(string str, out string error)
        {
            error = $"Invalid number {str}";
            return false;
        }

        private static bool SetInvalidStringFormatError(string str, out string error)
        {
            error = $"Invalid format {str}";
            return false;
        }

        private static bool SetUnknownError(string str, out string error)
        {
            error = $"Unknown line type {str}";
            return false;
        }

        private static DataType GetLineType(ReadOnlySpan<char> span) =>
            _lineTypeMapper.TryGetValue(span.ToString(), out var type) ? type : DataType.Unknown;

        private static string[] GetParts(string str) =>
            str.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    }
}