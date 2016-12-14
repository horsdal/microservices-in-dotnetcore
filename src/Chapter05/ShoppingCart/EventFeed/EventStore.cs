namespace ShoppingCart.EventFeed
{
    using System;
    using System.Text;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;
    using Dapper;
    using Newtonsoft.Json;
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
      
    public async Task Raise(string eventName, object content)
    {
      await connection.ConnectAsync().ConfigureAwait(false);
      var contentJson = JsonConvert.SerializeObject(content);
      var metaDataJson =
        JsonConvert.SerializeObject(new EventMetadata
        {
          OccurredAt = DateTimeOffset.Now,
          EventName = eventName
        });

      var eventData = new EventData(
        Guid.NewGuid(),
        "ShoppingCartEvent",
        isJson: true,
        data: Encoding.UTF8.GetBytes(contentJson),
        metadata: Encoding.UTF8.GetBytes(metaDataJson)
      );

      await
        connection.AppendToStreamAsync(
          "ShoppingCart",
          ExpectedVersion.Any,
           eventData);
    }
    
    public async Task<IEnumerable<Event>> GetEvents(
      long firstEventSequenceNumber,
      long lastEventSequenceNumber)
    {
      await connection.ConnectAsync().ConfigureAwait(false);

      var result = await connection.ReadStreamEventsForwardAsync(
        "ShoppingCart",
        start:(int) firstEventSequenceNumber,
        count: (int) (lastEventSequenceNumber - firstEventSequenceNumber),
        resolveLinkTos: false).ConfigureAwait(false);

      return
        result.Events
          .Select(ev =>
            new
            {
              Content = JsonConvert.DeserializeObject(
                Encoding.UTF8.GetString(ev.Event.Data)),
              Metadata = JsonConvert.DeserializeObject<EventMetadata>(
                Encoding.UTF8.GetString(ev.Event.Data))
            })
          .Select((ev, i) =>
            new Event(
              i + firstEventSequenceNumber,
              ev.Metadata.OccurredAt,
              ev.Metadata.EventName,
              ev.Content));
    }
        
    private class EventMetadata
    {
      public DateTimeOffset OccurredAt { get; set; }
      public string EventName { get; set; }
    }
#else
    private string connectionString =
      @"Data Source=.\SQLEXPRESS;Initial Catalog=ShoppingCart;Integrated Security=True";

    private const string writeEventSql =
      @"insert into EventStore(Name, OccurredAt, Content) values (@Name, @OccurredAt, @Content)";
    public Task Raise(string eventName, object content)
    {
      var jsonContent = JsonConvert.SerializeObject(content);
      using (var conn = new SqlConnection(connectionString))
      {
        return
          conn.ExecuteAsync(
            writeEventSql,
            new
            {
              Name = eventName,
              OccurredAt = DateTimeOffset.Now,
              Content = jsonContent
            });
      }
    }
    
    private const string readEventsSql =
      @"select * from EventStore where ID >= @Start and ID <= @End";

    public async Task<IEnumerable<Event>> GetEvents(
      long firstEventSequenceNumber,
      long lastEventSequenceNumber)
    {
      using (var conn = new SqlConnection(connectionString))
      {
        return (await conn.QueryAsync<dynamic>(
          readEventsSql,
          new
          {
            Start = firstEventSequenceNumber,
            End = lastEventSequenceNumber
          }).ConfigureAwait(false))
          .Select(row =>
          {
            var content = JsonConvert.DeserializeObject(row.Content);
            return new Event(row.ID, row.OccurredAt, row.Name, content);
          });
      }
    }
#endif
  }
}
