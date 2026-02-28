using Microsoft.EntityFrameworkCore;

namespace Books.WebAPI.Models;

public sealed class DataBaseContext : DbContext
{
    public DbSet<Book> Books { get; set;}

    public DataBaseContext(DbContextOptions options) : base(options)
    {
        Database.EnsureCreated();
    }
}