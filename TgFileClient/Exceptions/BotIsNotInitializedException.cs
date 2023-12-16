namespace TgFileClient.Exceptions;

/// <summary>
///   Represents an exception thrown when trying to use a bot client that is not initialized.
/// </summary>
public sealed class BotIsNotInitializedException() : InvalidOperationException("Bot is not initialized");
