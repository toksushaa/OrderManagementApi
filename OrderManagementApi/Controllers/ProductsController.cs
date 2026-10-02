using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderManagementApi.Data;
using OrderManagementApi.Models;

namespace OrderManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public ProductsController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll([FromQuery] string? search)
    {
        var q = _context.Products.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(p => p.Name.Contains(search));
        return await q.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> Get(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();
        return product;
    }

    [HttpPost]
    [Authorize(Roles = "Seller")]
    public async Task<ActionResult<Product>> Create([FromForm] ProductForm form)
    {
        string? imageUrl = null;

        if (form.Image != null && form.Image.Length > 0)
        {
            var uploads = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploads);

            var ext = Path.GetExtension(form.Image.FileName);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(uploads, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
                await form.Image.CopyToAsync(stream);

            imageUrl = $"/uploads/{fileName}";
        }

        var user = await _context.Users.FindAsync(CurrentUserId);

        var product = new Product
        {
            Name = form.Name,
            Description = form.Description,
            Price = form.Price,
            StockQuantity = form.StockQuantity,
            ImageUrl = imageUrl,
            SellerId = CurrentUserId,
            SellerName = user?.Username ?? "Unknown"
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return Ok(product);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();
        if (product.SellerId != CurrentUserId) return Forbid();

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

public class ProductForm
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public IFormFile? Image { get; set; }
}