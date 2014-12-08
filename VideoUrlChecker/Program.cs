using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
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
                Console.WriteLine("Choose an option:\n1\tGet All Films\n2\tGet All Episodes");
                Console.WriteLine();
                string s = Console.ReadLine();
                Console.WriteLine();
                int switchCase = int.Parse(s);
                var kf = new KFservice();
                switch (switchCase)
                {
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
                        while(!taskEpisodes.IsCompleted)
                        {
                            ShowLoader(" Episodes  ");
                        }
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
