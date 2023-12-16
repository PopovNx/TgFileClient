namespace TgFileClient.Exceptions;

/// <summary>
///    Represents an exception thrown when a BadRequest response is received from Telegram.
/// </summary>
/// <param name="errorCode">Error code</param>
/// <param name="description">Description of the error</param>
public sealed class BadRequestTelegramException(int errorCode, string description) : Exception(
    $"[{errorCode}] {description}");