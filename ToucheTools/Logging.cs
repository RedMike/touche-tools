using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ToucheTools;

public static class Logging
{
    internal static ILoggerFactory Factory = new NullLoggerFactory();
    
    public static void SetUp(ILoggerFactory factory)
    {
        Factory = factory;
    }
}