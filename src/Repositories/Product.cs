
namespace Products
{
    public class Product
    {
        public Product(string id, string type, string name) {
            this.id = id;
            this.name = name;
            this.type = type;
        }
        public string type { get; set; }

        public string name { get; set; }

        public string id { get; set; }
    }
}
