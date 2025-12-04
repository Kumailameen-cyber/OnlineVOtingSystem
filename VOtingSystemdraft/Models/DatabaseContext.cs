using Microsoft.EntityFrameworkCore;
using VOtingSystemdraft.Models;

namespace VOtingSystemdraft.Models
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Voter> Voters { get; set; }
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<Announcement> Announcements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --------------------------
            // 1-to-1: Admin ↔ User
            // --------------------------
            modelBuilder.Entity<Admin>()
                .HasOne(a => a.User)
                .WithOne()
                .HasForeignKey<Admin>(a => a.Id);

            // --------------------------
            // 1-to-1: Voter ↔ User
            // --------------------------
            modelBuilder.Entity<Voter>()
                .HasOne(v => v.User)
                .WithOne()
                .HasForeignKey<Voter>(v => v.Id);

            // --------------------------
            // 1-to-1: Candidate ↔ User
            // --------------------------
            modelBuilder.Entity<Candidate>()
                .HasOne(c => c.User)
                .WithOne()
                .HasForeignKey<Candidate>(c => c.Id);
        }
       
    }
}
