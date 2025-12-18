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

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_context.Customers.ToList());
    }

    [HttpPost]
    public IActionResult Create(Customer customer)
    {
        _context.Customers.Add(customer);
        _context.SaveChanges();
        return Ok(customer);
    }
}
