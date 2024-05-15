using ExtendedAutoStart.Models;
using Microsoft.EntityFrameworkCore;

namespace ExtendedAutoStart.Data
{
    public class MainDbContext : DbContext
    {
        public DbSet<ExtendedStartupProgram> ProgramsInExtendedStartup { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=MainDb.db");
        }
    }
}