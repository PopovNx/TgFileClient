using System.Text.Json.Serialization;

namespace TgFileClient.Models;

internal sealed record BotInfo(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("first_name")]
    string FirstName,
    [property: JsonPropertyName("username")]
    string Username);