using NLog;
using VRtask.Middleware;

var logger = LogManager.GetCurrentClassLogger();
var middleware = new MainMiddleware();

var targetFolder = Environment.CurrentDirectory; //default folder for test

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