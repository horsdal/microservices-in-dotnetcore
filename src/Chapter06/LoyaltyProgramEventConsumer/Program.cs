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
    private long start = 0, chunkSize = 100;
    private readonly Timer timer;

    public EventSubscriber(string loyaltyProgramHost)
    {
      WriteLine("created");
      this.loyaltyProgramHost = loyaltyProgramHost;
      this.timer = new Timer(10*1000);
      this.timer.AutoReset = false;
      this.timer.Elapsed += (_, __) => SubscriptionCycleCallback().Wait();
    }

    private async Task SubscriptionCycleCallback()
    {
      var response = await ReadEvents().ConfigureAwait(false);
      if (response.StatusCode == HttpStatusCode.OK)
        HandleEvents(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
      this.timer.Start();
    }

    private async Task<HttpResponseMessage> ReadEvents()
    {
      using (var httpClient = new HttpClient())
      {
        httpClient.BaseAddress = new Uri($"http://{this.loyaltyProgramHost}");
        var resource = $"/events/?start={this.start}&end={this.start + this.chunkSize}";
        var response = await httpClient.GetAsync(resource).ConfigureAwait(false);
        PrettyPrintResponse(response);
        return response;
      }
    }

    private void HandleEvents(string content)
    {
      WriteLine("Handling events");
      var events = JsonConvert.DeserializeObject<IEnumerable<Event>>(content);
      WriteLine(events);
      WriteLine(events.Count());
      foreach (var ev in events)
      {
        WriteLine(ev.Content);
        dynamic eventData = ev.Content;
        WriteLine("product name from data: " + (string) eventData.item.productName);
        this.start = Math.Max(this.start, ev.SequenceNumber + 1);
      }
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

      public static void Main(string[] args) => new Program().Main();

        public void Main()
        {
          this.subscriber = new EventSubscriber("localhost:5000");
          //Run(this);
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