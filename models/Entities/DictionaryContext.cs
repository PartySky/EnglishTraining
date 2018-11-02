using Microsoft.EntityFrameworkCore;

namespace EnglishTraining
{
    public class DictionaryContext : DbContext
    {
        public DbSet<Word> Words { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=Dictionary.db");
        }
    }
}
