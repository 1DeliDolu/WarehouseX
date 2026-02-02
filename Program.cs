using WarehouseX.Components;
using WarehouseX;
using Microsoft.EntityFrameworkCore;
using WarehouseX.Repositories;
using WarehouseX.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var defaultConn = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<WarehouseXDbContext>(options =>
    options.UseMySql(defaultConn, ServerVersion.AutoDetect(defaultConn)));

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IImportOrderRepository, ImportOrderRepository>();
builder.Services.AddScoped<IExportOrderRepository, ExportOrderRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IImportOrderService, ImportOrderService>();
builder.Services.AddScoped<IExportOrderService, ExportOrderService>();
builder.Services.AddScoped<ProductHistoryService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
