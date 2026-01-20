using MyShop.Shared.DTOs.Reports;
using MyShop.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Shared.DTOs.Products
{
    public record ProductComparisonDTO 
    {
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public ReportTimeInterval interval { get; set; }
        public List<string> ProductNames { get; set; } = new List<string>();
    }
}
