using Smartstore.Web.Modelling;

namespace MyOrg.HelloWorld.Models
{
    // This attribute must be added for proper model binding.
    // We've implemented it for security reasons. 
    // Explaining this is beyond the scope of this tutorial.
    [CustomModelPart]
    public class AdminEditTabModel : ModelBase
    {
        public int EntityId { get; set; }

        [LocalizedDisplay("Plugins.MyOrg.HelloWorld.MyTabValue")]
        public string MyTabValue { get; set; }
    }
}
