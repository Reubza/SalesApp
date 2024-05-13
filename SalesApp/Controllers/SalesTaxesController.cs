using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace SalesTaxesAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SalesTaxesController : ControllerBase
    {
        [HttpPost]
        public ActionResult<Receipt> GenerateReceipt([FromBody] List<Item> items)
        {
            if (items == null || !items.Any())
                return BadRequest("No items provided.");

            var receipt = new Receipt();
            decimal totalSalesTax = 0;
            decimal totalAmount = 0;

            foreach (var item in items)
            {
                var tax = CalculateTax(item);
                totalSalesTax += tax;
                var totalPrice = item.ItemQuantity * (item.Price + tax);
                totalAmount += totalPrice;

                receipt.Items.Add(new ReceiptItem
                {
                    ItemQuantity = item.ItemQuantity,
                    Name = item.Name,
                    Price = totalPrice
                });
            }

            receipt.TotalSalesTaxes = Math.Ceiling(totalSalesTax * 20m) / 20m; // Rounding up to the nearest 0.05
            receipt.TotalAmount = totalAmount;

            return Ok(receipt);
        }

        private decimal CalculateTax(Item item)
        {
            decimal basicTaxRate = 0.1m;
            decimal importTaxRate = 0.05m;

            // Check if the item is exempt from basic tax and Calculate basic tax
            decimal basicTax = item.IsExempt ? 0 : Math.Ceiling(item.Price * basicTaxRate * 20m) / 20m;

            //Check if the item is imported and Calculate import tax
            decimal importTax = item.IsImported ? Math.Ceiling(item.Price * importTaxRate * 20m) / 20m  : 0;

            return item.ItemQuantity * (basicTax + importTax); 
        }
    }

    public class Item
    {
        public int ItemQuantity { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public bool IsExempt { get; set; }
        public bool IsImported { get; set; }
    }

    public class Receipt
    {
        public List<ReceiptItem> Items { get; set; } = new List<ReceiptItem>();
        public decimal TotalSalesTaxes { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class ReceiptItem
    {
        public int ItemQuantity { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
