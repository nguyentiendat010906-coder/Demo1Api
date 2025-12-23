using Microsoft.AspNetCore.Mvc;
using Demo1Api.Data;
using Demo1Api.Models;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/products
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_context.Products.ToList());
    }

    // POST: api/products
    [HttpPost]
    public IActionResult Create(Product product)
    {
        // ===== VALIDATE CATEGORY =====
        var validCategories = new[]
        {
            "Bar",
            "Bếp",
            "Tính thời gian",
            "Khác"
        };

        if (!validCategories.Contains(product.Category))
        {
            return BadRequest("Category không hợp lệ");
        }

        // ===== VALIDATE UNIT TYPE =====
        if (product.UnitType == "Thời gian")
        {
            product.Stock = null; // bắt buộc không có kho
        }
        else if (product.UnitType == "Số lượng")
        {
            if (product.Stock == null)
                return BadRequest("Sản phẩm theo số lượng phải có tồn kho");
        }
        else
        {
            return BadRequest("UnitType không hợp lệ");
        }

        _context.Products.Add(product);
        _context.SaveChanges();

        return Ok(product);
    }
}
