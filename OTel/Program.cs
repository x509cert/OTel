using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Set up a service collection and configure logging
var serviceCollection = new ServiceCollection();
ConfigureServices(serviceCollection);

// Build the service provider to get the ILogger instance
using var serviceProvider = serviceCollection.BuildServiceProvider();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

// Log messages
logger.LogInformation("This is an informational message.");
logger.LogWarning("This is a warning message.");
logger.LogError("This is an error message.");

Console.WriteLine("Logs written to file. Press any key to exit.");
Console.ReadKey();

static void ConfigureServices(IServiceCollection services)
{
    // Add logging and configure it to write to a file
    services.AddLogging(configure =>
    {
        configure.ClearProviders(); // Remove default providers
        configure.AddConsole(); // Console logging
        configure.AddFileLogger(); // Custom file logger configuration
    });
}

// Custom file logger extension
static class FileLoggerExtensions
{
    public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder)
    {
        builder.AddProvider(new FileLoggerProvider("logfile.txt"));
        return builder;
    }
}

// File logger provider
class FileLoggerProvider : ILoggerProvider
{
    private readonly string _filePath;

    public FileLoggerProvider(string filePath) => _filePath = filePath;

    public ILogger CreateLogger(string categoryName) => new FileLogger(_filePath);

    public void Dispose() { }
}

// File logger implementation
class FileLogger : ILogger
{
    private readonly string _filePath;
    private readonly object _lock = new();

    public FileLogger(string filePath) => _filePath = filePath;

    public IDisposable BeginScope<TState>(TState state) => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (formatter != null)
        {
            lock (_lock)
            {
                File.AppendAllText(_filePath, $"[{DateTime.Now}] {logLevel}: {formatter(state, exception)}{Environment.NewLine}");
            }
        }
    }
}
