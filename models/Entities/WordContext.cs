using EnglishTraining.models;
using Microsoft.EntityFrameworkCore;

namespace EnglishTraining
{
    public class WordContext : DbContext
    {
        public DbSet<Word> Words { get; set; }
        public DbSet<VmWordLocalization> WordLocalization { get; set; }
        public DbSet<LearnDay> LearnDay { get; set; }
        public DbSet<FourDaysLearnPhase> FourDaysLearnPhase { get; set; }
        public DbSet<RepeatIterationNum> RepeatIterationNum { get; set; }
        public DbSet<NextRepeatDate> NextRepeatDate { get; set; }
        public DbSet<DailyReapeatCount> DailyReapeatCount { get; set; }
        public DbSet<VmCollocation> Collocations { get; set; }
        public DbSet<VmSettings> Settings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=EnglishTraining.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Entity<VmWordLocalization>(entity =>
            //{
            //    entity.HasKey(e => e.Key);
            //    entity.Property(e => e.Value);
            //});
        }
    }
}
