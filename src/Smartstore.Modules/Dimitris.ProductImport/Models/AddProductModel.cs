using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimitris.ProductImport.Models
{
    public class AddProductModel
    {
        public string Sku { get; set; }
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string CategoryId { get; set; }

        public string Currency {  get; set; }
        public bool IsDownload { get; set; }
        
        public DateOnly PublishedOn { get; set; }
    }
}
