using Demo1Api.Data;
using Demo1Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Demo1Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TableGroupsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TableGroupsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/TableGroups
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetGroups()
        {
            var groups = await _context.TableGroups
                .Include(g => g.Tables)
                .ToListAsync();

            // Map sang format đúng cho Angular
            var result = groups.Select(g => new
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
            });

            return Ok(result);
        }

        // GET: api/TableGroups/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetGroupById(int id)
        {
            var group = await _context.TableGroups
                .Include(g => g.Tables)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null) return NotFound();

            // Map sang format đúng
            var result = new
            {
                id = group.Id,
                name = group.Name,
                tables = group.Tables.Select(t => new
                {
                    id = t.Id,
                    name = t.Name,
                    status = t.Status,
                    tableGroupId = t.TableGroupId
                }).ToList()
            };

            return Ok(result);
        }

        // POST: api/TableGroups
        [HttpPost]
        public async Task<ActionResult<TableGroup>> CreateGroup([FromBody] TableGroup group)
        {
            if (string.IsNullOrWhiteSpace(group.Name))
                return BadRequest("Name is required");

            _context.TableGroups.Add(group);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGroupById), new { id = group.Id }, group);
        }

        // DELETE: api/TableGroups/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGroup(int id)
        {
            var group = await _context.TableGroups.FindAsync(id);
            if (group == null) return NotFound();

            _context.TableGroups.Remove(group);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/TableGroups/{id}/tables
        [HttpPost("{id}/tables")]
        public async Task<ActionResult<Table>> AddTable(int id, [FromBody] Table table)
        {
            var group = await _context.TableGroups.FindAsync(id);
            if (group == null) return NotFound();

            table.TableGroupId = id;
            table.Status = "empty";

            _context.Tables.Add(table);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetGroupById), new { id = id }, table);
        }

        // DELETE: api/TableGroups/{groupId}/tables/{tableId}
        [HttpDelete("{groupId}/tables/{tableId}")]
        public async Task<IActionResult> DeleteTable(int groupId, int tableId)
        {
            var table = await _context.Tables
                .FirstOrDefaultAsync(t => t.Id == tableId && t.TableGroupId == groupId);

            if (table == null) return NotFound();

            _context.Tables.Remove(table);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}