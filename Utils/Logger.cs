using Microsoft.Extensions.Logging;

namespace Trick.Utils;

public static class Logger
{
    public static void LogInfo(ILogger logger, string message, params object[] args)
    {
        logger.LogInformation(message, args);
    }

    public static void LogWarning(ILogger logger, string message, params object[] args)
    {
        logger.LogWarning(message, args);
    }

    public static void LogError(ILogger logger, Exception? exception, string message, params object[] args)
    {
        logger.LogError(exception, message, args);
    }

    public static void LogError(ILogger logger, string message, params object[] args)
    {
        logger.LogError(message, args);
    }

    public static void LogDebug(ILogger logger, string message, params object[] args)
    {
        logger.LogDebug(message, args);
    }

    public static void LogCritical(ILogger logger, Exception? exception, string message, params object[] args)
    {
        logger.LogCritical(exception, message, args);
    }

    public static void LogTrace(ILogger logger, string message, params object[] args)
    {
        logger.LogTrace(message, args);
    }
}
