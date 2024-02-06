using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyOrg.HelloWorld.Extensions;
using MyOrg.HelloWorld.Settings;
using MyOrg.HelloWorld.Extensions;
using Smartstore.Core.Data;
using Smartstore.Scheduling;

namespace MyOrg.HelloWorld.Tasks
{
    internal class CleanupHelloWorldTask : ITask
    {
        private readonly HelloWorldSettings _settings;
        private readonly SmartDbContext _db;

        public CleanupHelloWorldTask(HelloWorldSettings settings, SmartDbContext db)
        {
            _settings = settings;
            _db = db;
        }

        public async Task Run(TaskExecutionContext ctx, CancellationToken cancelToken = default)
        {
            var date = DateTime.UtcNow.AddDays(-_settings.NumberOfDaysToKeepNotification);

            await _db.Notifications()
                .Where(x => x.Published < date)
                .ExecuteDeleteAsync();

            await _db.SaveChangesAsync();
        }
    }
}