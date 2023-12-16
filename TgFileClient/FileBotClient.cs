using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using TgFileClient.Exceptions;
using TgFileClient.Models;

namespace TgFileClient;

/// <summary>
/// Represents a client for interacting with the Telegram Bot API
/// </summary>
public sealed class FileBotClient : IBotClient
{
    private const string ApiUrl = "https://api.telegram.org/bot{0}/";
    private const int ProgressCallbackDelay = 25;
    private const int MaxFileSize = 20 * 1024 * 1024;

    private string BaseUrl { get; }
    private string ChatId { get; }

    private long? _id;
    private string? _username;
    private readonly string _token;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileBotClient"/> class.
    /// </summary>
    /// <param name="token">The Telegram bot token.</param>
    /// <param name="chatId">The chat id for sending files.</param>
    public FileBotClient(string token, long chatId)
    {
        _token = token;
        BaseUrl = string.Format(ApiUrl, token);
        ChatId = chatId.ToString();
    }


    /// <summary>
    /// Gets a value indicating whether the bot client is authorized.
    /// </summary>
    public bool IsAuthorized => _id is not null && _username is not null;

    /// <summary>
    /// Gets the ID of the initialized bot.
    /// </summary>

    public long Id => _id ?? throw new BotIsNotInitializedException();

    /// <summary>
    /// Gets the username of the initialized bot.
    /// </summary>

    public string Username => _username ?? throw new BotIsNotInitializedException();


    private static async Task ProgressReporter(Stream stream, ProgressCallback? callback,
        CancellationToken cancellationToken = default)
    {
        if (callback is null)
            return;

        long previousPosition = -1;
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(ProgressCallbackDelay, cancellationToken);
            var progress = stream.Position / (double)stream.Length;
            if (stream.Position == stream.Length)
                progress = 1;
            if (stream.Position == previousPosition)
                continue;
            previousPosition = stream.Position;

            callback?.Invoke(stream.Position, stream.Length, progress);
        }
    }


    private async Task<T> SendRequestAsync<T>(string method, JsonTypeInfo<T> responseTypeInfo,
        Dictionary<string, string>? parameters = null,
        CancellationToken cancellationToken = default)

    {
        var url = $"{BaseUrl}{method}";
        using var httpClient = new HttpClient();
        using var content = new FormUrlEncodedContent(parameters ?? new Dictionary<string, string>());

        var response = await httpClient.PostAsync(url, content, cancellationToken: cancellationToken);
        var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize(responseString, responseTypeInfo);
        if (result is null)
            throw new UnknownBotErrorException(-1, "result is null");
        return result;
    }

    private Task<ApiResponse<BotInfo>> GetMe(CancellationToken cancellationToken = default)
    {
        const string method = "getMe";
        return SendRequestAsync(method, SourceGenerationContext.Default.ApiResponseBotInfo,
            cancellationToken: cancellationToken);
    }

    private void ValidateSendFileRequest(Stream stream)
    {
        if (!IsAuthorized)
            throw new BotIsNotInitializedException();

        switch (stream)
        {
            case { CanSeek: false }:
                throw new ArgumentException("File stream is not seekable", nameof(stream));
            case { Length: 0 }:
                throw new ArgumentException("Stream is empty", nameof(stream));
            case { Length: > MaxFileSize }:
                throw new ArgumentException("File size is more than 20 MB", nameof(stream));
            case null:
                throw new ArgumentException("Stream is null", nameof(stream));
            case { CanRead: false }:
                throw new ArgumentException("File stream is not readable", nameof(stream));
        }
    }

    /// <summary>
    /// Sends a document asynchronously to the specified chat.
    /// </summary>
    /// <param name="stream">The stream containing the document data.</param>
    /// <param name="fileName">The name of the document file.</param>
    /// <param name="progressCallback">The callback to track the upload progress.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>The sent document message.</returns>
    public async Task<Message> SendDocumentAsync(Stream stream, string fileName,
        ProgressCallback? progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        ValidateSendFileRequest(stream);

        const string method = "sendDocument";
        var url = $"{BaseUrl}{method}";

        using HttpClient httpClient = new();

        var syncStream = Stream.Synchronized(stream);
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(ChatId), "chat_id");
        content.Add(new StreamContent(syncStream), "document", WebUtility.UrlEncode(fileName));

        using var progressCts = new CancellationTokenSource();

        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, progressCts.Token);

        var fileUploadTrackingTask = ProgressReporter(syncStream, progressCallback, linkedCts.Token);

        try
        {
            var response = await httpClient.PostAsync(url, content, cancellationToken);
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
            await progressCts.CancelAsync();
            var apiResponse =
                JsonSerializer.Deserialize(responseString, SourceGenerationContext.Default.ApiResponseMessage);
            switch (apiResponse)
            {
                case null or { Ok: true, Result: null }:
                    throw new UnknownBotErrorException(-1, "result is null");
                case { Ok: false, ErrorCode: 400, Description: var description }:
                    throw new BadRequestTelegramException(400, description ?? "Unknown error");
                case { Ok: false, ErrorCode: var errorCode, Description: var description }:
                    throw new UnknownBotErrorException(errorCode ?? -1, description ?? "Unknown error");
            }

            return apiResponse.Result;
        }
        finally
        {
            await progressCts.CancelAsync();
            try
            {
                await fileUploadTrackingTask;
            }
            catch (OperationCanceledException)
            {
            }
        }
    }

    /// <summary>
    /// Gets information about a file from Telegram.
    /// </summary>
    /// <param name="fileId">The ID of the file to retrieve information about.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>The information about the Telegram file.</returns>
    public async Task<TelegramFile> GetFile(string fileId, CancellationToken cancellationToken = default)
    {
        if (!IsAuthorized)
            throw new BotIsNotInitializedException();
        var response = await SendRequestAsync("getFile", SourceGenerationContext.Default.ApiResponseTelegramFile,
            new Dictionary<string, string> { { "file_id", fileId } }, cancellationToken);
        switch (response)
        {
            case { Ok: false, ErrorCode: 400 }:
                throw new BadRequestTelegramException(response.ErrorCode ?? -1,
                    response.Description ?? "Unknown error");
            case { Ok: false }:
                throw new UnknownBotErrorException(response.ErrorCode ?? -1, response.Description ?? "Unknown error");
            case { Ok: true, Result: null }:
                throw new UnknownBotErrorException(-1, "GetFile result is null");
            case { Ok: true, Result.FilePath: null }:
                throw new UnknownBotErrorException(-1, "GetFile result.FilePath is null");
        }

        return response.Result;
    }


    /// <summary>
    /// Downloads a file from Telegram and writes it to the specified stream.
    /// </summary>
    /// <param name="stream">The stream to write the downloaded file data to.</param>
    /// <param name="filePath">The path of the file to download.</param>
    /// <param name="progressCallback">The callback to track the download progress.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    public async Task DownloadFile(Stream stream, string filePath,
        ProgressCallback? progressCallback = null, CancellationToken cancellationToken = default)
    {
        if (!IsAuthorized)
            throw new BotIsNotInitializedException();
        var url = $"https://api.telegram.org/file/bot{_token}/{filePath}";
        using var httpClient = new HttpClient();
        using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseData =
                JsonSerializer.Deserialize(responseString, SourceGenerationContext.Default.ApiResponseObject);
            throw new UnknownBotErrorException(responseData?.ErrorCode ?? -1,
                responseData?.Description ?? "Unknown error");
        }

        var syncStream = Stream.Synchronized(stream);
        using var progressCts = new CancellationTokenSource();
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, progressCts.Token);
        var fileDownloadTrackingTask = ProgressReporter(syncStream, progressCallback, linkedCts.Token);
        try
        {
            await response.Content.CopyToAsync(syncStream, cancellationToken);
            await progressCts.CancelAsync();
        }
        finally
        {
            await progressCts.CancelAsync();
            try
            {
                await fileDownloadTrackingTask;
            }
            catch (OperationCanceledException)
            {
            }
        }
    }


    /// <summary>
    ///    Initializes bot client
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <exception cref="InvalidAccessTokenException">Thrown when token is invalid</exception>
    /// <exception cref="UnknownBotErrorException">Thrown when unknown error occurred</exception>
    public async Task InitAsync(CancellationToken cancellationToken = default)
    {
        var me = await GetMe(cancellationToken);

        switch (me)
        {
            case { Ok: false, ErrorCode: 401 }:
                throw new InvalidAccessTokenException();
            case { Ok: false }:
                throw new UnknownBotErrorException(me.ErrorCode ?? -1, me.Description ?? "Unknown error");
            case { Ok: true, Result: null }:
                throw new UnknownBotErrorException(-1, "GetMe result is null");
        }

        _id = me.Result.Id;
        _username = me.Result.Username;
    }
}