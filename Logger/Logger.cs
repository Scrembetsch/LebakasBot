using Discord;
using System;
using System.Threading.Tasks;
using GenericUtil;

public static class Logger
{
    public static Task LogAsync(LogMessage message)
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
        return Task.CompletedTask;
    }
}
