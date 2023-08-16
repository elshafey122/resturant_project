using EcommerceApi.api.ViewModel;
using firstproject.api.ef;
using firstproject.api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace EcommerceApi.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult>GetAllCategories()
        {
            var data = await _context.categories.Include(x => x.products)
                .Select(x => new Category
                {
                    Id = x.Id,
                    Name = x.Name,
                    products = x.products,
                })
                .ToListAsync();
            return Ok(data);
        }

        [HttpGet("getbycategoryid")]
        public async Task<IActionResult> Getbyid(int id)
        {
            var data = await _context.products.Where(x => x.categoryId == id)
                .OrderByDescending(x => x.Price).Include(x => x.category)
                .Select(x => new ProductsDetailsViewModel
                {
                    Name = x.Name,
                    Price = x.Price,
                    CategoryId = x.categoryId,
                    CategoryName = x.category.Name,
                }).ToListAsync();
            return Ok(data);
        }
        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> addcategory(CategoryViewModel dto)
        {
            var category = new Category
            {
                Name=dto.Name,
            };
            await _context.categories.AddAsync(category);
            _context.SaveChangesAsync();
            return Ok(category);
        }
        [Authorize(Roles = "admin")]
        [HttpPut("id")]
        public async Task<IActionResult> changecategory(int id, [FromBody] CategoryViewModel dto)
        {
            var item = await _context.categories.SingleOrDefaultAsync(x => x.Id == id);
            if(item is null)
            {
                return NotFound("item is not found");
            }
            item.Name=dto.Name;
            _context.SaveChanges();
            return Ok(item);
        }
        [Authorize(Roles = "admin")]
        [HttpDelete("id")]
        public async Task<IActionResult> deleteitem(int id)
        {
            var res = await _context.categories.FindAsync(id);
            if(res is null)
            {
                return NotFound("invalid id");
            }
            _context.categories.Remove(res);
            _context.SaveChangesAsync();
            return Ok(res);
        }
    }
}
