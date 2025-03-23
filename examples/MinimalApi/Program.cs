using Microsoft.AspNetCore.Mvc;
using MinimalApi.Models;
using QKP.EzId;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// example of getting a unique generator ID per node
static int GetGeneratorId()
{
    int hash = Environment.MachineName.GetHashCode();
    int tenBitMask = 0x3FF;
    int generatorId = hash & tenBitMask; //0-1023
    return generatorId;
}

// example of product which uses EzId as type for identifier
app.MapPost("/product", (
        [FromBody]
        CreateProduct createProduct) =>
    {
        var id = new EzIdGenerator<EzId>(GetGeneratorId()).GetNextId();
        return Results.Ok(id);
    })
    .WithName("PostProduct");

app.MapGet("/product/{id}", (
        [FromRoute]
        EzId id
    ) =>
    {
        var product = new Product(id, "John Doe");
        return product;
    })
    .WithName("GetProduct");

// Example order, which uses a derived type of EzId as OrderId.
app.MapPost("/order", (
        [FromBody]
        CreateOrder createOrder) =>
    {
        var id = new EzIdGenerator<EzId>(GetGeneratorId()).GetNextId();
        var order = new Order(id, createOrder.CustomerName, createOrder.TotalPrice);
        return Results.Created((string?)null, id);
    })
    .WithName("PostOrder");

app.MapGet("/order/{id}", (
        [FromRoute]
        EzId id
    ) =>
    {
        var order = new Order(id, "John Doe", 100);
        return order;
    })
    .WithName("GetOrder");

app.Run();
