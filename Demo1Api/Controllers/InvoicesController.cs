using Demo1Api.Data;
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
                .Select(i => new
                {
                    i.Id,
                    i.InvoiceDate,
                    i.Status,
                    i.TotalAmount,
                    CustomerName = i.Customer.Name
                })
                .ToList();

            return Ok(invoices);
        }

        // ======================
        // POST: api/invoices
        // ======================
        [HttpPost]
        public IActionResult Create(Models.Invoice invoice)
        {
            _context.Invoices.Add(invoice);
            _context.SaveChanges();
            return Ok(invoice);
        }

        // ======================
        // GET: api/invoices/{id}
        // ======================
        [HttpGet("{id}")]
        public IActionResult GetDetail(int id)
        {
            var invoice = _context.Invoices
                .Where(i => i.Id == id)
                .Select(i => new
                {
                    i.Id,
                    i.InvoiceDate,
                    i.Status,
                    i.TotalAmount,
                    Customer = new
                    {
                        i.Customer.Id,
                        i.Customer.Name,
                        i.Customer.Phone,
                        i.Customer.Address
                    },
                    InvoiceDetails = i.InvoiceDetails.Select(d => new
                    {
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
    }
}
