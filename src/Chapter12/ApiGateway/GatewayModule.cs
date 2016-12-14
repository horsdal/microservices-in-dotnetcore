namespace ApiGateway
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using System.Net.Http;
  using MicroserviceNET.Platform;
  using Nancy;
  using Nancy.ModelBinding;
  using Newtonsoft.Json;
  using Serilog;
  using static System.Text.Encoding;

  public class GatewayModule : NancyModule
  {
    private static Product[] productList = new[] { new Product { ProductName = "Fancy shirt", ProductId = 1 }, new Product { ProductName = "Fancier shirt", ProductId = 2 } };

    public GatewayModule(IHttpClientFactory clientFactory, ILogger logger)
    {
      Get("/productlist", async parameters => 
      {
        var userId = (int)parameters.userid;

        var client = await clientFactory.Create(new Uri("http://localhost:5100/"), "product_catalog_read");
        var response = await client.GetAsync("/products?productIds=1,2,3,4");
        var content = await response?.Content.ReadAsStringAsync();
        logger.Information(content);
        productList =
          JsonConvert.DeserializeObject<List<Product>>(content).ToArray();

        client = await clientFactory.Create(new Uri("http://localhost:5200/"), "shopping_cart_write");
        response = await client.GetAsync($"/shoppingcart/{userId}");
        content = await response?.Content.ReadAsStringAsync();
        logger.Information(content);
        var basketProducts = GetBasketProductsFromResponse(content);

        return View["productlist", new { ProductList = productList, BasketProducts = basketProducts }];
      });

      Post("/shoppingcart/{userid}", async parameters =>
      {
        var productId = this.Bind<int>();
        var userId = (int) parameters.userid;

        var client = await clientFactory.Create(new Uri("http://localhost:5200/"), "shopping_cart_write");
        var response =  await
          client.PostAsync(
            $"/shoppingcart/{userId}/items",
            new StringContent(JsonConvert.SerializeObject(new[] { productId }), UTF8, "application/json"));
        var content = await response?.Content.ReadAsStringAsync();
        logger.Information(content);
        var basketProducts = GetBasketProductsFromResponse(content);

        logger.Information("{@basket}", basketProducts);
        return View["productlist", new { ProductList = productList, BasketProducts = basketProducts }];
      });

      Delete("/shoppingcart/{userid}", async parameters =>
      {
        var productId = this.Bind<int>();
        var userId = (int) parameters.userid;

        HttpClient client = await clientFactory.Create(new Uri("http://localhost:5200/"), "shopping_cart_write");
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/shoppingcart/{userId}/items")
        {
          Content = new StringContent(JsonConvert.SerializeObject(new[] { productId }), UTF8, "application/json")
        };
        var response = await client.SendAsync(request);
        var content = await response?.Content.ReadAsStringAsync();
        logger.Information(content);
        var basketProducts = GetBasketProductsFromResponse(content);

        logger.Information("{@basket}", basketProducts);
        return View["productlist", new { ProductList = productList, BasketProducts = basketProducts }];
      });
    }

    private List<Product> GetBasketProductsFromResponse(string responseBody)
    {
      return JsonConvert.DeserializeObject<ShoppingCart>(responseBody)
                        .Items
                        ?.Select(item =>
                                   new Product {ProductName = item.ProductName, ProductId = item.ProductCatalogueId})
                        ?.ToList() ?? new List<Product>();
    }
  }
  
  public class Product
  {
    public string ProductName;
    public int ProductId;
  }
  
      public class ShoppingCart
      {
        public IEnumerable<ShoppingCartItem> Items { get; set; }
      }

      public class ShoppingCartItem
      {
        public int ProductCatalogueId { get; set;}
        public string ProductName { get; set; }
      }
}