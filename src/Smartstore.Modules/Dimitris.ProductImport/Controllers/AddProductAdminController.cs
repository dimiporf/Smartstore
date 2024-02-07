using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Dimitris.ProductImport.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using Smartstore.Core.Data;
using Smartstore.Web.Controllers;
using LumenWorks.Framework.IO.Csv;
using Smartstore.Core.DataExchange.Csv;
using System.Text.RegularExpressions;

namespace Dimitris.ProductImport.Controllers
{
    public class AddProductAdminController : AdminController
    {
        private readonly SmartDbContext _smartDbContext;

        public AddProductAdminController(SmartDbContext smartDbContext)
        {
            _smartDbContext = smartDbContext;
        }

        // GET: Display the index view
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // POST: Import product data from a file
        [HttpPost]
        public IActionResult ImportData(IFormFile file)
        {
            var addProducts = new List<AddProductModel>();

            try
            {
                if (file != null && file.Length > 0)
                {
                    using (var stream = new MemoryStream())
                    {
                        file.CopyTo(stream);
                        stream.Position = 0;

                        // Determine the file format based on its content type
                        if (file.ContentType == "application/xml" || file.ContentType == "text/xml")
                        {
                            addProducts = ParseXml(stream);
                        }
                        else if (file.ContentType == "application/json" || file.ContentType == "text/json")
                        {
                            addProducts = ParseJson(stream);
                        }
                        else
                        {
                            addProducts = ParseCsv(stream);
                        }
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "No file uploaded.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while importing data: " + ex.Message;
            }

            TempData["SuccessMessage"] = "Data imported successfully.";
            return View("Index", addProducts);
        }

        // Parse XML data from the stream
        private List<AddProductModel> ParseXml(Stream stream)
        {
            var addProducts = new List<AddProductModel>();

            var xmlDoc = XDocument.Load(stream);

            // Iterate through each Product element in the XML document
            foreach (var productElement in xmlDoc.Descendants("Product"))
            {
                var product = new AddProductModel
                {
                    ProductID = productElement.Element("ProductID")?.Value ?? productElement.Element("UniqueID")?.Value,
                    Name = productElement.Element("Name")?.Value ?? productElement.Element("ProductName")?.Value,
                    Description = productElement.Element("Description")?.Value,
                    Stock = int.Parse(productElement.Element("Stock")?.Value),
                    Price = ParsePrice(productElement.Element("Price")?.Value),
                    CategoryId = productElement.Element("CategoryId")?.Value ?? productElement.Element("Category")?.Value
                };

                addProducts.Add(product);
            }

            return addProducts;
        }

        // Parse JSON data from the stream
        private List<AddProductModel> ParseJson(Stream stream)
        {
            var addProducts = new List<AddProductModel>();

            using (var reader = new StreamReader(stream))
            {
                var jsonString = reader.ReadToEnd();
                var jsonObject = JObject.Parse(jsonString);

                if (jsonObject["Products"] != null)
                {
                    var productsArray = jsonObject["Products"];

                    foreach (var product in productsArray)
                    {
                        var parsedProduct = new AddProductModel
                        {
                            ProductID = product["ProductID"]?.ToString() ?? product["UniqueID"]?.ToString(),
                            Name = product["Name"]?.ToString() ?? product["ProductName"]?.ToString(), 
                            Description = product["Description"]?.ToString(),
                            Stock = (int)product["Stock"],
                            Price = ParsePrice(product["Price"]?.ToString()),
                            CategoryId = product["CategoryId"]?.ToString() ?? product["Category"]?.ToString(),
                        };

                        addProducts.Add(parsedProduct);
                    }
                }
            }

            return addProducts;
        }

        // Parse CSV data from the stream
        private List<AddProductModel> ParseCsv(Stream stream)
        {
            var addProducts = new List<AddProductModel>();

            using (var reader = new StreamReader(stream))
            {
                string line;
                // Read the first line to determine the format
                var headers = reader.ReadLine().Split(',');

                while ((line = reader.ReadLine()) != null)
                {
                    // Split the line into individual values
                    var parts = line.Split(',');

                    // Check the number of columns to determine the format
                    if (parts.Length == headers.Length)
                    {
                        var product = new AddProductModel();
                        for (int i = 0; i < headers.Length; i++)
                        {
                            switch (headers[i])
                            {
                                case "ProductID":
                                case "Product ID":
                                    product.ProductID = parts[i];
                                    break;
                                case "Name":
                                case "ProductName":
                                    product.Name = parts[i];
                                    break;
                                case "Price":
                                    product.Price = ParsePriceCSV(parts[i]);
                                    break;
                                case "Stock":
                                    product.Stock = int.Parse(parts[i]);
                                    break;
                                case "Description":
                                    product.Description = parts[i];
                                    break;
                                case "CategoryId":
                                case "Category":
                                    product.CategoryId = parts[i];
                                    break;
                                case "hasDownload":
                                    bool.TryParse(parts[i], out bool hasDownload);
                                    product.HasDownload = hasDownload;
                                    break;
                                case "PublishedOn":
                                    DateOnly.TryParse(parts[i], out DateOnly publishedOn);
                                    product.PublishedOn = publishedOn;
                                    break;
                                    // Add additional cases for other columns if needed
                            }
                        }
                        addProducts.Add(product);
                    }
                }
            }

            return addProducts;
        }

        // Parse the price field, handling different formats
        private decimal ParsePriceCSV(string price)
        {
            // Remove any non-numeric characters except for the decimal separator
            var cleanedPrice = Regex.Replace(price, @"[^0-9\.,]+", "");
            // Replace comma with dot as decimal separator
            cleanedPrice = cleanedPrice.Replace(",", ".");
            return decimal.Parse(cleanedPrice, NumberStyles.Currency, CultureInfo.InvariantCulture);
        }


        // Parse the price string to a decimal value
        private decimal ParsePrice(string priceString)
        {
            decimal price = 0;

            if (!string.IsNullOrEmpty(priceString))
            {
                priceString = priceString.Replace("$", "").Replace("€", "").Replace("£", "").Replace("USD", "").Trim();
                decimal.TryParse(priceString, NumberStyles.Currency, CultureInfo.InvariantCulture, out price);
            }

            return price;
        }

        // GET: Retrieve product details by ID
        [HttpGet]
        public IActionResult GetProduct(int productId)
        {
            try
            {
                var product = _smartDbContext.Products.FirstOrDefault(p => p.Id == productId);

                if (product != null)
                {
                    return Json(product);
                }
                else
                {
                    return NotFound($"Product with ID '{productId}' not found.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving the product: {ex.Message}");
            }
        }
    }
}
