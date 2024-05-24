using Microsoft.AspNetCore.Mvc;

namespace Products.Controllers;

public class ProductsController : Controller
{
    private ProductRepository _repository;

    // This would usually be from a Repository/Data Store

    public ProductsController()
    {
        _repository = ProductRepository.GetInstance();
    }

    [HttpGet]
    [Route("/products")]
    public IActionResult GetAll()
    {
        return new JsonResult(_repository.GetProducts());
    }

    [HttpGet]
    [Route("/product/{id?}")]
    public IActionResult GetSingle(string id)
    {
        var product = _repository.GetProduct(id);
        if (product != null) 
        {
            return new JsonResult(product);
        }
        return new NotFoundResult();
    }
}
