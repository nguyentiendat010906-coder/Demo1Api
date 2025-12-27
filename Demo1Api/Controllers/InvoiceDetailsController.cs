using Demo1Api.Data;
using Demo1Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Demo1Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceDetailsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InvoiceDetailsController(AppDbContext context)
        {
            _context = context;
        }

        // ======================
        // DTOs
        // ======================
        public class AddDetailDto
        {
            public int InvoiceId { get; set; }
            public int ProductId { get; set; }
            public int Quantity { get; set; } = 1;
        }

        public class UpdateDetailDto
        {
            public int DetailId { get; set; }
            public int Quantity { get; set; }
        }

        // ======================
        // POST: api/invoicedetails/add
        // thêm hoặc tăng số lượng
        // ======================
        [HttpPost("add")]
        public IActionResult Add([FromBody] AddDetailDto dto)
        {
            var invoice = _context.Invoices
                .Include(i => i.InvoiceDetails)
                .FirstOrDefault(i => i.Id == dto.InvoiceId);

            if (invoice == null)
                return NotFound("Invoice not found");

            var product = _context.Products.Find(dto.ProductId);
            if (product == null)
                return NotFound("Product not found");

            var detail = invoice.InvoiceDetails
                .FirstOrDefault(d => d.ProductId == dto.ProductId);

            if (detail == null)
            {
                detail = new InvoiceDetail
                {
                    InvoiceId = dto.InvoiceId,
                    ProductId = dto.ProductId,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = dto.Quantity
                };
                _context.InvoiceDetails.Add(detail);
            }
            else
            {
                detail.Quantity += dto.Quantity;
            }

            // ===== RECALC TOTAL =====
            invoice.TotalAmount = invoice.InvoiceDetails.Sum(d => d.UnitPrice * d.Quantity);

            _context.SaveChanges();
            return Ok(detail);
        }

        // ======================
        // PUT: api/invoicedetails/update
        // ======================
        [HttpPut("update")]
        public IActionResult UpdateQuantity([FromBody] UpdateDetailDto dto)
        {
            var detail = _context.InvoiceDetails
                .Include(d => d.Invoice)
                .ThenInclude(i => i.InvoiceDetails)
                .FirstOrDefault(d => d.Id == dto.DetailId);

            if (detail == null)
                return NotFound();

            detail.Quantity = dto.Quantity;

            detail.Invoice.TotalAmount = detail.Invoice.InvoiceDetails.Sum(d => d.UnitPrice * d.Quantity);

            _context.SaveChanges();
            return Ok(detail);
        }

        // ======================
        // DELETE: api/invoicedetails/{id}
        // ======================
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var detail = _context.InvoiceDetails
                .Include(d => d.Invoice)
                .ThenInclude(i => i.InvoiceDetails)
                .FirstOrDefault(d => d.Id == id);

            if (detail == null)
                return NotFound();

            _context.InvoiceDetails.Remove(detail);

            detail.Invoice.TotalAmount = detail.Invoice.InvoiceDetails
                .Where(d => d.Id != id)
                .Sum(d => d.UnitPrice * d.Quantity);

            _context.SaveChanges();
            return Ok();
        }

        // ======================
        // GET: api/invoicedetails/by-invoice/{invoiceId}
        // ======================
        [HttpGet("by-invoice/{invoiceId}")]
        public IActionResult GetByInvoice(int invoiceId)
        {
            var details = _context.InvoiceDetails
                .Where(d => d.InvoiceId == invoiceId)
                .Select(d => new
                {
                    d.Id,
                    d.ProductId,
                    d.ProductName,
                    d.Quantity,
                    d.UnitPrice,
                    Total = d.Quantity * d.UnitPrice
                })
                .ToList();

            return Ok(details);
        }

    }
}
