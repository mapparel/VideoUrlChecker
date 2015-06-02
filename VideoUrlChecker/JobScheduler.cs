using Quartz;
using Quartz.Impl;

namespace VideoUrlChecker
{
    public class JobScheduler
    {
        public static void Start()
        {
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();

            IJobDetail job = JobBuilder.Create<LinksChecker>().Build();

            ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity("trigger1", "group1")
            .StartNow()
            .WithSimpleSchedule(x => x
            .WithIntervalInMinutes(3)
            //.WithIntervalInSeconds(10)
            .RepeatForever())
            .Build();

            scheduler.ScheduleJob(job, trigger);
        }
    }
}
