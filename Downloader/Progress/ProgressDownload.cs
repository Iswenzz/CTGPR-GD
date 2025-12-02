using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace CTGPR.Downloader.Progress
{
    public class ProgressDownload(string title, ProgressBase parent = null) : ProgressBase(title, parent), IDisposable
    {
        private HttpClient HTTP { get; set; } = new();

        public async Task Download(string url, string folder, string file)
        {
            using var response = await HTTP.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            var filePath = Path.Combine(folder, file);
            var fileSize = response.Content.Headers.ContentLength ?? 0;
            Bar.MaxTicks = Convert.ToInt32(fileSize);

            using var stream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            var buffer = new byte[512];
            int bytes = 0;
            int read = 0;

            while ((read = await stream.ReadAsync(buffer)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory());
                bytes += read;
                Tick(bytes);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            HTTP?.Dispose();
        }
    }
}
