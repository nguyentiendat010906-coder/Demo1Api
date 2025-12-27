using Demo1Api.Data;
using Demo1Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Demo1Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoicesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private const decimal VAT_RATE = 0.10m; // 10% VAT

        public InvoicesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/invoices
        [HttpGet]
        public IActionResult GetAll()
        {
            var invoices = _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Table)
                .Select(i => new
                {
                    id = i.Id,
                    invoiceDate = i.InvoiceDate,
                    status = i.Status,
                    subtotal = i.Subtotal,
                    vatAmount = i.VatAmount,
                    totalAmount = i.TotalAmount,
                    tableId = i.TableId,
                    tableName = i.Table != null ? i.Table.Name : null,
                    customerName = i.Customer != null ? i.Customer.Name : "Khách lẻ"
                })
                .OrderByDescending(i => i.invoiceDate)
                .ToList();

            return Ok(invoices);
        }

        [HttpGet("{id}")]
        public IActionResult GetDetail(int id)
        {
            var invoice = _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Table)
                    .ThenInclude(t => t.TableGroup)
                .Include(i => i.InvoiceDetails)
                .Where(i => i.Id == id)
                .Select(i => new
                {
                    i.Id,
                    i.InvoiceDate,
                    i.EndTime,
                    i.Status,
                    i.Subtotal,
                    i.VatAmount,
                    i.TotalAmount,
                    TableName = i.Table != null ? i.Table.Name : null,
                    GroupName = i.Table != null && i.Table.TableGroup != null ? i.Table.TableGroup.Name : null,
                    CashierName = i.CashierName,
                    Customer = i.Customer == null ? null : new
                    {
                        i.Customer.Id,
                        i.Customer.Name,
                        i.Customer.Phone,
                        i.Customer.Address
                    },
                    InvoiceDetails = i.InvoiceDetails.Select(d => new
                    {
                        d.Id,
                        d.ProductName,
                        d.Quantity,
                        d.UnitPrice
                    }).ToList()
                })
                .FirstOrDefault();

            if (invoice == null) return NotFound();
            return Ok(invoice);
        }

        [HttpGet("by-table/{tableId}")]
        public IActionResult GetByTable(int tableId)
        {
            var invoice = _context.Invoices
                .FirstOrDefault(i => i.TableId == tableId && i.Status == "open");

            if (invoice == null)
                return NotFound("No open invoice found for this table");

            return Ok(invoice);
        }

        [HttpPost("create-for-table/{tableId}")]
        public IActionResult CreateForTable(int tableId)
        {
            var table = _context.Tables.FirstOrDefault(t => t.Id == tableId);
            if (table == null)
                return NotFound("Table not found");

            var existingInvoice = _context.Invoices
                .FirstOrDefault(i => i.TableId == tableId && i.Status == "open");

            if (existingInvoice != null)
                return Ok(existingInvoice);

            var invoice = new Invoice
            {
                InvoiceDate = DateTime.Now,
                Status = "open",
                Subtotal = 0,
                VatAmount = 0,
                TotalAmount = 0,
                TableId = tableId
            };

            table.Status = "serving";

            _context.Invoices.Add(invoice);
            _context.SaveChanges();

            return Ok(invoice);
        }

        [HttpGet("{invoiceId}/items")]
        public IActionResult GetInvoiceItems(int invoiceId)
        {
            var items = _context.InvoiceDetails
                .Where(d => d.InvoiceId == invoiceId)
                .Select(d => new
                {
                    id = d.Id,
                    invoiceId = d.InvoiceId,
                    productId = d.ProductId,
                    productName = d.ProductName,
                    quantity = d.Quantity,
                    unitPrice = d.UnitPrice,
                    total = d.Quantity * d.UnitPrice
                })
                .ToList();

            return Ok(items);
        }

        [HttpPost("{invoiceId}/items")]
        public IActionResult AddInvoiceItem(int invoiceId, [FromBody] InvoiceItemDto itemDto)
        {
            var invoice = _context.Invoices.Find(invoiceId);
            if (invoice == null)
                return NotFound("Invoice not found");

            var product = _context.Products.Find(itemDto.ProductId);
            if (product == null)
                return NotFound("Product not found");

            var existingItem = _context.InvoiceDetails
                .FirstOrDefault(d => d.InvoiceId == invoiceId && d.ProductId == itemDto.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += itemDto.Quantity;
                existingItem.UnitPrice = itemDto.UnitPrice;
            }
            else
            {
                var item = new InvoiceDetail
                {
                    InvoiceId = invoiceId,
                    ProductId = itemDto.ProductId,
                    ProductName = product.Name,
                    Quantity = itemDto.Quantity,
                    UnitPrice = itemDto.UnitPrice
                };
                _context.InvoiceDetails.Add(item);
            }

            _context.SaveChanges();
            UpdateInvoiceTotal(invoiceId);
            _context.SaveChanges();

            return Ok();
        }

        [HttpPut("{invoiceId}/items/{itemId}")]
        public IActionResult UpdateInvoiceItem(int invoiceId, int itemId, [FromBody] InvoiceItemDto itemDto)
        {
            var item = _context.InvoiceDetails
                .FirstOrDefault(d => d.Id == itemId && d.InvoiceId == invoiceId);

            if (item == null)
                return NotFound("Item not found");

            item.Quantity = itemDto.Quantity;
            item.UnitPrice = itemDto.UnitPrice;

            _context.SaveChanges();
            UpdateInvoiceTotal(invoiceId);
            _context.SaveChanges();

            return Ok();
        }

        [HttpDelete("{invoiceId}/items/{itemId}")]
        public IActionResult DeleteInvoiceItem(int invoiceId, int itemId)
        {
            var item = _context.InvoiceDetails
                .FirstOrDefault(d => d.Id == itemId && d.InvoiceId == invoiceId);

            if (item == null)
                return NotFound("Item not found");

            _context.InvoiceDetails.Remove(item);

            _context.SaveChanges();
            UpdateInvoiceTotal(invoiceId);
            _context.SaveChanges();

            return NoContent();
        }

        // ✅ FIX: LƯU ENDTIME KHI CHECKOUT
        [HttpPut("{id}/checkout")]
        public IActionResult Checkout(int id)
        {
            var invoice = _context.Invoices
                .Include(i => i.Table)
                .FirstOrDefault(i => i.Id == id);

            if (invoice == null)
                return NotFound();

            // Tính lại tổng tiền trước khi thanh toán
            UpdateInvoiceTotal(id);

            // ✅ LƯU THỜI GIAN KẾT THÚC
            invoice.EndTime = DateTime.Now;
            invoice.Status = "paid";

            if (invoice.Table != null)
                invoice.Table.Status = "empty";

            _context.SaveChanges();

            return Ok();
        }

        private void UpdateInvoiceTotal(int invoiceId)
        {
            var invoice = _context.Invoices.Find(invoiceId);
            if (invoice == null) return;

            var subtotal = _context.InvoiceDetails
                .Where(d => d.InvoiceId == invoiceId)
                .Sum(d => d.Quantity * d.UnitPrice);

            var vatAmount = subtotal * VAT_RATE;
            var totalAmount = subtotal + vatAmount;

            invoice.Subtotal = subtotal;
            invoice.VatAmount = vatAmount;
            invoice.TotalAmount = totalAmount;

            Console.WriteLine($"✅ Invoice {invoiceId}: Subtotal={subtotal}, VAT={vatAmount}, Total={totalAmount}");
        }
    }

    public class InvoiceItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}