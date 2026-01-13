using Microsoft.AspNetCore.Mvc;
using Demo1Api.Data;
using Demo1Api.Models;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _context;

    public CustomersController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/customers
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_context.Customers.ToList());
    }

    // POST /api/customers
    [HttpPost]
    public IActionResult Create(Customer customer)
    {
        _context.Customers.Add(customer);
        _context.SaveChanges();
        return Ok(customer);
    }

    // ✅ PUT /api/customers/{id}
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Customer customer)
    {
        var existing = _context.Customers.Find(id);
        if (existing == null)
        {
            return NotFound(); // Không tìm thấy bản ghi
        }

        // ✅ Cập nhật đầy đủ các trường
        existing.Name = customer.Name;
        existing.Phone = customer.Phone;
        existing.Email = customer.Email;
        existing.Address = customer.Address;
        existing.TaxCode = customer.TaxCode;
        existing.IdCard = customer.IdCard;
        existing.CreatedAt = customer.CreatedAt;

        _context.SaveChanges(); // ✅ Lưu thay đổi vào DB

        return Ok(existing); // Trả về dữ liệu đã cập nhật
    }


    // ✅ DELETE /api/customers/{id}
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var existing = _context.Customers.Find(id);
        if (existing == null)
        {
            return NotFound();
        }

        _context.Customers.Remove(existing);
        _context.SaveChanges();
        return NoContent();
    }
}
