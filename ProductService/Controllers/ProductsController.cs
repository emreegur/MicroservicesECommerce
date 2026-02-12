using Microsoft.AspNetCore.Mvc;
using ProductService.Data;
using ProductService.Models;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly ProductDbContext _context;

    public ProductsController(ProductDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        // Veritabanından çek
        var products = await _context.Products.ToListAsync();
        return Ok(products);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Product newProduct)
    {
        // Veritabanına ekle
        _context.Products.Add(newProduct);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetAll), new { id = newProduct.Id }, newProduct);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound("Ürün yok!");

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return Ok($"{id} silindi.");
    }
}