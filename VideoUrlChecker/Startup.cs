using System;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(VideoUrlChecker.Startup))]

namespace VideoUrlChecker
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888

            HangFireScheduler.ConfigureHangfire(app);
            HangFireScheduler.InitializeJobs();
            
        }
    }
}
