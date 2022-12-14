using EF7ColumnJSON.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConsoleApp1.DataAccess
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //User
            modelBuilder.Entity<User>().HasKey(t => t.Id);

            modelBuilder.Entity<User>().Property(t => t.Username)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<User>()
                .OwnsOne(b => b.Address, ownedNavigationBuilder =>
                {
                    ownedNavigationBuilder.ToJson();
                });

            //Post
            modelBuilder.Entity<Post>().HasKey(t => t.Id);

            modelBuilder.Entity<Post>().Property(t => t.Title)
                .HasMaxLength(200)
                .IsRequired();

            modelBuilder.Entity<Post>().Property(t => t.Body)
                .HasMaxLength(4000)
                .IsRequired();

            //Comment
            modelBuilder.Entity<Comment>().HasKey(t => t.Id);

            modelBuilder.Entity<Comment>().Property(t => t.Name)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Comment>().Property(t => t.Body)
                .HasMaxLength(1000)
                .IsRequired();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        public static async Task InitializeAsync(ApplicationDbContext context)
        {
            await context.Database.EnsureDeletedAsync();

            await context.Database.MigrateAsync();

            // seed data
            var jsonPath = @"data.json";
            var json = File.ReadAllText(jsonPath);
            var posts = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Post>>(json);

            posts[0].User.Address = new Address("Street 1", "Sevilla", "Sevilla", "Spain", "111");
            posts[1].User.Address = new Address("Street 2", "Madrid", "Madrid", "Spain", "222");
            posts[2].User.Address = new Address("Street 3", "Barcelona", "Barcelona", "Spain", "333");

            await context.AddRangeAsync(posts);
            await context.SaveChangesAsync();

        }
    }
}
