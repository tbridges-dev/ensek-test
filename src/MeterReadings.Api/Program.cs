using MeterReadings.Api.Data;
using MeterReadings.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// To keep this clean, I would consider extracting this into a separate method for the database setup
var dbConnection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<IApplicationDbContext, ApplicationDbContext>(opt =>
    opt.UseNpgsql(dbConnection, x => x.MigrationsAssembly("MeterReadings.Api.Data"))
                                      .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
// builder.Services.AddDbContext<IApplicationDbContext, ApplicationDbContext>(opt => 
// opt.UseSqlite($"Data Source=ENSEK", x => x.MigrationsAssembly("MeterReadings.Api.Data")));

// Would likely extract this into a separate method as the project grew
builder.Services.AddScoped<IMeterReadingService, MeterReadingService>();

builder.Services.AddCors(opts =>
{
    opts.AddPolicy("CorsPolicy", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Migrate the database, in practise, this would likely be done during deployment.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(opt =>
    {
        opt.SwaggerEndpoint("/openapi/v1.json", "OpenAPI V1");
    });
    // app.UseSwaggerUi(opt =>
    // {
    //     opt.DocumentPath = "openapi/v1.json";
    // });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
