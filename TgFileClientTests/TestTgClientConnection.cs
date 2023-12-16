using Microsoft.Extensions.Configuration;

namespace TgFileClientTests;

public class TestTgClientConnection
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly string _token;
    private readonly long _chatId;

    public TestTgClientConnection(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        var config = new ConfigurationBuilder()
            .AddUserSecrets<TestTgClientConnection>()
            .Build();
        _token = config["BotToken"] ?? throw new Exception("BotToken is not set");
        var chatId = config["ChatId"] ?? throw new Exception("ChatId is not set");
        _chatId = long.Parse(chatId);
    }

    [Fact]
    public async Task Auth()
    {
        var client = new FileBotClient(_token, _chatId);

        await client.InitAsync();
        Assert.True(client.IsAuthorized);
        var id = long.Parse(_token.Split(':')[0]);
        Assert.Equal(id, client.Id);
        _testOutputHelper.WriteLine(client.Username);
    }

    [Fact]
    public async Task<Message> Upload()
    {
        var client = new FileBotClient(_token, _chatId);
        await client.InitAsync();

        var randomBytes = new byte[1024 * 1024];
        Random.Shared.NextBytes(randomBytes);
        await using var stream = new MemoryStream(randomBytes);
        var message = await client.SendDocumentAsync(stream, "test.txt", cancellationToken: CancellationToken.None);
        Assert.NotNull(message);
        Assert.NotNull(message.Document);
        Assert.Equal("test.txt", message.Document.FileName);
        Assert.Equal(stream.Length, message.Document.FileSize);
        return message;
    }

    [Fact]
    public async Task Download()
    {
        var client = new FileBotClient(_token, _chatId);
        await client.InitAsync();

        var randomBytes = new byte[1024 * 1024];
        Random.Shared.NextBytes(randomBytes);
        await using var stream = new MemoryStream(randomBytes);
        var message = await client.SendDocumentAsync(stream, "test.txt", cancellationToken: CancellationToken.None);
        Assert.NotNull(message);
        Assert.NotNull(message.Document);
        Assert.Equal("test.txt", message.Document.FileName);
        Assert.Equal(stream.Length, message.Document.FileSize);

        var fileInfo = await client.GetFile(message.Document.FileId, CancellationToken.None);
        Assert.NotNull(fileInfo);
        Assert.Equal(stream.Length, fileInfo.FileSize);

        await using var downloadedStream = new MemoryStream();
        await client.DownloadFile(downloadedStream, fileInfo.FilePath, cancellationToken: CancellationToken.None);
        Assert.Equal(stream.Length, downloadedStream.Length);
        Assert.Equal(stream.ToArray(), downloadedStream.ToArray());
    }
    
}