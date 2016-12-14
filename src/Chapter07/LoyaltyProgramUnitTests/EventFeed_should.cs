namespace LoyaltyProgramUnitTests
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using LoyaltyProgram.EventFeed;
  using Nancy;
  using Nancy.Testing;
  using Xunit;

  public class EventFeed_should
  {
    private Browser sut;

    public EventFeed_should()
    {
      this.sut = new Browser(
        with =>
        with.Module<EventsFeedModule>().Dependency<IEventStore>(typeof (FakeEventStore)),
        withDefault => withDefault.Accept("application/json"));
    }

    [Fact]
    public async Task return_events_when_from_event_store()
    {
      var actual = await sut.Get("/events/",
                                 with =>
                                 {
                                   with.Query("start", "0");
                                   with.Query("end", "100");
                                 });

      Assert.Equal(HttpStatusCode.OK, actual.StatusCode);
      Assert.StartsWith("application/json", actual.ContentType);
      //Assert.Equal(100, actual.Body.DeserializeJson<IEnumerable<Event>>().Count());
    }

    [Fact]
    public async Task return_empty_response_when_there_are_no_more_events()
    {
      var actual = await sut.Get("/events/",
                                 with =>
                                 {
                                   with.Query("start", "200");
                                   with.Query("end", "300");
                                 });

      Assert.Empty(actual.Body.DeserializeJson<IEnumerable<Event>>());
    }
  }


  public class FakeEventStore : IEventStore
  {
    public IEnumerable<Event> GetEvents(long firstEventSequenceNumber, long lastEventSequenceNumber)
    {
      if (firstEventSequenceNumber > 100)
        return Enumerable.Empty<Event>();
      else
        return Enumerable
          .Range((int) firstEventSequenceNumber, (int) (lastEventSequenceNumber - firstEventSequenceNumber))
          .Select(i => new Event(i, DateTimeOffset.Now.AddMinutes(i), "some event", i));
    }

    public void Raise(string eventName, object content)
    {
    }
  }
}