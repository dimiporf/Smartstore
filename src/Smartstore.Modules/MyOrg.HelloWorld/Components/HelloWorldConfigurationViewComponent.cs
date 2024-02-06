using Microsoft.AspNetCore.Mvc;
using MyOrg.HelloWorld.Models;
using Smartstore.Web.Components;

namespace MyOrg.HelloWorld.Components
{
    public class HelloWorldConfigurationViewComponent : SmartViewComponent
    {
        public IViewComponentResult Invoke(object data)
        {
            var model = data as ProfileConfigurationModel;
            return View(model);
        }
    }
}