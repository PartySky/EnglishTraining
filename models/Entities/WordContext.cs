using EnglishTraining.models;
using Microsoft.EntityFrameworkCore;

namespace EnglishTraining
{
    public class WordContext : DbContext
    {
        public DbSet<VmWord> Words { get; set; }
        public DbSet<VmCollocation> Collocations { get; set; }
        public DbSet<VmSettings> Settings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=EnglishTraining.db");
        }
    }
}
