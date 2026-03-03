using DbUp;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var connectionString = configuration.GetConnectionString("ChessConnection")
    ?? throw new InvalidOperationException("Connection string 'ChessConnection' is missing. Set it in appsettings.json or environment.");

var scriptsPath = Path.Combine(AppContext.BaseDirectory, "Scripts");
if (!Directory.Exists(scriptsPath))
{
    Console.WriteLine("Scripts folder not found at: {0}", scriptsPath);
    return 1;
}

var upgrader = DeployChanges.To
    .SqlDatabase(connectionString)
    .WithScriptsFromFileSystem(scriptsPath)
    .WithTransaction()
    .LogToConsole()
    .Build();

var result = upgrader.PerformUpgrade();

if (!result.Successful)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(result.Error);
    Console.ResetColor();
    return 1;
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Migrations completed successfully.");
Console.ResetColor();
return 0;
