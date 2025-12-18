using Demo1Api.Data;
using Demo1Api.DTOs;
using Demo1Api.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Demo1Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ REGISTER
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDTo request)
        {
            if (_context.Users.Any(u => u.Username == request.Username))
            {
                return BadRequest(new { message = "Tài khoản đã tồn tại" });
            }

            var user = new User
            {
                Username = request.Username.Trim(),
                Password = request.Password.Trim()
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok(new { message = "Đăng ký thành công" });
        }

        // ✅ LOGIN
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDTo request)
        {
            var user = _context.Users.FirstOrDefault(u =>
                u.Username == request.Username &&
                u.Password == request.Password);

            if (user == null)
                return Unauthorized(new { message = "Sai tài khoản hoặc mật khẩu" });

            return Ok(new
            {
                message = "Đăng nhập thành công",
                userId = user.Id,
                username = user.Username
            });
        }
    }
}
