using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderManagementApi.Data;
using OrderManagementApi.DTOs;
using OrderManagementApi.Models;
using OrderManagementApi.Services;

namespace OrderManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly JwtService _jwt;

    public AuthController(AppDbContext context, JwtService jwt)
    {
        _context = context;
        _jwt = jwt;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
            return BadRequest(new { message = "Пользователь уже существует" });

        var role = dto.Role == "Seller" ? "Seller" : "User";

        var user = new User
        {
            Username = dto.Username,
            PasswordHash = PasswordHasher.Hash(dto.Password),
            Role = role
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Регистрация успешна" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
        if (user == null || !PasswordHasher.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new { message = "Неверный логин или пароль" });

        var token = _jwt.GenerateToken(user);
        return Ok(new { token, role = user.Role, username = user.Username });
    }
}