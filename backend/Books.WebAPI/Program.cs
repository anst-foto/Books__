using System;
using System.Linq;

using Books.WebAPI.Models;
using Books.WebAPI.Services;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;

const string versionApi = "v1";
const string corsPolicyName = "MyAllowSpecificOrigins";

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrEmpty(connectionString))
    throw new DbConnectionStringException("Отсутствует строка подключения к БД");
builder.Services.AddDbContext<DataBaseContext>(options => options.UseNpgsql(connectionString));

var redisConnection = builder.Configuration.GetConnectionString("Redis");
if (string.IsNullOrEmpty(connectionString))
    throw new RedisConnectionStringException("Отсутствует строка подключения к Redis");
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
});

builder.Services.AddScoped<IBookService, BookServiceDb>();
builder.Services.AddScoped<BookServiceCache>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName,
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc(versionApi,
        new OpenApiInfo { Title = builder.Environment.ApplicationName, Version = versionApi });
});

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint($"/swagger/{versionApi}/swagger.json",
        $"{builder.Environment.ApplicationName} {versionApi}"));
}

app.UseHttpsRedirection();
app.UseCors(corsPolicyName);

var api = app.MapGroup($"/api/{versionApi}/books")
    .WithTags("Books");

api.MapGet("/", async (BookServiceCache service) =>
    {
        var books = await service.GetAllAsync();
        var enumerable = books as Book[] ?? books.ToArray();
        return enumerable.Length == 0
            ? Results.NoContent()
            : Results.Ok(enumerable);
    }).WithName("GetAll")
    .Produces(StatusCodes.Status204NoContent)
    .Produces<Book[]>(StatusCodes.Status200OK);

api.MapGet("/{id:guid}", async (Guid id, BookServiceCache service) =>
    {
        try
        {
            var book = await service.GetByIdAsync(id);
            return book is null
                ? Results.NotFound()
                : Results.Ok(book);
        }
        catch (Exception e)
        {
            return Results.InternalServerError(e.Message);
        }
    }).WithName("GetById")
    .Produces<Book>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status500InternalServerError);

api.MapPost("/", async (Book book, BookServiceCache service) =>
    {
        try
        {
            await service.AddNewAsync(book);

            return Results.Created();
        }
        catch (Exception e)
        {
            return Results.InternalServerError(e.Message);
        }
    }).WithName("Create")
    .Produces(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status500InternalServerError);

await app.RunAsync();