using System;
using Quartz;

namespace VideoUrlChecker
{
    public class LinksChecker:IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            var taskLinksChecker = KFservice.CheckAllVideoLinksAsync();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            Console.WriteLine("Please wait, looking for broken video links...");
            Console.WriteLine("----------------------------------------");

            taskLinksChecker.Wait();
            stopwatch.Stop();
            Console.WriteLine("Time elapsed while scanning: {0}", stopwatch.Elapsed);
            Console.WriteLine();
            var markedEpisodes = taskLinksChecker.Result;

            if (markedEpisodes != null)
            {
                KFservice.SetConsolColorBody();
                Console.WriteLine(markedEpisodes);
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("There are no episodes to mark for delete");
            }
            Console.WriteLine("\n");
        }
    }
}
