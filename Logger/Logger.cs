using Discord;
using System;
using System.Threading.Tasks;
using GenericUtil;

public static class Logger
{
    private readonly static object _ThreadLock = new object();
    public static LogSeverity LogLevel { get; set; } = LogSeverity.Debug;
    public async static Task LogAsync(LogMessage message)
    {
        await Task.Run(() =>
        {
            if(message.Severity <= LogLevel)
            {
                lock (_ThreadLock)
                {
                    ConsoleWrapper.WriteLine(message.ToString(), message.Severity switch
                    {
                        LogSeverity.Debug => ConsoleColor.White,
                        LogSeverity.Verbose => ConsoleColor.Gray,
                        LogSeverity.Info => ConsoleColor.Green,
                        LogSeverity.Warning => ConsoleColor.Yellow,
                        LogSeverity.Error => ConsoleColor.Red,
                        LogSeverity.Critical => ConsoleColor.DarkRed,
                        _ => ConsoleColor.Cyan
                    });
                }
            }
        });
    }
}
