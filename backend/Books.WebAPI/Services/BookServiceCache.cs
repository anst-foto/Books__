using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

using Books.WebAPI.Models;

using Microsoft.Extensions.Caching.Distributed;

namespace Books.WebAPI.Services;

public class BookServiceCache : IBookService
{
    private readonly IBookService _service;
    private readonly IDistributedCache _cache;


    public BookServiceCache(IBookService service, IDistributedCache cache)
    {
        _service = service;
        _cache = cache;
    }

    public async Task<IEnumerable<Book>> GetAllAsync() => await _service.GetAllAsync();

    public async Task<Book?> GetByIdAsync(Guid id)
    {
        var key = CalculateKey(id);
        var bookJson = await _cache.GetStringAsync(key);
        if (string.IsNullOrWhiteSpace(bookJson)) return await _service.GetByIdAsync(id);

        var book = JsonSerializer.Deserialize<Book>(bookJson) ?? throw new InvalidOperationException();
        return book;
    }

    public async Task AddNewAsync(Book book)
    {
        await _service.AddNewAsync(book);

        var key = CalculateKey(book.Id);
        var value = JsonSerializer.Serialize(book);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
        };
        await _cache.SetStringAsync(key, value, options);
    }

    private static string CalculateKey(Guid id) => $"book:{id.ToString()}";
}