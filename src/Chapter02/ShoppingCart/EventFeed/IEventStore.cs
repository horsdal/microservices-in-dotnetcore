using System.Collections.Generic;
using ShoppingCart.ShoppingCart;

namespace ShoppingCart.EventFeed
{
  public interface IEventStore
  {
    IEnumerable<Event> GetEvents(long firstEventSequenceNumber, long lastEventSequenceNumber);
    void Raise(string eventName, object content);
  }
}