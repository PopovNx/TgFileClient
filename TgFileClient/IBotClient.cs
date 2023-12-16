using TgFileClient.Exceptions;
using TgFileClient.Models;

namespace TgFileClient;

/// <summary>
/// Represents a Telegram bot client.
/// </summary>
public interface IBotClient
{
    /// <summary>
    /// Gets a value indicating whether the bot client is authorized.
    /// </summary>
    public bool IsAuthorized { get; }

    /// <summary>
    /// Gets the ID of the initialized bot.
    /// </summary>
    public long Id { get; }

    /// <summary>
    /// Gets the username of the initialized bot.
    /// </summary>
    public string Username { get; }

    
}