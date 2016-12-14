namespace ShoppingCart
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using ShoppingCart;

  public interface IProductCatalogueClient
  {
    Task<IEnumerable<ShoppingCartItem>>
      GetShoppingCartItems(int[] productCatalogueIds);
  }
}