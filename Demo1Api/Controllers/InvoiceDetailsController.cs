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
        // POST: api/invoicedetails/add
        // thêm hoặc tăng số lượng
        // ======================
        [HttpPost("add")]
        public IActionResult Add(
            int invoiceId,
            int productId,
            int quantity = 1)
        {
            var invoice = _context.Invoices
                .Include(i => i.InvoiceDetails)
                .FirstOrDefault(i => i.Id == invoiceId);

            if (invoice == null)
                return NotFound("Invoice not found");

            var product = _context.Products.Find(productId);
            if (product == null)
                return NotFound("Product not found");

            var detail = invoice.InvoiceDetails
                .FirstOrDefault(d => d.ProductId == productId);

            if (detail == null)
            {
                detail = new InvoiceDetail
                {
                    InvoiceId = invoiceId,
                    ProductId = productId,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = quantity
                };
                _context.InvoiceDetails.Add(detail);
            }
            else
            {
                detail.Quantity += quantity;
            }

            // ===== RECALC TOTAL =====
            invoice.TotalAmount = invoice.InvoiceDetails
                .Sum(d => d.UnitPrice * d.Quantity);

            _context.SaveChanges();
            return Ok();
        }

        // ======================
        // PUT: api/invoicedetails/update
        // ======================
        [HttpPut("update")]
        public IActionResult UpdateQuantity(int detailId, int quantity)
        {
            var detail = _context.InvoiceDetails
                .Include(d => d.Invoice)
                .ThenInclude(i => i.InvoiceDetails)
                .FirstOrDefault(d => d.Id == detailId);

            if (detail == null)
                return NotFound();

            detail.Quantity = quantity;

            detail.Invoice.TotalAmount = detail.Invoice.InvoiceDetails
                .Sum(d => d.UnitPrice * d.Quantity);

            _context.SaveChanges();
            return Ok();
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
    }
}
