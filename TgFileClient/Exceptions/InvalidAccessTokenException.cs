namespace TgFileClient.Exceptions;

/// <summary>
///  Represents an exception thrown when trying initialize a bot client with an invalid access token.
/// </summary>
public sealed class InvalidAccessTokenException() : Exception("Invalid access token");