using DAL_ConferenceHallManagement.DbContexts;
using Microsoft.EntityFrameworkCore;
using BLL_ConferenceHallManagement;
using UoW_ConferenceHallManagement;
using Repository_ConferenceHallManagement.AppDataRepositoy;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", builder =>
    {
        builder.WithOrigins("https://localhost:7150", "http://localhost:5000")
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Add DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ConferenceHallManagementContext>(options =>
    options.UseSqlServer(connectionString));

// Register UnitOfWork and Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IConferenceHallDataRepository, ConferenceHallDataRepository>();

// Register BLL services
builder.Services.AddScoped<IBLLConferenceHall, BLLConferenceHall>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowBlazor");

app.UseAuthorization();

app.MapControllers();

app.Run();
