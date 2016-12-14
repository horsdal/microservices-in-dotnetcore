namespace ShoppingCart.ShoppingCart
{
  using System.Collections.Generic;

  public class ShoppingCartStore : IShoppingCartStore
  {
    private static readonly Dictionary<int, ShoppingCart> database = new Dictionary<int, ShoppingCart>();

    public ShoppingCart Get(int userId)
    {
      if (!database.ContainsKey(userId))
        database[userId] = new ShoppingCart(userId);
      return database[userId];
    }

    public void Save(ShoppingCart shoppingCart)
    {
      // Nothing needed. Saving would be needed with a real DB
    }
  }
}