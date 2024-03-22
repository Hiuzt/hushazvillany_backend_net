using hushazvillany_backend.Data;
using hushazvillany_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using MySqlConnector;
using System.IO;

namespace hushazvillany_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : Controller
    {
      
        private readonly AppDbContext _appDbContext;
        private readonly IHostEnvironment _hostEnvironment;
        public ProductController(AppDbContext appDbContext, IHostEnvironment hostEnviroment)
        {
            _appDbContext = appDbContext;
            _hostEnvironment = hostEnviroment;
        }

        [Authorize]
        [HttpPost]
        public async Task <IActionResult> AddProduct(Products products) {
            products.ImagePath = "Nincs kép feltöltve";

            IFormFile imageFile = products.ImageFile;
            products.ImagePath = await SaveImage(imageFile);

            _appDbContext.Products.Add(products);
            await _appDbContext.SaveChangesAsync();

            return Ok(new {success = true, data = products});
        }

        [HttpGet]
        public async Task <IActionResult> GetAllProducts()
        {
            var products = _appDbContext.Products;

            foreach(var product in products)
            {
                product.ImagePath = String.Format("{0}://{1}{2}/products/{3}", HttpContext.Request.Scheme, HttpContext.Request.Host, HttpContext.Request.PathBase, product.ImagePath);

            }
            return Ok(products);
        }


        [HttpGet]
        [Route("{id:int}")]
        public async Task <IActionResult> GetOneProduct(int id)
        {
            var productSource = _appDbContext.Products.FirstOrDefault(x => x.Id == id);
            if (productSource == null)
            {
                return BadRequest("Nincs ilyen termék!");
            }
            return Ok(productSource);
        }

        [Authorize]
        [HttpPut]
        [Route("{id:int}")]
        public async Task <IActionResult> UpdateOneProduct(int id, Products product)
        {
            try
            {
                var productSource = _appDbContext.Products.FirstOrDefault(x => x.Id == id);
                if (productSource == null)
                {
                    return BadRequest("Nincs ilyen termék!");
                }

                productSource.Discount = product.Discount;
                productSource.Price = product.Price;
                productSource.Name = product.Name;
                productSource.Description = product.Description;
                productSource.Category = product.Category;

                if (product.ImageFile != null) {
                    IFormFile imageFile = product.ImageFile;
                    productSource.ImagePath = await SaveImage(imageFile);
                }

                await _appDbContext.SaveChangesAsync();

                return Ok(new { success = true, data = new { message = "Egy termék sikeresen szerkesztve lett" } });
            } catch (Exception ex) 
            {
                return BadRequest(ex.ToString());
            }
               
        }

        [Authorize]
        [HttpDelete]
        [Route("{id:int}")] 
        public async Task <IActionResult> DeleteProduct(int id)
        {
            try
            {
                var productSource = _appDbContext.Products.FirstOrDefault(x => x.Id == id);
                Console.WriteLine(id);
                if (productSource == null)
                {
                    return BadRequest("Nincs ilyen termék!");
                }

                _appDbContext.Products.Remove(productSource);
                await _appDbContext.SaveChangesAsync();

                return Ok(new { success = true, data = new { message = "A termék sikeresen kitörölve" } });
            } catch(Exception ex)
            {
                return BadRequest(ex.ToString());
            }

        }

        [NonAction]
        public async Task<string> SaveImage(IFormFile imageSource)
        {
            string imageName = new String(Path.GetFileNameWithoutExtension(imageSource.FileName).Take(10).ToArray()).Replace(" ", "-");
            imageName = imageName + DateTime.Now.ToString("yymmssff") + Path.GetExtension(imageSource.FileName);

            // Image path létrehozása
            var imagePath = Path.Combine(_hostEnvironment.ContentRootPath, "Products", imageName);

            using (var fileStream = new FileStream(imagePath, FileMode.Create))
            {
                await imageSource.CopyToAsync(fileStream);
            }
            return imageName;
        }
    }
}
