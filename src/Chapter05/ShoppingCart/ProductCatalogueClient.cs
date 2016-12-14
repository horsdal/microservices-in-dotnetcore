namespace ShoppingCart
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Net.Http;
    using System.Threading;
    using Newtonsoft.Json;
    using Polly;
    using ShoppingCart;
    using System.Net.Http.Headers;

    public class ProductCatalogueClient : IProductCatalogueClient
  {
    private static Policy exponentialRetryPolicy =
      Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(
          3, 
          attempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)), (ex, _) => Console.WriteLine(ex.ToString()));

    private static string productCatalogueBaseUrl =
      @"http://private-05cc8-chapter2productcataloguemicroservice.apiary-mock.com";
    private static string getProductPathTemplate =
      "/products?productIds=[{0}]";
    private readonly ICache cache;  
    
    public ProductCatalogueClient(ICache cache)
    {
      this.cache = cache;
    }

    public Task<IEnumerable<ShoppingCartItem>>
      GetShoppingCartItems(int[] productCatalogueIds) =>
      exponentialRetryPolicy
        .ExecuteAsync(() => GetItemsFromCatalogueService(productCatalogueIds));

    private async Task<IEnumerable<ShoppingCartItem>>
      GetItemsFromCatalogueService(int[] productCatalogueIds)
    {
      var response = await
        RequestProductFromProductCatalogue(productCatalogueIds).ConfigureAwait(false);
      return await ConvertToShoppingCartItems(response).ConfigureAwait(false);
    }

    private async Task<HttpResponseMessage> RequestProductFromProductCatalogue(int[] productCatalogueIds)
    {
      var productsResource = string.Format(
        getProductPathTemplate, string.Join(",", productCatalogueIds));
      var response = this.cache.Get(productsResource) as HttpResponseMessage;
      if (response == null)
      {
        using (var httpClient = new HttpClient())
        {
          httpClient.BaseAddress = new Uri(productCatalogueBaseUrl);
          response = await httpClient.GetAsync(productsResource).ConfigureAwait(false);
          AddToCache(productsResource, response);
        }
      }
      return response;
    }
    
    private void AddToCache(string resource, HttpResponseMessage response)
    {
      var cacheHeader = response
        .Headers
        .FirstOrDefault(h => h.Key == "cache-control");
      if (string.IsNullOrEmpty(cacheHeader.Key))
        return;
      var maxAge =
        CacheControlHeaderValue.Parse(cacheHeader.Value.ToString())
          .MaxAge;
      if (maxAge.HasValue)
        this.cache.Add(key: resource, value: response, ttl: maxAge.Value);
    }

    private static async Task<IEnumerable<ShoppingCartItem>> ConvertToShoppingCartItems(HttpResponseMessage response)
    {
      response.EnsureSuccessStatusCode();
      var products = 
        JsonConvert.DeserializeObject<List<ProductCatalogueProduct>>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
      return
        products
          .Select(p => new ShoppingCartItem(
            int.Parse(p.ProductId),
            p.ProductName,
            p.ProductDescription,
            p.Price
        ));
    }

    private class ProductCatalogueProduct
    {
      public string ProductId { get; set; }
      public string ProductName { get; set; }
      public string ProductDescription { get; set; }
      public Money Price { get; set; }
    }
  }
}