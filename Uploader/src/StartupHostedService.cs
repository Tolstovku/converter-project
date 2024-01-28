using System.Text.Json;
using Common;
using Common.Database;
using Microsoft.EntityFrameworkCore;

namespace Converter;

public class StartupHostedService : IHostedService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly IMessageQueueClient _messageQueueClient;

    public StartupHostedService(IDbContextFactory<AppDbContext> dbFactory, IMessageQueueClient messageQueueClient)
    {
        _dbFactory = dbFactory;
        _messageQueueClient = messageQueueClient;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var filesNotSentForConverting = db.Files.Where(x => x.Status == FileStatus.Uploaded).ToList();
        foreach (var file in filesNotSentForConverting)
        {
            var msg = new ConvertFileMessage()
            {
                FileId = file.Id
            };
            await _messageQueueClient.SendMessageAsync(JsonSerializer.Serialize(msg), FileQueueConstants.ConvertFilesQueueName);
            file.Status = FileStatus.Converting;
            db.Update(file);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}