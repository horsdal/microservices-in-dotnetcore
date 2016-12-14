namespace HelloMicroservicesPlatform
{
  using System;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Builder;
  using MicroserviceNET.Logging;
  using MicroserviceNET.Auth;
  using MicroserviceNET.Platform;
  using Serilog;
  using Serilog.Events;
  using Nancy;
  using Nancy.Owin;
  using Nancy.TinyIoc;
  using Nancy.Bootstrapper;

  public class Startup
  {
    public void Configure(IApplicationBuilder app)
    {
      app.UseOwin()
         .UseMonitoringAndLogging(ConfigureLogger(), HealthCheck)
         .UseAuthPlatform("test-scope")
         .UseNancy();
    }


    private ILogger ConfigureLogger()
    {
      return new LoggerConfiguration()
        .Enrich.FromLogContext()
        .WriteTo.ColoredConsole(
                                LogEventLevel.Verbose,
                                "{NewLine}{Timestamp:HH:mm:ss} [{Level}] ({CorrelationToken}) {Message}{NewLine}{Exception}")
        .CreateLogger();
      }

    private static Task<bool> HealthCheck()
    {
      return Task.FromResult(true);
    }
  }

  public class Bootstrapper : DefaultNancyBootstrapper
  {
    protected override void RequestStartup(
      TinyIoCContainer container,
      IPipelines pipelines,
      NancyContext context)
    {
      base.RequestStartup(container, pipelines, context);
      container.UseHttpClientFactory(context);
    }
  }

  public class Hello : NancyModule
  {
    public Hello(IHttpClientFactory clientFactory)
    {
      Get("/", async (_, __) =>
      {
        var client = await
        clientFactory.Create(
          new Uri("http://otherservice/"),
          "scope_for_other_microservice");
        var resp = await
          client.GetAsync("/some/path").ConfigureAwait(false);
        return resp.StatusCode;
      });
    }
  }
}

