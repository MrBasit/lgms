using LGMS.Data.Context;
using Microsoft.EntityFrameworkCore;
using LGMS.Controllers;
using LGMS.Services;
using OfficeOpenXml;

var builder = WebApplication.CreateBuilder(args);

ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

// Add services to the container.
var IsBuildForProdEnv = false;
builder.Services.AddDbContext<LgmsDbContext>(db => db.UseSqlServer(builder.Configuration.GetConnectionString(IsBuildForProdEnv?"":"LgmsDev01INTSER")));
builder.Services.AddControllersWithViews().AddNewtonsoftJson(n=>n.SerializerSettings.ReferenceLoopHandling=Newtonsoft.Json.ReferenceLoopHandling.Ignore);
builder.Services.AddTransient<ExcelService>();
builder.Services.AddScoped<ExcelImportService>();
builder.Services.AddScoped<AttendanceRecordService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

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
