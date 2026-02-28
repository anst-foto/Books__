using System;

namespace Books.WebAPI.Models;

public record class Book
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public required string Title { get; init; }
}