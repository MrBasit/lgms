using LGMS.Data.Context;
using Microsoft.EntityFrameworkCore;
using LGMS.Controllers;
using LGMS.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<LgmsDbContext>(db => db.UseSqlServer(builder.Configuration.GetConnectionString("LgmsDev01INTSER")));
builder.Services.AddControllersWithViews().AddNewtonsoftJson(n=>n.SerializerSettings.ReferenceLoopHandling=Newtonsoft.Json.ReferenceLoopHandling.Ignore);
builder.Services.AddTransient<ExcelService>();
builder.Services.AddScoped<ExcelImportService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(c=>c.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
