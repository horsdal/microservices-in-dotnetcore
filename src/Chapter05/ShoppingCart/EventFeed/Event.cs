namespace ShoppingCart.EventFeed
{
  using System;

  public struct Event
  {
    public long SequenceNumber { get; }
    public DateTimeOffset OccurredAt { get; }
    public string Name { get; }
    public object Content { get; }

    public Event(
      long sequenceNumber,
      DateTimeOffset occurredAt,
      string name,
      object content)
    {
      this.SequenceNumber = sequenceNumber;
      this.OccurredAt = occurredAt;
      this.Name = name;
      this.Content = content;
    }
  }
}
