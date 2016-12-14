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
      return productIds.Select(id => new ProductCatalogProduct(id, "foo" + id, "bar", new Money()));
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