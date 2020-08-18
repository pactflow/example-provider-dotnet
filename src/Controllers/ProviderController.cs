using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Products.Controllers
{
    public class ProductsController : Controller
    {
        private IConfiguration _Configuration { get; }

        public ProductsController(IConfiguration configuration)
        {
            this._Configuration = configuration;
        }

        // GET /products
        [HttpGet]
        [Route("/products")]
        public IActionResult GetAll()
        {
            var products = new {
                id = "666",
                name = "baguettes",
                type = "food"
            };

            return new JsonResult(new[] {products});
        }

        // GET /product/id
        [HttpGet]
        [Route("/product/{id?}")]
        public IActionResult GetSingle(string? id)
        {
            var products = new {
                id = "666",
                name = "baguettes",
                type = "food"
            };

            return new JsonResult(products);
        }
    }
}
