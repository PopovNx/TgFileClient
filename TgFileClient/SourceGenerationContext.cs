using System.Text.Json.Serialization;
using TgFileClient.Models;

namespace TgFileClient;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ApiResponse<BotInfo>))]
[JsonSerializable(typeof(ApiResponse<Message>))]
[JsonSerializable(typeof(ApiResponse<TelegramFile>))]
[JsonSerializable(typeof(ApiResponse<object>))]
internal partial class SourceGenerationContext: JsonSerializerContext;