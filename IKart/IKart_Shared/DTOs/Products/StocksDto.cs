using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKart_Shared.DTOs
{
    public class StocksDto
    {
        public int StockId { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public int? TotalStocks { get; set; }
        public int? AvailableStocks { get; set; }
    }
}
