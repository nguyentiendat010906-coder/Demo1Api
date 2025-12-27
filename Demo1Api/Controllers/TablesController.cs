using Demo1Api.Data;
using Demo1Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Demo1Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TablesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TablesController(AppDbContext context)
        {
            _context = context;
        }

        // ===============================
        // GET: api/tables?groupId=1
        // ===============================
        [HttpGet]
        public IActionResult GetAll([FromQuery] int? groupId)
        {
            var query = _context.TableGroups
                .Include(g => g.Tables)
                .AsQueryable();

            if (groupId.HasValue && groupId.Value != 0)
                query = query.Where(g => g.Id == groupId);

            var result = query.Select(g => new
            {
                id = g.Id,
                name = g.Name,
                tables = g.Tables.Select(t => new
                {
                    id = t.Id,
                    name = t.Name,
                    status = t.Status,
                    tableGroupId = t.TableGroupId
                }).ToList()
            }).ToList();

            return Ok(result);
        }


        // ===============================
        // GET: api/tables/{id}
        // ===============================
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var table = _context.Tables
                .Include(t => t.TableGroup)
                .FirstOrDefault(t => t.Id == id);

            if (table == null)
                return NotFound();

            return Ok(new
            {
                table.Id,
                table.Name,
                
                table.Status,
                table.TableGroupId,
                TableGroupName = table.TableGroup.Name
            });
        }

        // ===============================
        // POST: api/tables
        // ===============================
        [HttpPost]
        public IActionResult Create([FromBody] Table table)
        {
            if (string.IsNullOrWhiteSpace(table.Name))
                return BadRequest("Name is required");

            

            var groupExists = _context.TableGroups.Any(g => g.Id == table.TableGroupId);
            if (!groupExists)
                return BadRequest("TableGroup not found");

            table.Status = "empty";

            _context.Tables.Add(table);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = table.Id }, table);
        }

        // ===============================
        // PUT: api/tables/{id}
        // ===============================
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Table updated)
        {
            var table = _context.Tables.Find(id);
            if (table == null)
                return NotFound();

            if (!string.IsNullOrWhiteSpace(updated.Name))
                table.Name = updated.Name;

           

            _context.SaveChanges();
            return Ok(table);
        }

        // ===============================
        // PUT: api/tables/{id}/status
        // ===============================
        [HttpPut("{id}/status")]
        public IActionResult UpdateStatus(int id, [FromBody] string status)
        {
            var table = _context.Tables.Find(id);
            if (table == null)
                return NotFound();

            status = status.ToLower().Trim();
            var allowed = new[] { "empty", "serving", "reserved" };

            if (!allowed.Contains(status))
                return BadRequest("Status must be: empty | serving | reserved");

            table.Status = status;
            _context.SaveChanges();

            return Ok();
        }

        // ===============================
        // DELETE: api/tables/{id}
        // ===============================
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var table = _context.Tables.Find(id);
            if (table == null)
                return NotFound();

            if (table.Status == "serving")
                return BadRequest("Cannot delete table that is currently serving");

            _context.Tables.Remove(table);
            _context.SaveChanges();

            return NoContent();
        }
        // ===============================
        // POST: api/tables/{id}/open
        // ===============================
        [HttpPost("{id}/open")]
        public IActionResult OpenTable(int id)
        {
            var table = _context.Tables.FirstOrDefault(t => t.Id == id);
            if (table == null)
                return NotFound("Table not found");

            if (table.Status != "empty")
                return BadRequest("Table is not empty");

            // 1️⃣ Tạo invoice mới
            var invoice = new Invoice
            {
                TableId = table.Id,
                InvoiceDate = DateTime.Now,   
                Status = "Open",
                TotalAmount = 0               
            };


            _context.Invoices.Add(invoice);
            _context.SaveChanges();

            // 2️⃣ Update bàn
            table.Status = "serving";
            table.CurrentInvoiceId = invoice.Id;

            _context.SaveChanges();

            return Ok(new
            {
                invoiceId = invoice.Id,
                tableId = table.Id
            });
        }

    }
}
