using System.Text.Json;
using Common;
using Common.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace Converter;

public class ConverterService : IHostedService
{
    private readonly IMessageQueueClient _messageQueueClient;
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly AppSettings _appSettings;

    public ConverterService(IMessageQueueClient messageQueueClient, IDbContextFactory<AppDbContext> dbFactory, IOptions<AppSettings> appSettings)
    {
        _messageQueueClient = messageQueueClient;
        _dbFactory = dbFactory;
        _appSettings = appSettings.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _messageQueueClient.SubscribeAsync(FileQueueConstants.ConvertFilesQueueName,
            msg => ConvertFileAsync(JsonSerializer.Deserialize<ConvertFileMessage>(msg)!).GetAwaiter().GetResult());
    }

    private async Task ConvertFileAsync(ConvertFileMessage msg)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            var file = db.Files.First(x => x.Id == msg.FileId);
            if (file.Status != FileStatus.Converting)
                return;
            var html = System.Text.Encoding.UTF8.GetString(file.HtmlFile!);

            var pdfOptions = new PdfOptions
            {
                Format = PaperFormat.A4
            };

            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                ExecutablePath = _appSettings.ChromeExecutablePath
            });
            await using var page = await browser.NewPageAsync();
            await page.SetContentAsync(html);
            var pdfBytes = await page.PdfDataAsync(pdfOptions);
            file.HtmlFile = null;
            file.ConvertedPdfFile = pdfBytes;
            file.Status = FileStatus.Converted;
            db.Update(file);
            await db.SaveChangesAsync();
            await _messageQueueClient.SendMessageAsync(JsonSerializer.Serialize(msg), FileQueueConstants.FinishedConvertingQueueName);
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