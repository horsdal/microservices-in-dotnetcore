namespace ShoppingCart.EventFeed
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading;
#if net461
  using global::EventStore.ClientAPI;
#endif
  public class EventStore : IEventStore
  {
    private static long currentSequenceNumber = 0;
    private static readonly IList<Event> database = new List<Event>();
#if net461
    private const string connectionString =
     "ConnectTo=discover://admin:changeit@127.0.0.1:2112/";
    private IEventStoreConnection connection =
      EventStoreConnection.Create(connectionString);
#endif
    public IEnumerable<Event> GetEvents(
      long firstEventSequenceNumber,
      long lastEventSequenceNumber) 
      => database
          .Where(e =>
            e.SequenceNumber >= firstEventSequenceNumber &&
            e.SequenceNumber <= lastEventSequenceNumber)
          .OrderBy(e => e.SequenceNumber);

    public void Raise(string eventName, object content)
    {
      var seqNumber = Interlocked.Increment(ref currentSequenceNumber);
      database.Add(
        new Event(
          seqNumber,
          DateTimeOffset.UtcNow,
          eventName,
          content));
    }
  }
}
