namespace LoyaltyProgramIntegrationTests
{
  using System;
  using System.Threading;
    using Microsoft.AspNetCore.Builder;
  using Nancy;
  using Nancy.Owin;

    public class FakeEventFeed : NancyModule
    {
      public static AutoResetEvent polled = new AutoResetEvent(initialState: false);
      
      public FakeEventFeed()
      {
        this.Get("/events", _ => 
        {
          polled.Set();
          return Response.AsJson(new [] { new { SequenceNumber = 1, Name= "baz", Content = new { OfferName = "foo", Desciption = "bar", item = new { ProductName = "name" }}}});
        });
      }
    }
    
    public class FakeNotifications : NancyModule
    {
      public static bool NotificationWasSent = false;
      public FakeNotifications()
      {
        this.Get("/notify", _ => 
        {
          NotificationWasSent = true;
          Console.WriteLine("notified");
          return 200;
        }); 
      }
    }

  public class FakeStartup
  {  
    public void Configure(IApplicationBuilder app)
    {
      app.UseOwin(buildFunc => buildFunc.UseNancy());
    }
  }
}
