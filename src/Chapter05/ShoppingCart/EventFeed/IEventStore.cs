namespace ShoppingCart.EventFeed
  {
  using System.Collections.Generic;
  using System.Threading.Tasks;
    
  public interface IEventStore
  {
    Task<IEnumerable<Event>> GetEvents(long firstEventSequenceNumber, 
      long lastEventSequenceNumber);
    Task Raise(string eventName, object content);
  }
}