using System.Text;
using System.Text.Json;
using Common;
using Common.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Converter.Controllers;

[SimpleAuthAuthorize]
[ApiController]
[Route("/api/v1")]
public class FilesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IMessageQueueClient _messageQueueClient;

    public FilesController(AppDbContext db, IMessageQueueClient messageQueueClient)
    {
        _db = db;
        _messageQueueClient = messageQueueClient;
    }
    
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Invalid file");

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var fileData = memoryStream.ToArray();

        var fileEntity = new FileEntity()
        {
            Filename = file.FileName,
            SessionId = User.GetUserId()!,
            Status = FileStatus.Uploaded,
            HtmlFile = fileData
        };
        _db.Add(fileEntity);
        await _db.SaveChangesAsync();
        var msg = new ConvertFileMessage()
        {
            FileId = fileEntity.Id
        };
        await _messageQueueClient.SendMessageAsync(JsonSerializer.Serialize(msg), FileQueueConstants.ConvertFilesQueueName);
        fileEntity.Status = FileStatus.Converting;
        _db.Update(fileEntity);
        await _db.SaveChangesAsync();

        return Ok(JsonSerializer.Serialize(FileStatusItem.FromFileEntity(fileEntity)));
    }
    
    [HttpGet("file/{fileId:int}")]
    public IActionResult DownloadFile([FromRoute] int fileId)
    {
        var convertedFile = _db.Files.AsNoTracking()
            .FirstOrDefault(x => x.SessionId == User.GetUserId() && x.Id == fileId && x.Status == FileStatus.Converted);
        if (convertedFile is null)
            return BadRequest("File not found");

        var fileNamePdf = convertedFile.Filename.Split(".")[0] + ".pdf";
        var contentType = "application/octet-stream";
        return File(convertedFile.ConvertedPdfFile!, contentType, fileNamePdf);
    }
    
    [HttpDelete("file/{fileId:int}")]
    public IActionResult DeleteFile([FromRoute] int fileId)
    {
        var fileExists = _db.Files.Any(x => x.Id == fileId && x.SessionId == User.GetUserId() && x.Status != FileStatus.Deleted);
        if (!fileExists)
            return BadRequest();
        
        _db.Files.Where(x => x.Id == fileId)
            .ExecuteUpdate(s =>
            s.SetProperty(x => x.Status, FileStatus.Deleted)
                .SetProperty(x => x.ConvertedPdfFile, (byte[]?) null)
                .SetProperty(x => x.HtmlFile, (byte[]?) null)
        );
        
        return Ok();
    }
}