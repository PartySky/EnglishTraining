using EnglishTraining.models;
using Microsoft.EntityFrameworkCore;

namespace EnglishTraining
{
    public class WordContext : DbContext
    {
        public DbSet<Word> Words { get; set; }
        public DbSet<WordLocalization> WordLocalization { get; set; }
        public DbSet<LearnDay> LearnDay { get; set; }
        public DbSet<FourDaysLearnPhase> FourDaysLearnPhase { get; set; }
        public DbSet<RepeatIterationNum> RepeatIterationNum { get; set; }
        public DbSet<NextRepeatDate> NextRepeatDate { get; set; }
        public DbSet<DailyReapeatCount> DailyReapeatCount { get; set; }
        public DbSet<VmCollocation> Collocations { get; set; }
        public DbSet<VmSettings> Settings { get; set; }
        public DbSet<WordWithoutAudio> WordsWithoutAudio { get; set; }
        public DbSet<ParsedWord> ParsedWords { get; set; }
        public DbSet<PrioritizedWords> PrioritizedWords { get; set; }

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

    public class PrioritizedWords
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Lang { get; set; }
    }
}
