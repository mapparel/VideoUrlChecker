﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
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
            //********************QUARTZ TASK***********************************************************************************
            //Common.Logging.LogManager.Adapter = new Common.Logging.Simple.ConsoleOutLoggerFactoryAdapter { Level = Common.Logging.LogLevel.Info };
            //JobScheduler.Start();
            //********************************************************************************************************************

            Console.WriteLine("*************** Press any key start****************************");
            Console.ReadKey();
            Console.WriteLine("");
            while (true)
            {
                Console.WriteLine(":::::::::::::::::::::::::::::::::::::::::::");
                Console.WriteLine(":    Kids Films Maintenance Suite         :");
                Console.WriteLine(":::::::::::::::::::::::::::::::::::::::::::");
                Console.WriteLine(":                                         :");
                Console.WriteLine(": 1\tGet All Films                     :");
                Console.WriteLine(": 2\tGet All Episodes                  :");
                Console.WriteLine(": 3\tLook For Broken Video Links       :");
                Console.WriteLine(": 4\tDelete Broken Video Links         :");
                Console.WriteLine(": 5\tLook For Films Without Episodes   :");
                Console.WriteLine(": 6\tDelete Films Without Episodes     :");
                Console.WriteLine(": 7\tCheck Single Video Link           :");
                Console.WriteLine(": 0\tExit                              :");
                Console.WriteLine("__________________________________________:");
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
                        Console.WriteLine("Please wait, calling database to get all films...");
                        Console.WriteLine("-------------------------------------------------");
                        Console.WriteLine();
                        while (!taskFilms.IsCompleted)
                        {
                            ShowLoader(" Loading Films... ");
                        }
                        Console.WriteLine();
                        Console.WriteLine(taskFilms.Status);
                        var filmList = taskFilms.Result;
                        Console.WriteLine();
                        KFservice.SetConsolColorSuccess();
                        Console.WriteLine(" Data recieved.");
                        Console.WriteLine();
                        KFservice.SetConsolColorHeader();
                        var headerFilm = String.Format("\t{0,-50}{1,-50}{2,-50}", "FilmTitle", "Url Id", "Is Private");
                        Console.WriteLine(headerFilm);
                        foreach (var film in filmList)
                        {
                            var responseBody = String.Format("\t{0,-50}{1,-50}{2,-50}", film.FilmTitle, film.ThumbUrl,
                                film.IsPrivate);
                            KFservice.SetConsolColorBody();
                            Console.WriteLine(responseBody);
                        }
                        Console.ResetColor();
                        Console.WriteLine("\n");
                        Console.WriteLine("Films in total: " + filmList.Count());
                        Console.WriteLine("\n");
                        break;
                    case 2:
                        var taskEpisodes = KFservice.GetAllEpisodesAsync();
                        Console.WriteLine("Please wait, calling database to get all episodes...");
                        Console.WriteLine("----------------------------------------------------");
                        Console.WriteLine();
                        while (!taskEpisodes.IsCompleted)
                        {
                            ShowLoader(" Loading Episodes... ");
                        }
                        Console.WriteLine();
                        Console.WriteLine(taskEpisodes.Status);
                        var episodeList = taskEpisodes.Result;
                        Console.WriteLine();
                        KFservice.SetConsolColorSuccess();
                        Console.WriteLine(" Data recieved.");
                        Console.WriteLine();
                        KFservice.SetConsolColorHeader();
                        var headerEpisode = String.Format("\t{0,-50}{1,-50}{2,-50}", "EpisodeTitle", "Url Id", "Is ActiveLink");
                        Console.WriteLine(headerEpisode);
                        foreach (var episode in episodeList)
                        {
                            var responseBody = String.Format("\t{0,-50}{1,-50}{2,-50}", episode.EpisodeTitle, episode.Url, episode.IsActiveLink);
                            KFservice.SetConsolColorBody();
                            Console.WriteLine(responseBody);
                        }
                        Console.ResetColor();
                        Console.WriteLine("\n");
                        Console.WriteLine("Episodes in total: " + episodeList.Count());
                        Console.WriteLine("\n");
                        break;
                    case 3:
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
                        break;
                    case 4:
                        var taskLinksDeleter = KFservice.DeleteBrokenEpisodeVideoLinksAsync();
                        Console.WriteLine("Please wait, deleting broken video links...");
                        Console.WriteLine("-------------------------------------------");
                        while (!taskLinksDeleter.IsCompleted)
                        {
                            ShowLoader(" Deleting In Progress...  ");
                        }
                        Console.WriteLine("\n");
                        Console.WriteLine(taskLinksDeleter.Status);
                        Console.WriteLine();
                        var deletedEpisodes = taskLinksDeleter.Result;

                        if (deletedEpisodes != null)
                        {
                            KFservice.SetConsolColorBody();
                            Console.WriteLine(deletedEpisodes);
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.WriteLine("No episodes to delete");
                        }
                        Console.WriteLine("\n");
                        break;
                    case 5:
                        var taskFilmsNoEpisodes = KFservice.GetFilmsWithoutEpisodes();
                        Console.WriteLine("Please wait, calling database to get all films without episodes...");
                        Console.WriteLine("------------------------------------------------------------------");
                        Console.WriteLine();
                        while (!taskFilmsNoEpisodes.IsCompleted)
                        {
                            ShowLoader(" Looking For Films... ");
                        }
                        Console.WriteLine();
                        Console.WriteLine(taskFilmsNoEpisodes.Status);
                        var filmListNoEpisodes = taskFilmsNoEpisodes.Result;
                        Console.WriteLine();
                        if (filmListNoEpisodes != null)
                        {
                            KFservice.SetConsolColorSuccess();
                            Console.WriteLine(" Data recieved.");
                            Console.WriteLine();
                            KFservice.SetConsolColorHeader();
                            var headerFilmNoEpisodes = String.Format("\t{0,-50}{1,-50}{2,-50}", "FilmTitle", "Url Id", "Is Private");
                            Console.WriteLine(headerFilmNoEpisodes);
                            foreach (var film in filmListNoEpisodes)
                            {
                                var responseBody = String.Format("\t{0,-50}{1,-50}{2,-50}", film.FilmTitle, film.ThumbUrl,
                                    film.IsPrivate);
                                KFservice.SetConsolColorBody();
                                Console.WriteLine(responseBody);
                            }
                            Console.ResetColor();
                            Console.WriteLine("\n");
                            Console.WriteLine("Found " + filmListNoEpisodes.Count() + " films with no episodes");
                            Console.WriteLine("\n");
                        }
                        else
                        {
                            Console.WriteLine("No films without episodes found!");
                        }
                        Console.WriteLine("\n");
                        break;
                    case 6:
                        var taskEmptyFilmsDeleter = KFservice.DeleteFilmsWithoutEpisodesAsync();
                        Console.WriteLine("Please wait, deleting films without episodes...");
                        Console.WriteLine("----------------------------------------------");
                        while (!taskEmptyFilmsDeleter.IsCompleted)
                        {
                            ShowLoader(" Deleting In Progress...  ");
                        }
                        Console.WriteLine("\n");
                        Console.WriteLine(taskEmptyFilmsDeleter.Status);
                        Console.WriteLine();
                        var deletedFilms = taskEmptyFilmsDeleter.Result;

                        if (deletedFilms != null)
                        {
                            KFservice.SetConsolColorBody();
                            Console.WriteLine(deletedFilms);
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.WriteLine("No Films to delete");
                        }
                        Console.WriteLine("\n");
                        break;
                    case 7:
                        Console.WriteLine("Enter provider youtube/vimeo ?");
                        var provider = Console.ReadLine();
                        Console.WriteLine("VideoId To Check");
                        var input = Console.ReadLine();
                        var output = KFservice.CheckVideoLinkResponseStatus(provider, input);
                        Console.WriteLine();
                        Console.WriteLine(output.ToString());
                        Console.WriteLine("\n");
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

        public static async Task<List<Film>> GetAllFilmsAsync()
        {
            return await context.Film.ToListAsync();
        }

        public static async Task<List<Film>> GetFilmsWithoutEpisodes()
        {
            var emptyFilms = context.Film.Where(f => f.Episode.Count(e => e.IsActiveLink) == 0);
            if (!emptyFilms.Any()) return (null);
            return await emptyFilms.ToListAsync();
        }

        public static async Task<string> DeleteFilmsWithoutEpisodesAsync()
        {
            var deletedFilms = new StringBuilder();
            var filmsToDelete = context.Film.Where(f => f.Episode.Count(e => e.IsActiveLink) == 0);
            if (!filmsToDelete.Any()) return (null);
            deletedFilms.Append(filmsToDelete.Count()).Append(" films deleted:");
            deletedFilms.AppendLine("\n");
            deletedFilms.AppendFormat("\t{0,-35} {1,-35} {2,-35} \t", "Film Title", "Thumb Id", "Provider");
            deletedFilms.AppendLine();
            deletedFilms.AppendLine("\t----------------------------------------------------------------------------------------------------------- \t");
            foreach (var film in filmsToDelete)
            {
                deletedFilms.AppendFormat("\t{0,-35} {1,-35} {2,-35}\t ", film.FilmTitle, film.ThumbUrl, film.Provider.ProviderName);
                deletedFilms.AppendLine();
                context.Film.Remove(film);
            }

            await context.SaveChangesAsync();
            return deletedFilms.ToString();
        }

        public static async Task<List<Episode>> GetAllEpisodesAsync()
        {
            return await context.Episode.ToListAsync();
        }

        //********************Find Broken Links**********************************************************

        public static async Task<string> CheckAllVideoLinksAsync()
        {
            var markedEpisodes = new StringBuilder();
            int counterChecked = 0;
            int counterDead = 0;

            markedEpisodes.Append("  episodes checked for broken links ");
            markedEpisodes.AppendLine();
            markedEpisodes.Append(" episodes marked as broken:           \n");
            markedEpisodes.AppendLine("\n");
            markedEpisodes.AppendFormat("\t{0,-35} {1,-35} {2,-35} {3,-35}\t ", "Episode Id", "Episode Title", "Video Id", "Film Title");
            markedEpisodes.AppendLine();
            markedEpisodes.AppendLine("\t-------------------------------------------------------------------------------------------------------------------------------------            ");

            var episodes = await context.Episode.ToListAsync();
            var total = episodes.Count();
            foreach (var episode in episodes)
            {
                counterChecked++;

                int percent = (100 * (counterChecked + 1)) / total;
                Console.Write("\r{0}{1}% complete", "Scanning in progress... ", percent);
                if (counterChecked == total)
                {
                    Console.WriteLine("\nDone :-)");
                }

                var videoExist = CheckVideoLinkResponseStatus(episode.Film.Provider.ProviderName, episode.Url);

                if (videoExist.HasValue && (bool)!videoExist)
                {
                    counterDead++;
                    await MarkLinkAsBrokenAsync(episode.EpisodeId);
                    markedEpisodes.AppendFormat("\t{0,-35} {1,-35} {2,-35} {3,-35}\t ", episode.EpisodeId, episode.EpisodeTitle, episode.Url, episode.Film.FilmTitle);
                    markedEpisodes.AppendLine();
                }
            }

            if (counterDead == 0) return (null);
            markedEpisodes.Insert(0, counterChecked);
            // ReSharper disable once StringIndexOfIsCultureSpecific.1
            var pos = markedEpisodes.ToString().IndexOf("links") + 6 + Environment.NewLine.Length;
            markedEpisodes.Insert(pos, counterDead);
            return markedEpisodes.ToString();
        }

        public static bool? CheckVideoLinkResponseStatus(string provider, string videoId)
        {
            string url = string.Empty;
            var exists = false;

            switch (provider)
            {
                case "youtube":
                    //url = String.Format("http://gdata.youtube.com/feeds/api/videos/{0}", videoId); => Cease to work in 2015
                    url = String.Format("http://img.youtube.com/vi/{0}/0.jpg", videoId);
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
        }
        //************************************************************************************

        //************************Delete Broken Links******************************************
        public static async Task<string> DeleteBrokenEpisodeVideoLinksAsync()
        {
            var deletedEpisodes = new StringBuilder();
            var episodesToDelete = context.Episode.Where(e => e.IsActiveLink == false);
            if (!episodesToDelete.Any()) return (null);
            deletedEpisodes.Append(episodesToDelete.Count()).Append(" episodes deleted:");
            deletedEpisodes.AppendLine("\n");
            deletedEpisodes.AppendFormat("\t{0,-35} {1,-35} {2,-35}\t", "Episode Title", "Video Id", "Film Title");
            deletedEpisodes.AppendLine();
            deletedEpisodes.AppendLine("\t-----------------------------------------------------------------------------------------------------------\t");
            foreach (var episode in episodesToDelete)
            {
                deletedEpisodes.AppendFormat("\t{0,-35} {1,-35} {2,-35}\t ", episode.EpisodeTitle, episode.Url, episode.Film.FilmTitle);
                deletedEpisodes.AppendLine();
                context.Episode.Remove(episode);
            }
            await context.SaveChangesAsync();
            return deletedEpisodes.ToString();
        }
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
