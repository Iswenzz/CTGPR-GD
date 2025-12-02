using System;
using System.Threading.Tasks;

namespace CTGPR.Downloader
{
    public static class Program
    {
        public static async Task Main()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Title = "CTGP-R Ghost Downloader";

            await Downloader.Leaderboards();
            Downloader.Shutdown();

            Console.WriteLine(Environment.NewLine + "Press ENTER to continue . . .");
            Console.ReadLine();
        }
    }
}
