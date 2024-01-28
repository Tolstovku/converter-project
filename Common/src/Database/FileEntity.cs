using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Database;

public class FileEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Column("session_id")]
    public string SessionId { get; set; }
    
    [Column("filename")]
    public string Filename { get; set; }
    
    [Column("status")]
    public FileStatus Status { get; set; }
    
    [Column("html_file")]
    public byte[]? HtmlFile { get; set; }
    
    [Column("converted_pdf_file")]
    public byte[]? ConvertedPdfFile { get; set; }
}