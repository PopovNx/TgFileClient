using System.Text.Json.Serialization;

namespace TgFileClient.Models;

/// <summary>
/// This object represents a telegram file.
/// </summary>
/// <param name="FileId">Unique, bot-dependent identifier for this file. You can get download path with <see cref="IBotClient.GetFile"/> method.</param>
/// <param name="FileUniqueId">Unique identifier for this file, which is supposed to be the same over time and for different bots. Can't be used to download or reuse the file.</param>
/// <param name="FileSize">File size.</param>
/// <param name="FilePath">File path. Use <see cref="IBotClient.DownloadFile"/> method to download this file.</param>
public sealed record TelegramFile(
    [property: JsonPropertyName("file_id")]
    string FileId,
    [property: JsonPropertyName("file_unique_id")]
    string FileUniqueId,
    [property: JsonPropertyName("file_size")]
    long FileSize,
    [property: JsonPropertyName("file_path")]
    string FilePath);