using Microsoft.EntityFrameworkCore;

namespace LibraryManagementAPI.Models
{
    public class LibraryContext : DbContext
    {
        public LibraryContext(DbContextOptions<LibraryContext> options) : base(options) { }

        public DbSet<Book> Book { get; set; }
        public DbSet<Author> Authors { get; set; }

    }
}
