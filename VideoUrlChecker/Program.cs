using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
                Console.WriteLine("Choose an option:\n1\tGet All Films\n2\tGet All Episodes\n3\tLook for dead video links\n4\tCheckSingleVideo\n0\tExit");
                Console.WriteLine();
                var s = Console.ReadLine();
                Console.WriteLine();
                var switchCase = int.Parse(s);
                var kf = new KFservice();
                switch (switchCase)
                {
                    case 0:
                        Environment.Exit(0);
                        break;
                    case 1:
                        var taskFilms = kf.GetAllFilmsAsync();
                        //while(!taskFilms.IsCompleted)
                        while (!taskFilms.IsCompleted)
                        {
                            ShowLoader(" Films  ");
                        }
                        break;
                    case 2:
                        var taskEpisodes = kf.GetAllEpisodesAsync();
                        while (!taskEpisodes.IsCompleted)
                        {
                            ShowLoader(" Episodes  ");
                        }
                        break;
                    case 3:
                        //var taskLinksChecker = kf.CheckAllVideoLinksAsync();
                        //while (!taskLinksChecker.IsCompleted)
                        //{
                        //    ShowLoader(" Video Links  ");
                        //}
                        kf.CheckAllVideoLinks();
                        break;
                    case 4:
                        Console.WriteLine("VideoId to check");
                        var input = Console.ReadLine();
                        var provider = "vimeo";
                        //var vidId = int.Parse(input);
                        var output = kf.RemoteVideoExists(provider, input);
                        Console.WriteLine(output.ToString());
                        break;
                }
            }
        }

        private static void ShowLoader(string entity)
        {
            Console.Write("\r {0} ", "Loading" + entity + "--");
            Thread.Sleep(250);
            Console.Write("\r {0} ", "Loading" + entity + "\\");
            Thread.Sleep(250);
            Console.Write("\r {0} ", "Loading" + entity + "|");
            Thread.Sleep(250);
            Console.Write("\r {0} ", "Loading" + entity + "/");
            Thread.Sleep(250);
        }
    }

    class KFservice
    {
        private readonly KidsFilmsEntities _context = new KidsFilmsEntities();

        public async Task GetAllFilmsAsync()
        {
            Console.WriteLine("Please wait, calling database...");
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine();

            var films = await _context.Film.ToListAsync();
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

        public async Task GetAllEpisodesAsync()
        {
            Console.WriteLine("Please wait, calling database...");
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine();

            var episodes = await _context.Episode.ToListAsync();
            Console.WriteLine();
            Console.WriteLine();
            SetConsolColorSuccess();
            Console.WriteLine(" Data recieved.");
            Console.WriteLine();
            SetConsolColorHeader();
            var header = String.Format("{0,-50}{1,-50}{2,-50}", "EpisodeTitle", "Url Id", "Is Private");
            Console.WriteLine(header);
            Console.ResetColor();
            foreach (var episode in episodes)
            {
                var responseBody = String.Format("{0,-50}{1,-50}{2,-50}", episode.EpisodeTitle, episode.Url, episode.IsActiveLink);
                SetConsolColorBody();
                Console.WriteLine(responseBody);
            }
            Console.ResetColor();
            Console.WriteLine();
        }

        public void CheckAllVideoLinks()
        {
            int counter = 0;
            Console.WriteLine("Please wait, marking dead video links...");
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine();
            var episodes = _context.Episode.ToList();
            foreach (var episode in episodes)
            {
                counter++;
                Console.Write("\r {0} ", counter);
                var videoCheck = RemoteVideoExists(episode.Film.Provider.ProviderName, episode.Url);

                if (videoCheck.HasValue && (bool)!videoCheck)
                {
                    Console.WriteLine("Found dead link: episodeTitle = " + episode.EpisodeTitle);
                    SetIsActive(episode.EpisodeId);
                }
            }
        }

        public bool? RemoteVideoExists(string provider, string videoId)
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

        public void SetIsActive(int episodeId)
        {

            var episode = _context.Episode.Find(episodeId);
            episode.IsActiveLink = false;
            _context.Entry(episode).State = EntityState.Modified;
            _context.SaveChanges();

            var output = String.Format("{0,-50}{1,-50}{2,-50}", episode.EpisodeTitle, episode.Url, episode.IsActiveLink);
            SetConsolColorBody();
            Console.WriteLine(output);
        }

        private void SetConsolColorHeader()
        {
            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.ForegroundColor = ConsoleColor.White;
        }
        private void SetConsolColorBody()
        {
            Console.BackgroundColor = ConsoleColor.Gray;
            Console.ForegroundColor = ConsoleColor.Black;
        }
        private void SetConsolColorSuccess()
        {
            Console.ForegroundColor = ConsoleColor.Green;
        }
    }

}
