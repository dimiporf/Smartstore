using Smartstore.Collections;
using Smartstore.Core.Content.Menus;
using Smartstore.Web.Rendering.Builders;

namespace Dimitris.ProductImport
{
    public class AdminMenu : AdminMenuProvider
    {
        protected override void BuildMenuCore(TreeNode<MenuItem> modulesNode)
        {
            var myMenuItem = new MenuItem().ToBuilder()
                .ResKey("Plugins.Dimitris.ProductImport.MyMenuItem")
                .Icon("gear", "bi")
                .Action("Configure", "HelloWorldAdmin", new { area = "Admin" })
                .AsItem();

            var menuNode = new TreeNode<MenuItem>(myMenuItem);
            var refNode = modulesNode.Root.SelectNodeById("themes");
            menuNode.InsertBefore(refNode);
           

           
            var trainingMenuItem = new MenuItem().ToBuilder()
                .Text("Training")
                .ResKey("Plugins.Dimitris.ProductImport.ProductImportMenuItem")
                .Icon("star", "bi") // Use an appropriate icon
                .Action("Index", "AddProductAdmin", new { area = "Admin" }) // Specify the area
                .AsItem();

            modulesNode.Append(trainingMenuItem);
        }
    }
}
