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

        public InvoicesController(AppDbContext context)
        {
            _context = context;
        }

        // ======================
        // GET: api/invoices
        // ======================
        [HttpGet]
        public IActionResult GetAll()
        {
            var invoices = _context.Invoices
                .Include(i => i.Customer)
                .Select(i => new
                {
                    i.Id,
                    i.InvoiceDate,
                    i.Status,
                    i.TotalAmount,
                    i.TableId,
                    CustomerName = i.Customer != null ? i.Customer.Name : null
                })
                .ToList();

            return Ok(invoices);
        }

        // ======================
        // GET: api/invoices/{id}
        // ======================
        [HttpGet("{id}")]
        public IActionResult GetDetail(int id)
        {
            var invoice = _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.InvoiceDetails)
                .Include(i => i.Table)
                .Where(i => i.Id == id)
                .Select(i => new
                {
                    i.Id,
                    i.InvoiceDate,
                    i.Status,
                    i.TotalAmount,
                    i.TableId,
                    TableStatus = i.Table.Status,
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

            if (invoice == null)
                return NotFound();

            return Ok(invoice);
        }

        // ======================
        // POST: api/invoices/create-for-table/{tableId}
        // CLICK BÀN → TẠO HÓA ĐƠN
        // ======================
        [HttpPost("create-for-table/{tableId}")]
        public IActionResult CreateForTable(int tableId)
        {
            var table = _context.Tables.FirstOrDefault(t => t.Id == tableId);
            if (table == null)
                return NotFound("Table not found");

            // Nếu bàn đang phục vụ → lấy hóa đơn cũ
            var existingInvoice = _context.Invoices
                .FirstOrDefault(i =>
                    i.TableId == tableId &&
                    i.Status == "open");

            if (existingInvoice != null)
                return Ok(existingInvoice);

            // Tạo hóa đơn mới
            var invoice = new Invoice
            {
                InvoiceDate = DateTime.Now,
                Status = "open",
                TotalAmount = 0,
                TableId = tableId
            };

            // Đổi trạng thái bàn
            table.Status = "serving";

            _context.Invoices.Add(invoice);
            _context.SaveChanges();

            return Ok(invoice);
        }

        // ======================
        // PUT: api/invoices/{id}/checkout
        // THANH TOÁN
        // ======================
        [HttpPut("{id}/checkout")]
        public IActionResult Checkout(int id)
        {
            var invoice = _context.Invoices
                .Include(i => i.Table)
                .FirstOrDefault(i => i.Id == id);

            if (invoice == null)
                return NotFound();

            invoice.Status = "paid";

            if (invoice.Table != null)
                invoice.Table.Status = "empty";

            _context.SaveChanges();

            return Ok();
        }
    }
}
