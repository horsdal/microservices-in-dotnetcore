using static System.Console;

namespace LoyaltyProgramEventConsumer
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Net;
  using System.Net.Http;
  using System.ServiceProcess;
  using System.Threading.Tasks;
  using System.Timers;
  using Newtonsoft.Json;

  public class EventSubscriber
  {
    private readonly string loyaltyProgramHost;
    private readonly string notificationHost;
    private long start = 0, chunkSize = 100;
    private readonly Timer timer;

    public EventSubscriber(string loyaltyProgramHost)
    {
      WriteLine("created");
      this.loyaltyProgramHost = loyaltyProgramHost;
      this.notificationHost = loyaltyProgramHost;
      this.timer = new Timer(10*1000);
      this.timer.AutoReset = false;
      this.timer.Elapsed += (_, __) => SubscriptionCycleCallback().Wait();
    }

    private async Task SubscriptionCycleCallback()
    {
      var response = await ReadEvents();
      if (response.StatusCode == HttpStatusCode.OK)
        await HandleEvents(await response.Content.ReadAsStringAsync());
      this.timer.Start();
    }

    private async Task<HttpResponseMessage> ReadEvents()
    {
      using (var httpClient = new HttpClient())
      {
        httpClient.BaseAddress = new Uri($"http://{this.loyaltyProgramHost}");
        var response = await httpClient.GetAsync($"/events/?start={this.start}&end={this.start + this.chunkSize}").ConfigureAwait(false);
        PrettyPrintResponse(response);
        return response;
      }
    }

    private async Task HandleEvents(string content)
    {
      WriteLine("Handling events");
      var events = JsonConvert.DeserializeObject<IEnumerable<Event>>(content);
      WriteLine(events);
      WriteLine(events.Count());

      var results = new List<Task>();
      foreach (var ev in events)
      {
        WriteLine(ev.Content);
        dynamic eventData = ev.Content;
        WriteLine("product name from data: " + (string) eventData.item.productName);
        this.start = Math.Max(this.start, ev.SequenceNumber + 1);

        using (var httpClient = new HttpClient())
        {
          WriteLine("notifying");
          httpClient.BaseAddress = new Uri($"http://{this.notificationHost}");
          var response = await httpClient.GetAsync("/notify").ConfigureAwait(false);
          PrettyPrintResponse(response);
//          results.Add(httpClient.GetAsync("/notify"));
        }
      }
//      await Task.WhenAll(results).ConfigureAwait(false);
      WriteLine("done notifying");
    }


    public void Start()
    {
      this.timer.Start();
    }

    public void Stop()
    {
      this.timer.Stop();
    }

    private static async void PrettyPrintResponse(HttpResponseMessage response)
    {
      WriteLine("Status code: " + response?.StatusCode.ToString() ?? "command failed");
      WriteLine("Headers: " + response?.Headers.Aggregate("", (acc, h) => acc + "\n\t" + h.Key + ": " + h.Value) ?? "");
      WriteLine("Body: " + await response?.Content.ReadAsStringAsync() ?? "");
    }
  }

  public struct Event
  {
    public long SequenceNumber { get; set; }
    public string Name { get; set; }
    public object Content { get; set; }
  }

  public class Program : ServiceBase
  {
    private EventSubscriber subscriber;

    public static void Main(string[] args) => new Program().Entry(args);

    public void Entry(string[] args)
    {
      this.subscriber = new EventSubscriber(args[0]);
      if (args.Length >= 2 && args[1].Equals("--service"))
        Run(this);
      else
        OnStart(null);
      ReadLine();
    }

    protected override void OnStart(string[] args)
    {
      this.subscriber.Start();
    }

    protected override void OnStop()
    {
      this.subscriber.Stop();
    }
  }
}