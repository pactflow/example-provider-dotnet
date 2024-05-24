
namespace Products;

public class Product
{
    public Product(string id, string type, string name, string version) {
        this.id = id;
        this.name = name;
        this.type = type;
        this.version = version;
    }
    public string type { get; set; }

    public string name { get; set; }

    public string id { get; set; }

    public string version { get; set; }
}
