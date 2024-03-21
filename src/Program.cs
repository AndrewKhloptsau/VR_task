using NLog;
using NLog.Config;
using NLog.Targets;
using VRtask.Middleware;

var config = new LoggingConfiguration();

config.AddRule(LogLevel.Info, LogLevel.Fatal, new ConsoleTarget("logconsole"));

LogManager.Configuration = config;

var logger = LogManager.GetCurrentClassLogger();
var middleware = new MainMiddleware();

var targetFolder = Path.Combine(Environment.CurrentDirectory, "TargetFolder"); //default folder for test

try
{
    logger.Info("Init middleware");
    middleware.Init(targetFolder);

    logger.Info("Run middleware");
    middleware.Run();

    Console.WriteLine("Folder checking...");
    Console.ReadLine();
}
catch (Exception ex)
{
    logger.Error(ex);
}
finally
{
    middleware.Dispose();
}