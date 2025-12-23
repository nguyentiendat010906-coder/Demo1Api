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
            var query = _context.Tables
                .Include(t => t.TableGroup)
                .AsQueryable();

            if (groupId.HasValue)
                query = query.Where(t => t.TableGroupId == groupId);

            var tables = query.Select(t => new
            {
                t.Id,
                t.Name,
                t.Capacity,
                t.Status,
                t.TableGroupId,
                TableGroupName = t.TableGroup.Name
            }).ToList();

            return Ok(tables);
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
                table.Capacity,
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

            if (table.Capacity <= 0)
                return BadRequest("Capacity must be greater than 0");

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

            if (updated.Capacity > 0)
                table.Capacity = updated.Capacity;

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
    }
}
