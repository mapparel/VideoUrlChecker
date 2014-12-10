using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VideoUrlChecker.KidsFilms.Biz;

namespace VideoUrlChecker
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("*************** Press any key start****************************");
            Console.ReadKey();
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Choose an option:\n1\tGet All Films\n2\tGet All Episodes\n3\tLook For Dead Video Links\n4\tDelete Broken Video Links\n5\tCheck Single Video Link\n0\tExit");
                Console.WriteLine();
                var s = Console.ReadLine();
                Console.WriteLine();
                var switchCase = int.Parse(s);
                switch (switchCase)
                {
                    case 0:
                        Environment.Exit(0);
                        break;
                    case 1:
                        var taskFilms = KFservice.GetAllFilmsAsync();
                        //while(!taskFilms.IsCompleted)
                        while (!taskFilms.IsCompleted)
                        {
                            ShowLoader(" Loading Films... ");
                        }
                        break;
                    case 2:
                        var taskEpisodes = KFservice.GetAllEpisodesAsync();
                        while (!taskEpisodes.IsCompleted)
                        {
                            ShowLoader(" Loading Episodes...  ");
                        }
                        break;
                    case 3:
                        var taskLinksChecker = KFservice.CheckAllVideoLinksAsync();
                        while (!taskLinksChecker.IsCompleted)
                        {
                            ShowLoader(" Checking Video Links... ");
                        }
                        break;
                    case 4:
                        //KFservice.DeleteBrokenEpisodeVideoLinks();
                        var taskLinksDeleter = KFservice.DeleteBrokenEpisodeVideoLinksAsync();
                        while (!taskLinksDeleter.IsCompleted)
                        {
                            Console.WriteLine(taskLinksDeleter.Status);
                            ShowLoader(" Deleting Broken Video Links...  ");
                        }
                        Console.WriteLine();
                        Console.WriteLine(taskLinksDeleter.Status);
                        List<Episode> deletedEpisodes = taskLinksDeleter.Result;
                        
                        if (deletedEpisodes == null)
                        {
                            Console.WriteLine("No episodes to delete");
                        }
                        else
                        {
                            Console.WriteLine(deletedEpisodes.Count() + " episodes were deleted:");
                            KFservice.SetConsolColorBody();
                            var printDeletedEpisodes = String.Format("\t{0,-50}{1,-50}{2,-50}", deletedEpisodes.Select(e => e.EpisodeTitle), deletedEpisodes.Select(e => e.Url), deletedEpisodes.Select(e => e.Film.FilmTitle));
                            Console.WriteLine(printDeletedEpisodes);
                        }
                        break;
                    case 5:
                        Console.WriteLine("VideoId To Check");
                        var input = Console.ReadLine();
                        var provider = "vimeo";
                        //var vidId = int.Parse(input);
                        var output = KFservice.CheckVideoLinkResponseStatus(provider, input);
                        Console.WriteLine(output.ToString());
                        break;
                }
            }
        }

        private static void ShowLoader(string message)
        {
            Console.Write("\r {0} ", message + "--");
            Thread.Sleep(250);
            Console.Write("\r {0} ", message + "\\ ");
            Thread.Sleep(250);
            Console.Write("\r {0} ", message + "| ");
            Thread.Sleep(250);
            Console.Write("\r {0} ", message + "/ ");
            Thread.Sleep(250);
        }
    }

    public class KFservice
    {
        private static readonly KidsFilmsEntities context = new KidsFilmsEntities();

        public static async Task GetAllFilmsAsync()
        {
            Console.WriteLine("Please wait, calling database...");
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine();

            var films = await context.Film.ToListAsync();
            Console.WriteLine();
            Console.WriteLine();
            SetConsolColorSuccess();
            Console.WriteLine(" Data recieved.");
            Console.WriteLine();
            SetConsolColorHeader();
            var header = String.Format("{0,-50}{1,-50}{2,-50}", "FilmTitle", "Url Id", "Is Private");
            Console.WriteLine(header);
            Console.ResetColor();
            foreach (var film in films)
            {
                var responseBody = String.Format("{0,-50}{1,-50}{2,-50}", film.FilmTitle, film.ThumbUrl,
                    film.IsPrivate);
                SetConsolColorBody();
                Console.WriteLine(responseBody);
            }
            Console.ResetColor();
            Console.WriteLine();
        }

        public static async Task GetAllEpisodesAsync()
        {
            Console.WriteLine("Please wait, calling database...");
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine();

            var episodes = await context.Episode.ToListAsync();
            Console.WriteLine();
            Console.WriteLine();
            SetConsolColorSuccess();
            Console.WriteLine(" Data recieved.");
            Console.WriteLine();
            SetConsolColorHeader();
            var header = String.Format("\t{0,-50}{1,-50}{2,-50}", "EpisodeTitle", "Url Id", "Is Private");
            Console.WriteLine(header);
            Console.ResetColor();
            foreach (var episode in episodes)
            {
                var responseBody = String.Format("\t{0,-50}{1,-50}{2,-50}", episode.EpisodeTitle, episode.Url, episode.IsActiveLink);
                SetConsolColorBody();
                Console.WriteLine(responseBody);
            }
            Console.ResetColor();
            Console.WriteLine();
        }

        //********************Find Broken Links**********************************************************
        public static async Task CheckAllVideoLinksAsync()
        {
            int counterChecked = 0;
            int counterDead = 0;
            Console.WriteLine("Please wait, marking dead video links...");
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine();
            var episodes = await context.Episode.ToListAsync();
            var count = episodes.Count();
            foreach (var episode in episodes)
            {
                counterChecked++;
                //var percent = (counterChecked/count)*100;
                //Console.Write("\r {0} links checked", counterChecked);
                var videoCheck = CheckVideoLinkResponseStatus(episode.Film.Provider.ProviderName, episode.Url);

                if (videoCheck.HasValue && (bool)!videoCheck)
                {
                    counterDead++;
                    await MarkLinkAsBrokenAsync(episode.EpisodeId);
                }
            }
            Console.WriteLine();
            Console.WriteLine("{0} video links were checked. ", counterChecked);
            Console.WriteLine();
            Console.WriteLine("{0} video links marked as inactive. ", counterDead);
            Console.ResetColor();
        }

        public static bool? CheckVideoLinkResponseStatus(string provider, string videoId)
        {
            string url = string.Empty;

            switch (provider)
            {
                case "youtube":
                    url = String.Format("http://gdata.youtube.com/feeds/api/videos/{0}", videoId);
                    break;
                case "vimeo":
                    url = String.Format("http://vimeo.com/api/v2/video/{0}.json", videoId);
                    break;
            }

            HttpWebResponse response = null;

            var request = WebRequest.Create(url) as HttpWebRequest;
            if (request == null) return null;
            request.Method = "HEAD";
            request.Timeout = 5000; // milliseconds
            request.AllowAutoRedirect = false;

            var exists = false;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                exists = response.StatusCode == HttpStatusCode.OK;
            }
            catch
            {
                exists = false;
            }
            finally
            {
                if (response != null)
                    response.Close();
            }
            return exists;
        }

        public static async Task MarkLinkAsBrokenAsync(int episodeId)
        {
            var episode = await context.Episode.FindAsync(episodeId);
            episode.IsActiveLink = false;
            context.Entry(episode).State = EntityState.Modified;
            await context.SaveChangesAsync();
            var output = String.Format("\t{0,-35}{1,-35}{2,-35}{3,-35}{4,-35}", episode.EpisodeId, episode.EpisodeTitle, episode.Film.FilmTitle, episode.Url, episode.IsActiveLink);
            SetConsolColorBody();
            Console.WriteLine(output);
        }
        //************************************************************************************

        //************************Delete Broken Links******************************************
        //public static async Task DeleteBrokenEpisodeVideoLinksAsync()
        //{
        //    var episodesToDelete = context.Episode.Where(e => !e.IsActiveLink);
        //    if (episodesToDelete.Count() != 0)
        //    {
        //        foreach (var episode in episodesToDelete)
        //        {
        //            await DeleteAsync(episode.EpisodeId);
        //        }
        //    }
        //}
        public static async Task<List<Episode>> DeleteBrokenEpisodeVideoLinksAsync()
        {
            var episodesToDelete = context.Episode.Where(e => e.IsActiveLink == false);
            if (!episodesToDelete.Any()) return (null);
            foreach (var episode in episodesToDelete)
            {
                context.Episode.Remove(episode);
            }
            await context.SaveChangesAsync();
            await return (episodesToDelete.ToListAsync());
        }
        //public static async Task<string> DeleteAsync(int id)
        //{
        //    Episode episode = await context.Episode.FindAsync(id);
        //    var title = episode.EpisodeTitle;
        //    context.Episode.Remove(episode);
        //    await context.SaveChangesAsync();
        //    Console.WriteLine("Episode {0} deleted successfully", title);
        //    return String.Format("Episode {0} deleted successfully", title);
        //}
        //public static void DeleteBrokenEpisodeVideoLinks()
        //{
        //    var episodesToDelete = context.Episode.Where(e => e.IsActiveLink == false);
        //    if (episodesToDelete.Count() != 0)
        //    {
        //        foreach (var episode in episodesToDelete)
        //        {
        //            var title = episode.EpisodeTitle;
        //            context.Episode.Remove(episode);
        //            Console.WriteLine("Episode {0} deleted successfully", title);
        //        }
        //        context.SaveChanges();
        //    }
        //}

        //************************************************************************************
        public static void SetConsolColorHeader()
        {
            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static void SetConsolColorBody()
        {
            Console.BackgroundColor = ConsoleColor.Gray;
            Console.ForegroundColor = ConsoleColor.Black;
        }
        public static void SetConsolColorSuccess()
        {
            Console.ForegroundColor = ConsoleColor.Green;
        }
    }

}
