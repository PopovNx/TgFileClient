namespace TgFileClient.Exceptions;

/// <summary>
///  Represents an exception thrown when an unknown error is received from Telegram.
/// </summary>
/// <param name="code">Error code</param>
/// <param name="description">Description of the error</param>
public sealed class UnknownBotErrorException(int code, string description) : Exception(
    $"Unknown error: [{code}] {description}");