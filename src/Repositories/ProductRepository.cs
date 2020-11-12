
using System.Collections.Generic;

namespace Products
{
  public sealed class ProductRepository
  {
      private static readonly ProductRepository _instance = new ProductRepository();
      private List<Product> _products = new List<Product>();

      // Explicit static constructor to tell C# compiler
      // not to mark type as beforefieldinit
      static ProductRepository()
      {
      }

      private ProductRepository()
      {
        this._products.Add(new Product("1", "food", "pancake", "1.0.0"));
        this._products.Add(new Product("2", "food", "sanwhich", "1.0.0"));
      }

      public static ProductRepository GetInstance()
      {
        return _instance;
      }

      public void AddProduct(Product product) {
        _products.Add(product);
      }

      public void RemoveProducts() {
        _products.Clear();
      }

      public List<Product> GetProducts() {
        return _products;
      }

      public Product GetProduct(string id) {
        return _products.Find(product => product.id == id);
      }
  }
}
