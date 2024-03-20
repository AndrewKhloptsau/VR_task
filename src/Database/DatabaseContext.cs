using Microsoft.EntityFrameworkCore;
using VRtask.Database.Models;

namespace VRtask.Database
{
    internal sealed class DatabaseContext : DbContext, IDatabaseContext
    {
        private const string ConnectionString = @"Server=(localdb)\mssqllocaldb;Database=vr_storage;Trusted_Connection=True;"; // should be hidden in some private storage
        private const int SaveTimeout = 10_000; //10 sec

        private readonly CancellationTokenSource _tokenSource = new();
        private long _newEventsCnt = 0;

        public DbSet<Box> Boxes { get; set; }

        public DbSet<Content> BoxContents { get; set; }


        public DatabaseContext()
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();

            _ = RunSaveLoop(); // I decided to use a timer to optimize writing values to the database
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // if data is immutable i would use some NoSQL db (like MongoDB or LevelDB) to improve write operation performance
            optionsBuilder.UseSqlServer(ConnectionString);
        }


        public void SaveBoxInfo(string supplierId, string boxId)
        {
            if (!Boxes.Any(b => b.Identifier == boxId))
            {
                var box = new Box
                {
                    SupplierIdentifier = supplierId,
                    Identifier = boxId,
                };

                Boxes.Add(box);
                Interlocked.Increment(ref _newEventsCnt); // this is multithreading protection because I can have more than 1 data loader
                // maybe a new sync queue for database operations will be better idea? (but i don't have time to implement it)
            }
        }

        public void SaveContentInfo(string boxId, string poNumber, string isbn, int qty)
        {
            var box = Boxes.Find(boxId);
            var content = BoxContents.Find(poNumber);

            if (content is null)
            {
                content = new Content
                {
                    PoNumber = poNumber,
                    Isbn = isbn,
                    Quantity = qty,
                };

                BoxContents.Add(content);
                Interlocked.Increment(ref _newEventsCnt);
            }

            if (box is not null)
            {
                box?.Contents.Add(content);
                Interlocked.Increment(ref _newEventsCnt);
            }

            SaveChanges();
        }

        public new void Dispose() // overriding for Dispose is closed, so i have to use 'new' keyword
        {
            _tokenSource.Cancel();

            base.Dispose();
        }


        private async Task RunSaveLoop()
        {
            var token = _tokenSource.Token;

            while (!_tokenSource.IsCancellationRequested)
            {
                if (Interlocked.Read(ref _newEventsCnt) > 0)
                {
                    if (await Database.CanConnectAsync(token))
                    {
                        await SaveChangesAsync(token);

                        Interlocked.Exchange(ref _newEventsCnt, 0);
                    }
                }

                await Task.Delay(SaveTimeout, token);
            }
        }
    }
}