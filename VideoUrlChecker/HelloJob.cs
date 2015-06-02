using System;
using Quartz;

namespace VideoUrlChecker
{
    class HelloJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Console.WriteLine("Hello from Job - Working Hard");
        }
    }
}
