using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TicketInventory.Api.Services;
using TicketInventory.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<IEventService, EventService>();

//lets add the TicketInventory DbContex
builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
app.MapControllers();

//app.UseHttpsRedirection();
app.Run();
