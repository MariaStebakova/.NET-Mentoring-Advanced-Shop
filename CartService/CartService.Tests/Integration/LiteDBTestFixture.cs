namespace CartService.Tests.Integration
{
    public class LiteDbTestFixture : IDisposable
    {
        public string DatabaseFilename { get; }

        public LiteDbTestFixture()
        {
            DatabaseFilename = $"TestCart.db";
        }

        public void Dispose()
        {
            if (File.Exists(DatabaseFilename))
            {
                try
                {
                    File.Delete(DatabaseFilename);
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Warning: Failed to delete {DatabaseFilename}. {ex.Message}");
                }
            }
        }
    }
}