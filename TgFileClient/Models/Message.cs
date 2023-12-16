using System.Text.Json.Serialization;

namespace TgFileClient.Models;

/// <summary>
/// This object represents a telegram message.
/// </summary>
/// <param name="MessageId">Unique message identifier inside this chat.</param>
/// <param name="Date">Date the message was sent in Unix time.</param>
/// <param name="Document">Optional. Information about the file. More about <see cref="Document"/>.</param>
[Serializable]
public sealed record Message(
    [property: JsonPropertyName("message_id")]
    long MessageId,
    [property: JsonPropertyName("date")] long Date,
    [property: JsonPropertyName("document")]
    Document? Document);