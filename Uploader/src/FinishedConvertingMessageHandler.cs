using System.Text.Json;
using Common;
using Common.Database;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace Converter;

public class FinishedConvertingMessageHandler : IHostedService
{
    private readonly IHubContext<FileStatusHub> _hubContext;
    private readonly IMessageQueueClient _messageQueueClient;
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public FinishedConvertingMessageHandler(IMessageQueueClient messageQueueClient, IDbContextFactory<AppDbContext> dbFactory, IHubContext<FileStatusHub> hubContext)
    {
        _messageQueueClient = messageQueueClient;
        _dbFactory = dbFactory;
        _hubContext = hubContext;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _messageQueueClient.SubscribeAsync(FileQueueConstants.FinishedConvertingQueueName,
            msg => ConvertFileAsync(JsonSerializer.Deserialize<ConvertFileMessage>(msg)!).GetAwaiter().GetResult());
    }

    private async Task ConvertFileAsync(ConvertFileMessage msg)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var file = db.Files.AsNoTracking().Select(x => new {x.Id, x.SessionId, x.Filename, x.Status}).First(x => x.Id == msg.FileId);
            await _hubContext.Clients.Group(file.SessionId).SendAsync(FileStatusHub.ReceiveFileUpdateMethod,
                JsonSerializer.Serialize(new {file.Id, file.Status}));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}