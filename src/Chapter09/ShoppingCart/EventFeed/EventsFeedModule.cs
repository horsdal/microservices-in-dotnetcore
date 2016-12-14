namespace ShoppingCart.EventFeed
{
  using Nancy;

  public class EventsFeedModule : NancyModule
  {
    public EventsFeedModule(IEventStore eventStore) : base("/events")
    {
      Get("/", _ =>
      {
        long firstEventSequenceNumber, lastEventSequenceNumber;
        if (!long.TryParse(this.Request.Query.start.Value, 
          out firstEventSequenceNumber))
          firstEventSequenceNumber = 0;
        if (!long.TryParse(this.Request.Query.end.Value, 
          out lastEventSequenceNumber))
          lastEventSequenceNumber = long.MaxValue;

        return 
          eventStore.GetEvents(
            firstEventSequenceNumber,
            lastEventSequenceNumber);
      });
    }
  }
}