using LGMS.Data.Context;
using Microsoft.EntityFrameworkCore;
using LGMS.Controllers;
using LGMS.Services;
using OfficeOpenXml;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using MailSender.Model;
using MailSender.Services;
using System.IO;
using NLog.Web;

NLog.LogManager.Setup().LoadConfigurationFromAppSettings();
var logger = NLog.LogManager.GetCurrentClassLogger();

try
{
    logger.Info("Starting application");

    var builder = WebApplication.CreateBuilder(args);
    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

    // Add services to the container.
    var IsProdBuild = Boolean.Parse(builder.Configuration["App:IsProdBuild"]);
    builder.Services.AddDbContext<LgmsDbContext>(db => db.UseSqlServer(builder.Configuration.GetConnectionString(IsProdBuild? "LgmsINTSER" : "LgmsDev01INTSER")));
    builder.Services.AddControllersWithViews().AddNewtonsoftJson(n=>n.SerializerSettings.ReferenceLoopHandling=Newtonsoft.Json.ReferenceLoopHandling.Ignore);
    builder.Services.AddTransient<ExcelService>();
    builder.Services.AddScoped<ExcelImportService>();
    builder.Services.AddScoped<AttendanceRecordService>();
    builder.Services.AddScoped<AttendanceReportService>();
    builder.Services.AddScoped<SalarySlipService>();
    builder.Services.AddScoped<OverviewService>();
    builder.Services.AddScoped<SalarySlipPDFService>();
    builder.Services.AddScoped<QuotationPDFService>();
    builder.Services.AddScoped<InvoicePDFService>();
    builder.Services.AddScoped<ImageService>();
    builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<LgmsDbContext>().AddDefaultTokenProviders();
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
            ValidAudience = builder.Configuration["JWT:ValidAudience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
        };
    });

    var emailConfig = builder.Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>();
    builder.Services.AddSingleton(emailConfig);

    builder.Services.AddScoped<IEmailService, EmailService>();

    // Clear default logging providers and Use NLog
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
        {
            Name = "Authorization",
            Description = "valid jwt",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "Bearer"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    var app = builder.Build();

    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";

            var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
            var ex = exceptionHandlerPathFeature?.Error;

            logger.Error(ex, "Unhandled exception");

            await context.Response.WriteAsJsonAsync(new
            {
                error = "An unexpected error occurred and we are notified. Please try again later."
            });
        });
    });

    app.UseDefaultFiles();
    app.UseStaticFiles();

    app.UseCors(c => c.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
    app.UseHttpsRedirection();
    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    // Configure the HTTP request pipeline.
    if (
        app.Environment.IsDevelopment() ||
        bool.Parse(builder.Configuration["App:EnableSwagger"])
    )
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
        endpoints.MapFallbackToFile("/index.html");
    });



    app.Run();
}
catch (Exception ex)
{
    // Log exceptions from Program.cs itself
    logger.Fatal(ex, "Application failed to start");
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
}