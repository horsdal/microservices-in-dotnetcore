using System.Collections.Generic;

namespace LoyaltyProgram.EventFeed
{
  public interface IEventStore
  {
    IEnumerable<Event> GetEvents(long firstEventSequenceNumber, long lastEventSequenceNumber);
    void Raise(string eventName, object content);
  }
}