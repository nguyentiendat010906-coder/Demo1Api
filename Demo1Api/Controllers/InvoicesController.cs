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
        private const decimal VAT_RATE = 0.10m;

        public InvoicesController(AppDbContext context) => _context = context;

        [HttpGet]
        public IActionResult GetAll()
        {
            var invoices = _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Table)
                .OrderByDescending(i => i.InvoiceDate)
                .Select(i => new
                {
                    id = i.Id,
                    invoiceDate = i.InvoiceDate,
                    status = i.Status,
                    subtotal = i.Subtotal,
                    vatAmount = i.VatAmount,
                    totalAmount = i.TotalAmount,
                    tableId = i.TableId,
                    tableName = i.Table!.Name,
                    customerName = i.CustomerName ?? i.Customer!.Name ?? "Khách lẻ"
                })
                .ToList();

            return Ok(invoices);
        }

        [HttpGet("{id}")]
        public IActionResult GetDetail(int id)
        {
            var invoice = _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Table).ThenInclude(t => t!.TableGroup)
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
                    TableName = i.Table!.Name,
                    GroupName = i.Table.TableGroup!.Name,
                    i.CashierName,
                    i.CustomerName,
                    i.CustomerPhone,
                    i.CustomerTaxCode,
                    i.CustomerIdCard,
                    i.CustomerEmail,
                    i.CustomerAddress,
                    InvoiceDetails = i.InvoiceDetails.Select(d => new
                    {
                        d.Id,
                        d.ProductName,
                        d.Quantity,
                        d.UnitPrice
                    }).ToList()
                })
                .FirstOrDefault();

            return invoice == null ? NotFound() : Ok(invoice);
        }

        [HttpGet("by-table/{tableId}")]
        public IActionResult GetByTable(int tableId)
        {
            var invoice = _context.Invoices
                .FirstOrDefault(i => i.TableId == tableId && i.Status == "open");

            return invoice == null
                ? NotFound("No open invoice found for this table")
                : Ok(invoice);
        }

        [HttpPost("create-for-table/{tableId}")]
        public IActionResult CreateForTable(int tableId)
        {
            var table = _context.Tables.Find(tableId);
            if (table == null) return NotFound("Table not found");

            var existingInvoice = _context.Invoices
                .FirstOrDefault(i => i.TableId == tableId && i.Status == "open");

            if (existingInvoice != null) return Ok(existingInvoice);

            var invoice = new Invoice
            {
                InvoiceDate = DateTime.Now,
                Status = "open",
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
        public IActionResult AddInvoiceItem(int invoiceId, [FromBody] InvoiceItemDto dto)
        {
            var invoice = _context.Invoices.Find(invoiceId);
            if (invoice == null) return NotFound("Invoice not found");

            var product = _context.Products.Find(dto.ProductId);
            if (product == null) return NotFound("Product not found");

            var existingItem = _context.InvoiceDetails
                .FirstOrDefault(d => d.InvoiceId == invoiceId && d.ProductId == dto.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
                existingItem.UnitPrice = dto.UnitPrice;
            }
            else
            {
                _context.InvoiceDetails.Add(new InvoiceDetail
                {
                    InvoiceId = invoiceId,
                    ProductId = dto.ProductId,
                    ProductName = product.Name,
                    Quantity = dto.Quantity,
                    UnitPrice = dto.UnitPrice
                });
            }

            _context.SaveChanges();
            UpdateInvoiceTotal(invoiceId);

            return Ok();
        }

        [HttpPut("{invoiceId}/items/{itemId}")]
        public IActionResult UpdateInvoiceItem(int invoiceId, int itemId, [FromBody] InvoiceItemDto dto)
        {
            var item = _context.InvoiceDetails
                .FirstOrDefault(d => d.Id == itemId && d.InvoiceId == invoiceId);

            if (item == null) return NotFound("Item not found");

            item.Quantity = dto.Quantity;
            item.UnitPrice = dto.UnitPrice;

            _context.SaveChanges();
            UpdateInvoiceTotal(invoiceId);

            return Ok();
        }

        [HttpDelete("{invoiceId}/items/{itemId}")]
        public IActionResult DeleteInvoiceItem(int invoiceId, int itemId)
        {
            var item = _context.InvoiceDetails
                .FirstOrDefault(d => d.Id == itemId && d.InvoiceId == invoiceId);

            if (item == null) return NotFound("Item not found");

            _context.InvoiceDetails.Remove(item);
            _context.SaveChanges();
            UpdateInvoiceTotal(invoiceId);

            return NoContent();
        }

        [HttpPut("{id}/customer")]
        public IActionResult UpdateCustomerInfo(int id, [FromBody] UpdateCustomerDto dto)
        {
            var invoice = _context.Invoices.Find(id);
            if (invoice == null) return NotFound("Invoice not found");

            invoice.CustomerName = dto.CustomerName;
            invoice.CustomerPhone = dto.CustomerPhone;
            invoice.CustomerTaxCode = dto.CustomerTaxCode;
            invoice.CustomerIdCard = dto.CustomerIdCard;
            invoice.CustomerEmail = dto.CustomerEmail;
            invoice.CustomerAddress = dto.CustomerAddress;

            _context.SaveChanges();
            return Ok(invoice);
        }

        [HttpPut("{id}/checkout")]
        public IActionResult Checkout(int id)
        {
            try
            {
                var invoice = _context.Invoices
                    .Include(i => i.Table)
                    .FirstOrDefault(i => i.Id == id);

                if (invoice == null) return NotFound();

                // Handle customer
                if (!string.IsNullOrWhiteSpace(invoice.CustomerPhone))
                {
                    var customer = _context.Customers
                        .FirstOrDefault(c => c.Phone == invoice.CustomerPhone);

                    if (customer == null)
                    {
                        customer = new Customer
                        {
                            Name = invoice.CustomerName ?? "Khách hàng",
                            Phone = invoice.CustomerPhone,
                            TaxCode = invoice.CustomerTaxCode,
                            IdCard = invoice.CustomerIdCard,
                            Email = invoice.CustomerEmail,
                            Address = invoice.CustomerAddress ?? "",
                            CreatedAt = DateTime.Now
                        };

                        _context.Customers.Add(customer);
                        _context.SaveChanges();
                    }

                    invoice.CustomerId = customer.Id;
                }

                // Update totals and checkout
                UpdateInvoiceTotal(id);
                invoice.EndTime = DateTime.Now;
                invoice.Status = "paid";

                if (invoice.Table != null)
                    invoice.Table.Status = "empty";

                _context.SaveChanges();

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private void UpdateInvoiceTotal(int invoiceId)
        {
            var invoice = _context.Invoices.Find(invoiceId);
            if (invoice == null) return;

            var subtotal = _context.InvoiceDetails
                .Where(d => d.InvoiceId == invoiceId)
                .Sum(d => (decimal?)d.Quantity * d.UnitPrice) ?? 0;

            invoice.Subtotal = subtotal;
            invoice.VatAmount = subtotal * VAT_RATE;
            invoice.TotalAmount = subtotal + invoice.VatAmount;

            _context.SaveChanges();
        }
    }

    public record InvoiceItemDto(int ProductId, int Quantity, decimal UnitPrice);

    public record UpdateCustomerDto(
        string? CustomerName,
        string? CustomerPhone,
        string? CustomerTaxCode,
        string? CustomerIdCard,
        string? CustomerEmail,
        string? CustomerAddress
    );
}