using System;

namespace KittsMenuSystem;

internal class Log
{
    internal enum LogLevel
    {
        Error = 1,
        Warn,
        Info,
        Debug
    }

    /// <summary>
    /// Sends a <see cref="LogLevel.Info"/> level message to the server console.
    /// </summary>
    /// <param name="message">The message to be sent.</param>
    internal static void Info(object message) =>
        Send($"{message}", LogLevel.Info, ConsoleColor.Green);
    /// <summary>
    /// Sends a <see cref="LogLevel.Info"/> level message to the server console.
    /// </summary>
    /// <param name="path">Path the message is coming from.</param>
    /// <param name="message">The message to be sent.</param>
    internal static void Info(object path, object message)
    {
        string newPath = (path ?? "").ToString();

        Send(
            (newPath == "" ? "" : $"[{newPath}] ") + message,
            LogLevel.Info,
            ConsoleColor.Green
        );
    }

    /// <summary>
    /// Sends a <see cref="LogLevel.Debug"/> level message to the server console.
    /// </summary>
    /// <param name="message">The message to be sent.</param>
    internal static void Debug(object message)
    {
        if (KittsMenuSystem.Config.Debug)
            Send(message, LogLevel.Debug, ConsoleColor.Magenta);
    }
    /// <summary>
    /// Sends a <see cref="LogLevel.Debug"/> level message to the server console.
    /// </summary>
    /// <param name="path">Path the message is coming from.</param>
    /// <param name="message">The message to be sent.</param>
    internal static void Debug(object path, object message)
    {
        string newPath = (path ?? "").ToString();
        if (KittsMenuSystem.Config.Debug)
            Send(
                (newPath == "" ? "" : $"[{newPath}] ") + message,
                LogLevel.Debug, ConsoleColor.Cyan
            );
    }

    /// <summary>
    /// Sends a <see cref="LogLevel.Warn"/> level message to the server console.
    /// </summary>
    /// <param name="message">The message to be sent.</param>
    internal static void Warn(object message) =>
        Send($"{message}", LogLevel.Warn, ConsoleColor.Yellow);
    /// <summary>
    /// Sends a <see cref="LogLevel.Warn"/> level message to the server console.
    /// </summary>
    /// <param name="path">Path the message is coming from.</param>
    /// <param name="message">The message to be sent.</param>
    internal static void Warn(object path, object message)
    {
        string newPath = (path ?? "").ToString();
        Send(
            (newPath == "" ? "" : $"[{newPath}] ") + message,
            LogLevel.Warn, ConsoleColor.Yellow
        );
    }

    /// <summary>
    /// Sends a <see cref="LogLevel.Error"/> level message to the server console.
    /// </summary>
    /// <param name="message">The message to be sent.</param>
    internal static void Error(object message) =>
        Send($"{message}", LogLevel.Error, ConsoleColor.Red);
    /// <summary>
    /// Sends a <see cref="LogLevel.Error"/> level message to the server console.
    /// </summary>
    /// <param name="path">Path the message is coming from.</param>
    /// <param name="message">The message to be sent.</param>
    internal static void Error(object path, object message)
    {
        string newPath = (path ?? "").ToString();
        bool containsMenu = newPath.IndexOf("menu", StringComparison.OrdinalIgnoreCase) >= 0;

        Send(
            (newPath == "" ? "" : $"[{newPath}] ") + message,
            LogLevel.Error,
            containsMenu ? ConsoleColor.DarkRed : ConsoleColor.Red
        );
    }

    /// <summary>
    /// Sends a message to the console.
    /// </summary>
    /// <param name="message">The message to be sent.</param>
    /// <param name="level">Level of the message.</param>
    /// <param name="colour">Colour of the message.</param>
    internal static void Send(object message, LogLevel level = LogLevel.Info, ConsoleColor colour = ConsoleColor.Gray) =>
        SendRaw($"[{level.ToString().ToUpper()}] [KittsMenuSystem] {message}", colour);

    /// <summary>
    /// Sends a raw message to the console.
    /// </summary>
    /// <param name="message">The message to be sent.</param>
    /// <param name="colour">Colour of the message.</param>
    internal static void SendRaw(object message, ConsoleColor colour = ConsoleColor.Gray) =>
        ServerConsole.AddLog(message.ToString(), colour);
}
