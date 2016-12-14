namespace ProductCatalog
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Nancy;
  
  public class ProductsModule : NancyModule
  {
    public ProductsModule(ProductStore productStore) : base("/products")
    {
      Get("", _ =>
      {
        string productIdsString = this.Request.Query.productIds;
        var productIds = ParseProductIdsFromQueryString(productIdsString);
        var products = productStore.GetProductsByIds(productIds);

        return
          this
           .Negotiate
           .WithModel(products)
           .WithHeader("cache-control", "max-age:86400");
      });
    }

    private static IEnumerable<int> ParseProductIdsFromQueryString(string productIdsString)
    {
        return productIdsString.Split(',').Select(s => s.Replace("[", "").Replace("]", "")).Select(int.Parse);
    }
  }

  public interface ProductStore
  {
      IEnumerable<ProductCatalogProduct> GetProductsByIds(IEnumerable<int> productIds);
  }

  public class StaticProductStore : ProductStore
  {
    public IEnumerable<ProductCatalogProduct> GetProductsByIds(IEnumerable<int> productIds)
    {
      return productIds.Select(id => new ProductCatalogProduct(id, "Product no. " + id, "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum", new Money()));
    }
  }

  public class ProductCatalogProduct                                
  {
    public ProductCatalogProduct(int productId, string productName, string description, Money price)
    {
        this.ProductId = productId.ToString();
        this.ProductName = productName;
        this.ProductDescription = description;
        this.Price = price;
    }
    public string ProductId { get; private set; }
    public string ProductName { get; private set; }
    public string ProductDescription { get; private set; }
    public Money Price { get; private set; }
  }
    
  public class Money { }
}