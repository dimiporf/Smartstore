using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyOrg.HelloWorld.Models;
using System.Xml.Linq;
using Smartstore.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Smartstore.Core.Data;

namespace MyOrg.HelloWorld.Controllers
{
    public class AddProductAdminController: AdminController
    {
       private readonly SmartDbContext _smartDbContext;

        public AddProductAdminController(SmartDbContext smartDbContext)
        {
            _smartDbContext = smartDbContext;
        }

        [HttpGet]
        public IActionResult Index() {  return View(); }

        [HttpPost]
        public IActionResult ImportXml(IFormFile xmlFile)
        {
            var addProducts = new List<AddProductModel>();

            try
            {
                // Check if a file is uploaded
                if (xmlFile != null && xmlFile.Length > 0)
                {
                    using (var stream = new MemoryStream())
                    {
                        // Copy the uploaded file content to a memory stream
                        xmlFile.CopyTo(stream);
                        stream.Position = 0; // Reset the position to the beginning of the stream

                        // Load the XML document from the memory stream
                        var xmlDoc = XDocument.Load(stream);

                        addProducts = xmlDoc.Descendants("Product").Select(item => new AddProductModel
                        {
                            ProductID = item.Element("ProductID")?.Value,
                            Name = item.Element("Name")?.Value,
                            Description = item.Element("Description")?.Value,
                            Price = decimal.Parse(item.Element("Price")?.Value),
                            Stock = int.Parse(item.Element("Stock")?.Value)
                        }).ToList();
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "No file uploaded.";
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                TempData["ErrorMessage"] = "An error occurred while importing XML: " + ex.Message;
            }

            TempData["SuccessMessage"] = "XML imported successfully.";
            return View("Index", addProducts);
        }
    }
}
