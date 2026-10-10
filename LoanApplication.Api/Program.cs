using LoanApplication.Application.Interfcae;
using LoanApplication.Application.Mapper;
using LoanApplication.Infrastructure.Data;
using LoanApplication.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using LoanApplication.Application.Mapper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();



builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenApi();

// Register DbContext BEFORE builder.Build()
builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("dbconn")
    ));

builder.Services.AddSingleton<IMapper>(
    new MapperConfiguration(cfg =>
    {
        cfg.AddProfile<MappingData>();
    }).CreateMapper());

builder.Services.AddScoped<ILoanDealService, LoanDealService>();
builder.Services.AddScoped< ISanctionLetterService, SanctionLetterService>();

var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();