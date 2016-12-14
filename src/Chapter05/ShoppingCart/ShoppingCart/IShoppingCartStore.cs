namespace ShoppingCart.ShoppingCart
{
  using System.Threading.Tasks;
  
  public interface IShoppingCartStore
  {
    Task<ShoppingCart> Get(int userId);
    Task Save(ShoppingCart shoppingCart);
  }
}