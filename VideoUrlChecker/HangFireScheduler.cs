using System;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Owin;
using Owin;

namespace VideoUrlChecker
{
    class HangFireScheduler
    {
        public static void ConfigureHangfire(IAppBuilder app)
        {
            app.UseHangfire(config =>
            {
                config.UseSqlServerStorage("DefaultConnection");
                config.UseServer();
            });
        }

        public static void InitializeJobs()
        {
            RecurringJob.AddOrUpdate("first-job", () => Console.WriteLine("Hell Yeah! It's running smooooth !!!"), Cron.Minutely);
        }
    }
}
