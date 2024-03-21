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
        private string _lastBoxId;

        public event Action<string, string, string, int> SaveContentInfo;
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

                    if (string.IsNullOrEmpty(line) || TryParseString(line, out var error))
                        continue;

                    _logger.Error("Parsing error on line #{0}: {1}", lineNumber, error);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Cannot process file {0}. Exception {1}", filePath, ex);
            }
        }


        private bool TryParseString(string str, out string error)
        {
            var parts = GetParts(str);
            var itemType = parts[0];

            return GetLineType(itemType) switch
            {
                DataType.Content => TryParseAndSaveContentInfo(str, parts, out error),
                DataType.Box => TryParseAndSaveBoxInfo(str, parts, out error),

                _ => SetUnknownError(str, out error)
            };
        }

        private bool TryParseAndSaveBoxInfo(string str, string[] parts, out string error)
        {
            if (parts.Length != 3)
                return SetInvalidStringFormatError(str, out error);

            var supplierIdStr = parts[1];
            var boxIdStr = parts[2];

            if (!TryParseIdentifier(supplierIdStr, out var supplierId))
                return SetInvalidIdError(supplierIdStr, out error);

            if (!TryParseIdentifier(boxIdStr, out _lastBoxId))
                return SetInvalidIdError(boxIdStr, out error);

            SaveBoxInfo?.Invoke(supplierId, _lastBoxId);

            error = null;

            return true;
        }

        private bool TryParseAndSaveContentInfo(string str, string[] parts, out string error)
        {
            if (parts.Length != 4)
                return SetInvalidStringFormatError(str, out error);

            var poNumberStr = parts[1];
            var isbnStr = parts[2];
            var qtyStr = parts[3];

            if (!TryParseIdentifier(poNumberStr, out var poNumber))
                return SetInvalidIdError(poNumberStr, out error);

            if (!TryParseIdentifier(isbnStr, out var isbn))
                return SetInvalidIdError(isbnStr, out error);

            if (!int.TryParse(qtyStr, out var qty))
                return SetInvalidNumberError(qtyStr, out error);

            if (!string.IsNullOrEmpty(_lastBoxId))
                SaveContentInfo?.Invoke(_lastBoxId, poNumber, isbn, qty);

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


        private static DataType GetLineType(string typeStr) =>
            _lineTypeMapper.TryGetValue(typeStr, out var type) ? type : DataType.Unknown;

        private static string[] GetParts(string str) =>
            str.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    }
}