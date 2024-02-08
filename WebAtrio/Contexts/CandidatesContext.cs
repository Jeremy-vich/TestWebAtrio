using Microsoft.EntityFrameworkCore;
using WebAtrio.Models;

namespace WebAtrio.Contexts
{
    public class CandidatesContext : DbContext
    {
        public DbSet<Person> People { get; set; }
        public DbSet<Job> Jobs { get; set; }

        public string DbPath { get; }

        public CandidatesContext() : this("candidate.db") { }

        public CandidatesContext(string dataSource)
        {
            if (string.IsNullOrWhiteSpace(dataSource))
            {
                throw new ArgumentException("DataSource cannot be null or empty", nameof(dataSource));
            }

            if (dataSource.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
            {
                var folder = Environment.SpecialFolder.LocalApplicationData;
                var path = Environment.GetFolderPath(folder);
                DbPath = Path.Join(path, dataSource);
            }
            else
            {
                DbPath = dataSource;
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                if (DbPath.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
                {
                    options.UseSqlite($"Data Source={DbPath}");
                }
                else
                {
                    options.UseInMemoryDatabase(databaseName: DbPath);
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.Entity<Person>()
                .HasMany(e => e.Jobs)
                .WithOne(e => e.Person)
                .HasForeignKey(e => e.PersonId)
                .HasPrincipalKey(e => e.Id); ;
        }
    }
}
