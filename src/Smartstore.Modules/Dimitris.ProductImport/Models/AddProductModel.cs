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
        public string ProductID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal? Price { get; set; }
        public int? Stock { get; set; }
        public string CategoryId { get; set; }

        public string Currency {  get; set; }
        public bool HasDownload { get; set; }
        
        public DateOnly PublishedOn { get; set; }
    }
}
