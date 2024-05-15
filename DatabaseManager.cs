using ExtendedAutoStart.Data;

namespace ExtendedAutoStart
{
    public class DatabaseManager
    {
        private static readonly Lazy<DatabaseManager> instance = new Lazy<DatabaseManager>(() => new DatabaseManager());
        private static readonly object lockObject = new object();
        private static bool isInitialized = false;

        public static DatabaseManager Instance => instance.Value;

        private DatabaseManager()
        {
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            lock (lockObject)
            {
                if (!isInitialized)
                {
                    using (var context = new MainDbContext())
                    {
                        if (!File.Exists("MainDb.db"))
                        {
                            context.Database.EnsureCreated();
                        }
                    }
                    isInitialized = true;
                }
            }
        }

        public bool CheckDatabaseExists()
        {
            return File.Exists("MainDb.db");
        }

        public void InitializeDatabaseIfNeeded()
        {
            lock (lockObject)
            {
                if (!isInitialized)
                {
                    using (var context = new MainDbContext())
                    {
                        if (!File.Exists("MainDb.db"))
                        {
                            context.Database.EnsureCreated();
                        }
                    }
                    isInitialized = true;
                }
            }
        }
    }
}