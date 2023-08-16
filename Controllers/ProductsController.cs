using EcommerceApi.api.ViewModel;
using firstproject.api.ef;
using firstproject.api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApi.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult>getproducts()
        {
            var res = await _context.products.Include(x=>x.category)
                .Select(x => new ProductsDetailsViewModel
                {
                    Name = x.Name,
                    Price = x.Price,
                    CategoryId = x.categoryId,
                    CategoryName = x.category.Name
                }).ToListAsync();
            return Ok(res);
        }
        [HttpGet("id")]
        public async Task<IActionResult>getproductid(int id)
        {
            var res = await _context.products.Include(x=>x.category).SingleOrDefaultAsync(m=>m.Id==id);
            if(res is null)
            {
                return NotFound("no item with id");
            }
            var dto = new ProductsDetailsViewModel
            {
                Name = res.Name,
                Price = res.Price,
                CategoryId = res.categoryId,
                CategoryName = res.category.Name
            };
            return Ok(dto);
        }
        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult>postasync(ProductViewModel dto)
        {
            var isvalidcategory = await _context.categories.AnyAsync(x => x.Id == dto.CategoryId);
            if(!isvalidcategory)
            {
                return BadRequest("category id is not valid");
            }
            var product = new Product
            {
                Name = dto.Name,
                Price = dto.Price,
                categoryId = dto.CategoryId
            };
            await _context.products.AddAsync(product);
            _context.SaveChanges();
            return Ok(product);
        }
        [Authorize(Roles = "admin")]
        [HttpPut("id")]
        public async Task<IActionResult> putproduct(int id,[FromBody] ProductViewModel dto)
        {
            var item = await _context.products.SingleOrDefaultAsync(x => x.Id == id);
            if(item is null)
            {
                return NotFound();
            }
            var checkcatid = await _context.categories.FindAsync(dto.CategoryId);
            if (checkcatid==null)
            {
                return NotFound("category id not found");
            }
            item.Price = dto.Price;
            item.Name = dto.Name;
            item.categoryId = dto.CategoryId;
            await _context.SaveChangesAsync();
            return Ok(item);
        }
        [Authorize(Roles = "admin")]
        [HttpDelete("id")]
        public async Task<IActionResult> deleteproduct(int id)
        {
            var item = await _context.products.SingleOrDefaultAsync(x => x.Id == id);
            if (item is null)
            {
                return NotFound();
            }
            _context.products.Remove(item);
            _context.SaveChanges();
            return Ok(item);
        }
    }
}
