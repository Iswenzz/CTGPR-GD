using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using CTGPR.Downloader.Data;
using CTGPR.Downloader.Progress;

namespace CTGPR.Downloader
{
    public static class Downloader
    {
        private static HttpClient HTTP { get; set; } = new();
        private static ProgressBase Progress { get; set; }
        public static int Downloaded { get; set; }

        public static async Task Leaderboards()
        {
            if (!Directory.Exists("ghosts"))
                Directory.CreateDirectory("ghosts");

            List<Leaderboard> entries = [
                new("150cc Original Tracks", "http://tt.chadsoft.co.uk/original-track-leaderboards.json"),
                new("200cc Original Tracks", "http://tt.chadsoft.co.uk/original-track-leaderboards-200cc.json"),
                new("150cc CTGP Tracks", "http://tt.chadsoft.co.uk/ctgp-leaderboards.json"),
                new("200cc CTGP Tracks", "http://tt.chadsoft.co.uk/ctgp-leaderboards-200cc.json")
            ];

            foreach (var entry in entries)
            {
                Progress?.Dispose();

                Console.Clear();
                Console.WriteLine("CTGP-R Ghost Downloader (c) Iswenzz 2024");

                if (AskUser($"Download {entry.Name} ? [y/n]"))
                {
                    Console.Clear();
                    Progress = new(entry.Name);

                    string json = await HTTP.GetStringAsync(entry.URL);
                    await Tracks(JObject.Parse(json));
                }
            }
            Console.WriteLine($"Successfully downloaded {Downloaded} ghosts.");
        }

        public static async Task Tracks(JObject leaderboards)
        {
            var tracks = leaderboards["leaderboards"].ToList();
            Progress.MaxTick = tracks.Count;
            foreach (var track in tracks) await Track(track);
            Progress.Message = $"{Progress.Title}: {Progress.CurrentTick}/{Progress.MaxTick}";
        }

        public static async Task Track(JToken track)
        {
            Progress.Message = $"{Progress.Title}: {Progress.CurrentTick}/{Progress.MaxTick}";
            using var progress = new ProgressDownload(track["name"].ToString(), Progress);
            string json = await HTTP.GetStringAsync($"http://tt.chadsoft.co.uk{track["_links"]["item"]["href"]}?start=0&limit=1&times=pb");
            JObject data = JObject.Parse(json);

            var ghost = data["ghosts"][0];
            await progress.Download($"http://tt.chadsoft.co.uk{ghost["href"]}", "ghosts", $"{ghost["hash"]}.rkg");
            Downloaded++;

            Progress.Tick(Progress.CurrentTick + 1);
        }

        public static bool AskUser(string message)
        {
            Console.WriteLine();
            Console.WriteLine(message);
            string response = Console.ReadLine();

            if (!string.IsNullOrEmpty(response))
            {
                if (response.Trim().ToLower() is "y" or "yes")
                    return true;
                if (response.Trim().ToLower() is "n" or "no")
                    return false;
            }
            return true;
        }

        public static void Shutdown()
        {
            Progress?.Dispose();
            HTTP.Dispose();
        }
    }
}
