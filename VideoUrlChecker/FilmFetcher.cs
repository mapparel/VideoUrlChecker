using System;
using System.Linq;
using Quartz;

namespace VideoUrlChecker
{
    public class FilmFetcher : IJob
    {

        public void Execute(IJobExecutionContext context)
        {
            var taskFilms = KFservice.GetAllFilmsAsync();

            KFservice.SetConsolColorHeader();
            var headerFilm = String.Format("\t{0,-50}{1,-50}{2,-50}", "FilmTitle", "Url Id", "Is Private");
            Console.WriteLine(headerFilm);

            var filmList = taskFilms.Result;
            foreach (var responseBody in filmList.Select(film => String.Format("\t{0,-50}{1,-50}{2,-50}", film.FilmTitle, film.ThumbUrl,film.IsPrivate)))
            {
                KFservice.SetConsolColorBody();
                Console.WriteLine(responseBody);
            }
            Console.ResetColor();
            Console.WriteLine("\n");
            Console.WriteLine("Films in total: " + filmList.Count());
            Console.WriteLine("\n");

        }
    }
}
