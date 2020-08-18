using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Products.Controllers
{
    [Route("/products")]
    public class ProductsController : Controller
    {
        private IConfiguration _Configuration { get; }

        public ProductsController(IConfiguration configuration)
        {
            this._Configuration = configuration;
        }

        // GET api/provider?validDateTime=[DateTime String]
        [HttpGet]
        public IActionResult Get()
        {
            var products = new {
                id = "666",
                name = "baguettes",
                type = "food"
            };

            return new JsonResult(new[] {products});
        }
    }
}
