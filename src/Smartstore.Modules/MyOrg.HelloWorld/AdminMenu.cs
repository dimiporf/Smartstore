using Smartstore.Collections;
using Smartstore.Core.Content.Menus;
using Smartstore.Web.Rendering.Builders;

namespace MyOrg.HelloWorld
{
    public class AdminMenu : AdminMenuProvider
    {
        protected override void BuildMenuCore(TreeNode<MenuItem> modulesNode)
        {
            var myMenuItem = new MenuItem().ToBuilder()
                .ResKey("Plugins.MyOrg.HelloWorld.MyMenuItem")
                .Icon("gear", "bi")
                .Action("Configure", "HelloWorldAdmin", new { area = "Admin" })
                .AsItem();

            var menuNode = new TreeNode<MenuItem>(myMenuItem);
            var refNode = modulesNode.Root.SelectNodeById("themes");
            menuNode.InsertBefore(refNode);

            var secondMenuItem = new MenuItem().ToBuilder()
                .ResKey("Plugins.MyOrg.HelloWorld.MySecondMenuItem")
                .AsItem();
            var subMenuItem = new MenuItem().ToBuilder()
                .ResKey("Plugins.MyOrg.HelloWorld.MySubMenuItem")
                .Action("Index", "AddProductAdmin", new { area = "Admin" })
                .AsItem();

            var secondMenuNode = new TreeNode<MenuItem>(secondMenuItem);
            var subMenuNode = new TreeNode<MenuItem>(subMenuItem);

            secondMenuNode.InsertAfter(menuNode);
            secondMenuNode.Append(subMenuNode);

            // All notifications
            modulesNode.Append(new MenuItem().ToBuilder()
                .ResKey("Plugins.MyOrg.HelloWorld.Grid.Notification.Title")
                .Text("This is not working yet")
                .Icon("chat-left-text", "bi")
                .Id("hello-world-notifications")
                .Action("List", "NotificationAdmin", new { area = "Admin" })
                .AsItem());

            var trainingMenuItem = new MenuItem().ToBuilder()
                .Text("Training")
                .ResKey("Plugins.MyOrg.HelloWorld.TrainingMenuItem")
                .Icon("star", "bi") // Use an appropriate icon
                .Action("Index", "TrainingAdmin", new { area = "Admin" }) // Specify the area
                .AsItem();

            modulesNode.Append(trainingMenuItem);
        }
    }
}
