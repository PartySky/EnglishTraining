using Microsoft.EntityFrameworkCore;

namespace EnglishTraining
{
    public class WordContext : DbContext
    {
        public DbSet<VmWord> Words { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=EnglishTraining.db");
        }
    }
}
