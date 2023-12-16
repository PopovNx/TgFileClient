using System.Text.Json.Serialization;

namespace TgFileClient.Models;

/// <summary>
/// This object represents a general telegram file.
/// </summary>
/// <param name="FileId">Unique, bot-dependent identifier for this file. You can get download path with <see cref="FileBotClient.GetFile"/> method.</param>
/// <param name="FileUniqueId">Unique identifier for this file, which is supposed to be the same over time and for different bots. Can't be used to download or reuse the file.</param>
/// <param name="FileName">Optional. Original filename as defined by sender.</param>
/// <param name="MimeType">Optional. MIME type of the file as defined by sender.</param>
/// <param name="FileSize">Optional. File size.</param>
[Serializable]
public sealed record Document(
    [property: JsonPropertyName("file_id")]
    string FileId,
    [property: JsonPropertyName("file_unique_id")]
    string FileUniqueId,
    [property: JsonPropertyName("file_name")]
    string? FileName,
    [property: JsonPropertyName("mime_type")]
    string? MimeType,
    [property: JsonPropertyName("file_size")]
    long? FileSize);