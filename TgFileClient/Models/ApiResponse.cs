using System.Text.Json.Serialization;

namespace TgFileClient.Models;

[Serializable]
internal record ApiResponse<T>(
    [property: JsonPropertyName("ok")] bool Ok,
    [property: JsonPropertyName("result")] T? Result,
    [property: JsonPropertyName("error_code")]
    int? ErrorCode,
    [property: JsonPropertyName("description")]
    string? Description);