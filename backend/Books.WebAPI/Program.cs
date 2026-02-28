using System;

using Books.WebAPI.Models;

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
    throw new Exception("No Connection String");
builder.Services.AddDbContext<DataBaseContext>(options => options.UseNpgsql(connectionString));

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
api.MapGet("/", async (DataBaseContext db) =>
    {
        var books = await db.Books.ToArrayAsync();
        return books.Length == 0
            ? Results.NoContent()
            : Results.Ok(books);
    }).WithName("GetAll")
    .Produces(StatusCodes.Status204NoContent)
    .Produces<Book[]>(StatusCodes.Status200OK);

api.MapPost("/", async (Book book, DataBaseContext db) =>
    {
        try
        {
            await db.Books.AddAsync(book);
            await db.SaveChangesAsync();

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