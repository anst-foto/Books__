using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Books.WebAPI.Models;

namespace Books.WebAPI.Services;

public interface IBookService
{
    public Task<IEnumerable<Book>> GetAllAsync();
    public Task<Book?> GetByIdAsync(Guid id);
    public Task AddNewAsync(Book book);
}