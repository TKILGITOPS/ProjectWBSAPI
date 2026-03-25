using Microsoft.EntityFrameworkCore;
using ProjectWBSAPI.Helper;
using ProjectWBSAPI.Model;
using System.Net;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();
// Add services to the container.
// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sqlOptions => sqlOptions.CommandTimeout(120)));
builder.Services.AddControllers();
builder.Services.Configure<SapSettings>(
    builder.Configuration.GetSection("DevAppSettings"));
builder.Services.Configure<JobSettings>(
    builder.Configuration.GetSection("JobSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddSingleton<SapConnectionService>();
builder.Services.AddScoped<SapProjectService>();
builder.Services.AddScoped<ProjectSyncService>();
builder.Services.AddHostedService<BackgroundWorkerService>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
