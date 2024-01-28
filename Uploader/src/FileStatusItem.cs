using Common.Database;

namespace Converter;

public class FileStatusItem
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public FileStatus Status { get; set; }

    public static FileStatusItem FromFileEntity(FileEntity fileEntity)
        => new()
        {
            Id = fileEntity.Id,
            FileName = fileEntity.Filename,
            Status = fileEntity.Status
        };
}