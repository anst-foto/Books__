using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Books.WebAPI.Models;

using Microsoft.EntityFrameworkCore;

namespace Books.WebAPI.Services;

public class BookServiceDb : IBookService
{
    private readonly DataBaseContext _db;

    public BookServiceDb(DataBaseContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Book>> GetAllAsync() => await _db.Books.ToArrayAsync();
    public async Task<Book?> GetByIdAsync(Guid id) => await _db.Books.SingleOrDefaultAsync(b => b.Id == id);

    public async Task AddNewAsync(Book book)
    {
        await _db.Books.AddAsync(book);
        await _db.SaveChangesAsync();
    }
}