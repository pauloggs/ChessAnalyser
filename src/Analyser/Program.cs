using Analyser;
using Interfaces.Analytics;
using Microsoft.OpenApi.Models;
using Repositories;
using Services;
using Services.Analytics;
using Services.Helpers;
using System.Reflection;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<PgnOptions>(builder.Configuration.GetSection(PgnOptions.SectionName));

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
        Description =
            "An ASP.NET Core Web API for parsing and analysing PGN files. " +
            "Analytics metrics: Swagger group AnalyticsMetrics; routes GET /api/analytics/metrics and POST /api/analytics/metrics/execute. " +
            "This host binds http://localhost:5000 and https://localhost:5001 via ConfigureKestrel (launch profiles are aligned to these ports).",
        TermsOfService = new Uri("https://example.com/terms")
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

// Use this more permissive policy for debugging to rule out port issues:
builder.Services.AddCors(options => {
    options.AddPolicy("AllowSwagger", policy => {
        policy.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenLocalhost(5000); // Standard HTTP
    serverOptions.ListenAnyIP(5001, listenOptions =>
    {
        listenOptions.UseHttps(); // Explicit HTTPS
    });
});

builder.Services.AddScoped<IBitBoardManipulator, BitBoardManipulator>();
builder.Services.AddScoped<IBoardPositionCalculator, BoardPositionCalculator>();
builder.Services.AddScoped<IBoardPositionCalculatorHelper, BoardPositionCalculatorHelper>();
builder.Services.AddScoped<IDisplayService, DisplayService>();
builder.Services.AddScoped<IBoardPositionsHelper, BoardPositionsHelper>();
builder.Services.AddScoped<IPersistenceService, PersistenceService>();
builder.Services.AddScoped<IPlayerResolver, PlayerResolver>();
builder.Services.AddScoped<IMoveInterpreter, MoveInterpreter>();
builder.Services.AddScoped<IBoardPositionService, BoardPositionService>();
builder.Services.AddScoped<IMoveInterpreterHelper, MoveInterpreterHelper>();
builder.Services.AddScoped<ISourceSquareHelper, SourceSquareHelper>();
builder.Services.AddScoped<IDestinationSquareHelper, DestinationSquareHelper>();
builder.Services.AddScoped<IPawnMoveInterpreter, PawnMoveInterpreter>();
builder.Services.AddScoped<IPieceMoveInterpreter, PieceMoveInterpreter>();
builder.Services.AddScoped<IPieceSourceFinderService, PieceSourceFinderService>();
builder.Services.AddScoped<ILegalMoveChecker, LegalMoveChecker>();
builder.Services.AddScoped<IRankAndFileHelper, RankAndFileHelper>();
builder.Services.AddScoped<IBitBoardManipulatorHelper, BitBoardManipulatorHelper>();
builder.Services.AddScoped<INaming, Naming>();
builder.Services.AddScoped<IFileHandler, FileHandler>();
builder.Services.AddScoped<IPgnParser, PgnParser>();
builder.Services.AddScoped<IEtlService, EtlService>();
builder.Services.AddSingleton<IEtlProgressStore, EtlProgressStore>();
builder.Services.AddScoped<IChessRepository, ChessRepository>();
builder.Services.AddSingleton<IPieceValues, ClassicalPieceValues>();
builder.Services.AddScoped<IGamePositionSummaryFactory, GamePositionSummaryFactory>();
builder.Services.AddScoped<IGameMoveDeriver, GameMoveDeriver>();
builder.Services.AddScoped<IAnalyticsMaterializationService, AnalyticsMaterializationService>();
builder.Services.AddScoped<IMetricExecutor, AverageMaterialByYearAndColourExecutor>();
builder.Services.AddScoped<IMetricExecutor, KnightMoveDestinationFrequencyExecutor>();
builder.Services.AddScoped<IMetricExecutor, GameCountByEcoExecutor>();
builder.Services.AddScoped<IMetricExecutor, GameCountByYearExecutor>();
builder.Services.AddScoped<IMetricExecutor, GameCountByResultExecutor>();
builder.Services.AddScoped<IMetricExecutor, AverageMaterialByPlayerAtMoveExecutor>();
builder.Services.AddScoped<IMetricRegistry, MetricRegistry>();
builder.Services.AddScoped<IAnalyticsBackfillService, AnalyticsBackfillService>();

var app = builder.Build();

if (args.Any(a => string.Equals(a, "--backfill-analytics", StringComparison.OrdinalIgnoreCase)))
{
    int? maxGames = null;
    for (var i = 0; i < args.Length - 1; i++)
    {
        if (string.Equals(args[i], "--max-games", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(args[i + 1], out var parsed)
            && parsed > 0)
            maxGames = parsed;
    }

    await using var scope = app.Services.CreateAsyncScope();
    var backfill = scope.ServiceProvider.GetRequiredService<IAnalyticsBackfillService>();
    var outcome = await backfill.BackfillMissingAnalyticsAsync(new AnalyticsBackfillOptions { MaxGames = maxGames });
    Console.WriteLine(
        $"Analytics backfill: considered={outcome.GamesConsidered}, materialized={outcome.GamesMaterialized}, skipped={outcome.GamesSkipped}, failed={outcome.GamesFailed}.");
    return;
}

if (args.Any(a => string.Equals(a, "--profile-materialization", StringComparison.OrdinalIgnoreCase)))
{
    var iterations = 5000;
    for (var i = 0; i < args.Length - 1; i++)
    {
        if (string.Equals(args[i], "--iterations", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(args[i + 1], out var parsed)
            && parsed > 0)
            iterations = parsed;
    }

    var result = await MaterializationPerfHarness.RunAsync(iterations);
    Console.WriteLine(
        $"Materialization perf (in-memory, no DB): iterations={result.Iterations}, wall={result.ElapsedMilliseconds:F1} ms, " +
        $"{result.GamesPerSecond:F0} games/s, {result.DerivedRowsPerSecond:F0} summary+move rows/s " +
        $"({result.SummaryRowsPerIteration} summaries + {result.MoveRowsPerIteration} move per game). " +
        $"See docs/ANALYTICS_MATERIALIZATION_PERF.md for methodology (PLAN §11 item 12 / DESIGN NFR-3).");
    return;
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Avoid stale index.html when iterating on wwwroot (F5 / hard-refresh confusion in Development).
        if (app.Environment.IsDevelopment()
            && ctx.File.Name.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
        }
    }
});

app.UseRouting();

// ACTIVATE THE POLICY HERE
app.UseCors("AllowSwagger");

app.UseAuthorization();

app.MapControllers();

app.Run();


