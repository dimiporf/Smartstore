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
using Newtonsoft.Json;
using Smartstore.Core.Catalog.Categories;
using static Smartstore.Core.Security.Permissions.Catalog;

namespace Dimitris.ProductImport.Controllers
{
    public class AddProductAdminController : AdminController
    {
        private readonly SmartDbContext _smartDbContext;
        private List<AddProductModel> _newProducts;

        

        public AddProductAdminController(SmartDbContext smartDbContext)
        {
            _smartDbContext = smartDbContext;
            _newProducts = new List<AddProductModel>();
        }

        // GET: Display the index view
        [HttpGet]
        public IActionResult Index(string newProducts)
        {
            if (!string.IsNullOrEmpty(newProducts))
            {
                _newProducts = JsonConvert.DeserializeObject<List<AddProductModel>>(newProducts);
            }
            else
            {
                _newProducts = new List<AddProductModel>();
            }

            return View(_newProducts);
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

                    // Check for existing SKUs in the database
                    var existingSkus = _smartDbContext.Products.Select(p => p.Sku).ToList();

                    // Filter out products with existing SKUs
                    _newProducts = addProducts.Where(p => !existingSkus.Contains(p.Sku)).ToList();

                    TempData["SuccessMessage"] = "Data imported successfully.";

                    // Add new products to the database
                    AddNewProductsToDatabase(_newProducts);

                    TempData["SuccessMessage"] = "Data imported successfully and new products added to the database.";

                    // Pass _newProducts as a query parameter
                    return RedirectToAction("Index", new { newProducts = JsonConvert.SerializeObject(_newProducts) });

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

            return RedirectToAction("Index");
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
                    Sku = productElement.Element("ProductID")?.Value ?? productElement.Element("UniqueID")?.Value,
                    Name = productElement.Element("Name")?.Value ?? productElement.Element("ProductName")?.Value,
                    ShortDescription = productElement.Element("Description")?.Value,
                    StockQuantity = int.Parse(productElement.Element("Stock")?.Value),
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
                            Sku = product["ProductID"]?.ToString() ?? product["UniqueID"]?.ToString(),
                            Name = product["Name"]?.ToString() ?? product["ProductName"]?.ToString(), 
                            ShortDescription = product["Description"]?.ToString(),
                            StockQuantity = (int)product["Stock"],
                            Price = ParsePrice(product["Price"]?.ToString()),
                            CategoryId = product["CategoryId"]?.ToString() ?? product["Category"]?.ToString() 
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

            using (var csv = new CachedCsvReader(new StreamReader(stream), true))
            {
                var headers = csv.GetFieldHeaders();

                while (csv.ReadNextRecord())
                {
                    var product = new AddProductModel();
                    for (int i = 0; i < headers.Length; i++)
                    {
                        switch (headers[i])
                        {
                            case "ProductID":
                            case "Product ID":
                            case "SKU":
                                product.Sku = csv[i];
                                break;
                            case "Name":
                            case "ProductName":
                                product.Name = csv[i];
                                break;
                            case "Price":
                                product.Price = ParsePriceCSV(csv[i]);
                                break;
                            case "Stock":
                                product.StockQuantity = int.Parse(csv[i]);
                                break;
                            case "Description":
                                product.ShortDescription = csv[i];
                                break;
                            case "CategoryId":
                            case "Category":
                                product.CategoryId = csv[i];
                                break;
                            case "hasDownload":
                                bool.TryParse(csv[i], out bool hasDownload);
                                product.IsDownload = hasDownload;
                                break;
                            case "PublishedOn":
                                DateOnly.TryParse(csv[i], out DateOnly publishedOn);
                                product.PublishedOn = publishedOn;
                                break;
                                // Add additional cases for other columns if needed
                        }
                    }
                    addProducts.Add(product);
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


        // Add the rest of the products to the database
        private async void AddNewProductsToDatabase(List<AddProductModel> newProducts)
        {
            foreach (var product in newProducts)
            {
                var newProductEntity = new Smartstore.Core.Catalog.Products.Product
                {
                    Sku = product.Sku,
                    Name = product.Name,
                    ShortDescription = product.ShortDescription,
                    Price = product.Price,
                    StockQuantity = product.StockQuantity,
                    Published = true,
                    TaxCategoryId = 1,
                    
                    // Add other properties as needed
                };
                
               
                //    var categoryFromDb = _smartDbContext.Categories.Where(x => x.Id == product.CategoryId).FirstOrDefault();
                
                //newProductEntity.ProductCategories.Add(new ProductCategory { Category = product.CategoryId });
                _smartDbContext.Products.Add(newProductEntity);
            }

            _smartDbContext.SaveChanges();
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
