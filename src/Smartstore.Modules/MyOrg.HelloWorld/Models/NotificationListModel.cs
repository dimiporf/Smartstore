using Smartstore.Web.Modelling;

namespace MyOrg.HelloWorld.Models
{
    [LocalizedDisplay("Plugins.MyOrg.HelloWorld.Notification.Grid.")]
    public class NotificationListModel : EntityModelBase
    {
        [LocalizedDisplay("*Search.Message")]
        public string SearchMessage { get; set; }
    }
}