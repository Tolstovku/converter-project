using System.Text.Json;
using Common.Database;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Converter;

public class FileStatusHub : Hub
{
    public const string ReceiveFilesMethod = "ReceiveFilesInitial";
    public const string ReceiveFileUpdateMethod = "ReceiveFileUpdate";
    private readonly AppDbContext _dbContext;

    public FileStatusHub(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.GetHttpContext()!.User.GetUserId();
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);

            var files = _dbContext.Files.Where(x => x.SessionId == userId && x.Status != FileStatus.Deleted)
                .Select(x => FileStatusItem.FromFileEntity(x)).ToList();
            await Clients.Caller.SendAsync(ReceiveFilesMethod, JsonSerializer.Serialize(files));
        }

        await base.OnConnectedAsync();
    }
}