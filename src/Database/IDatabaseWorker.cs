namespace VRtask.Database
{
    internal interface IDatabaseWorker : IDisposable
    {
        void SaveBoxInfo(string supplierId, string boxId);

        void SaveContentInfo(string boxId, string isbn, int qty);
    }
}