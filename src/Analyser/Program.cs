using Microsoft.OpenApi.Models;
using Repositories;
using Services;
using Services.Helpers;
using System.Configuration;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Chess Analyser",
        Description = "An ASP.NET Core Web API for parsing and analysing PGN files.",
        TermsOfService = new Uri("https://example.com/terms")
    });

    // using System.Reflection;
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

builder.Services.AddScoped<IBitBoardManipulator, BitBoardManipulator>();
builder.Services.AddScoped<IDisplayService, DisplayService>();
builder.Services.AddScoped<IBoardPositionsHelper, BoardPositionsHelper>();
builder.Services.AddScoped<IPersistenceService, PersistenceService>();
builder.Services.AddScoped<IMoveInterpreter, MoveInterpreter>();
builder.Services.AddScoped<IBoardPositionUpdater, BoardPositionUpdater>();
builder.Services.AddScoped<IBoardPositionService, BoardPositionService>();
builder.Services.AddScoped<IMoveInterpreterHelper, MoveInterpreterHelper>();
builder.Services.AddScoped<ISourceSquareHelper, SourceSquareHelper>();
builder.Services.AddScoped<IDestinationSquareHelper, DestinationSquareHelper>();
builder.Services.AddScoped<INaming, Naming>();
builder.Services.AddScoped<IFileHandler, FileHandler>();
builder.Services.AddScoped<IPgnParser, PgnParser>();
builder.Services.AddScoped<IEtlService, EtlService>();
builder.Services.AddScoped<IChessRepository, ChessRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


