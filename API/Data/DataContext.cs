using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options)
            : base(options) { }

        public DbSet<TvShow> TvShows { get; set; }
        public DbSet<CastMember> CastMembers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TvShow>().HasMany(e => e.CastMembers).WithMany(e => e.TvShows);
        }
    }
}
