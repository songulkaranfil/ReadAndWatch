using Microsoft.EntityFrameworkCore;
using ReadAndWatch.Models;

namespace ReadAndWatch.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Book> Books { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<FavoriteBook> FavoriteBooks { get; set; }
        public DbSet<FavoriteMovie> FavoriteMovies { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // FavoriteBook ilişkileri
            modelBuilder.Entity<FavoriteBook>()
                .HasOne(fb => fb.User)
                .WithMany(u => u.FavoriteBooks)
                .HasForeignKey(fb => fb.UserId);

            modelBuilder.Entity<FavoriteBook>()
                .HasOne(fb => fb.Book)
                .WithMany(b => b.FavoriteBooks)
                .HasForeignKey(fb => fb.BookId);

            // 🔽 FavoriteMovie ilişkileri — tam buraya ekle
            modelBuilder.Entity<FavoriteMovie>()
                .HasOne(fm => fm.User)
                .WithMany(u => u.FavoriteMovies)
                .HasForeignKey(fm => fm.UserId);

            modelBuilder.Entity<FavoriteMovie>()
                .HasOne(fm => fm.Movie)
                .WithMany(m => m.FavoriteMovies)
                .HasForeignKey(fm => fm.MovieId);
        }
    }
}
