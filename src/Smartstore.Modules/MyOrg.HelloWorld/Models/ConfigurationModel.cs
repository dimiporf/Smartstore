using Smartstore.Web.Modelling;

namespace MyOrg.HelloWorld.Models
{
    [LocalizedDisplay("Plugins.MyOrg.HelloWorld.")]
    public class ConfigurationModel : ModelBase
    {
        [LocalizedDisplay("*Name")]
        public string Name { get; set; }

        [LocalizedDisplay("*NumberOfDaysToDisplayNotification")]
        public int NumberOfDaysToDisplayNotification { get; set; } = 3;

        [LocalizedDisplay("*NumberOfDaysToKeepNotification")]
        public int NumberOfDaysToKeepNotification { get; set; } = 7;
    }
}