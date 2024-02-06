using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyOrg.HelloWorld.Models;
using MyOrg.HelloWorld.Settings;
using Smartstore.ComponentModel;
using Smartstore.Core;
using Smartstore.Core.Data;
using Smartstore.Web.Controllers;
using Smartstore.Web.Modelling.Settings;
using System.Collections.Generic;
using System.Linq;


namespace MyOrg.Controllers
{
    public class TrainingAdminController : AdminController
    {
        private readonly SmartDbContext _db;

        public TrainingAdminController(SmartDbContext db)
        {
            _db = db;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("api/autoComplete")]
        public async Task<IActionResult> GetAutoCompleteSuggestions(string input)
        {
            // Query the database for entries matching the input value in Name and FullDescription
            var suggestions = await _db.Products
                .Where(p =>
                    p.Name.Contains(input) ||
                    p.FullDescription.Contains(input))
                .Select(p => p.Name)
                .Distinct()
                .ToListAsync();

            return Json(suggestions);
        }

    }
}
