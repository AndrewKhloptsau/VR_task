namespace VRtask.Database
{
    internal interface IDatabaseContext : IDisposable
    {
        void SaveBoxInfo(string supplierId, string boxId);

        void SaveContentInfo(string boxId, string poNumber, string isbn, int qty);
    }
}