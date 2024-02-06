using Microsoft.EntityFrameworkCore;
using Smartstore.Core.Data;

namespace MyOrg.HelloWorld.Extensions
{
    public static class SmartDbContextExtensions
    {
        public static DbSet<Notification> Notifications(this SmartDbContext db)
            => db.Set<Notification>();
    }
}
