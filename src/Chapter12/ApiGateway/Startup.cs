namespace ApiGateway
{
  using System.Threading.Tasks;
  using MicroserviceNET.Logging;
  using MicroserviceNET.Platform;
  using MicroserviceNET.Auth;
  using Microsoft.AspNetCore.Builder;
  using Nancy;
  using Nancy.Bootstrapper;
  using Nancy.Owin;
  using Nancy.TinyIoc;
  using Serilog;
  using Serilog.Events;

  public class Startup
  {
    public void Configure(IApplicationBuilder app)
    {
      var logger = ConfigureLogger();
      app.UseStaticFiles();
      app.UseOwin()
        .UseMonitoringAndLogging(logger, HealtCheck)
        .UseNancy(opt => opt.Bootstrapper = new Bootstrapper(logger));
    }

    private ILogger ConfigureLogger()
    {      
      MicroservicePlatform.Configure(
        tokenUrl: "http://localhost:5001/",
        clientName:"api_gateway",
        clientSecret: "secret");
      return new LoggerConfiguration()
        .Enrich.FromLogContext()
        .WriteTo.ColoredConsole(
          LogEventLevel.Verbose,
          "{NewLine}{Timestamp:HH:mm:ss} [{Level}] ({CorrelationToken}) {Message}{NewLine}{Exception}")
        .CreateLogger();
    }

    private static Task<bool> HealtCheck()
    {
        return Task.FromResult(true);
    }
 }
 
  public class Bootstrapper : DefaultNancyBootstrapper
  {
    private ILogger logger;
    public Bootstrapper(ILogger logger)
    {
      this.logger = logger;
    }
    protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
    {
      base.ApplicationStartup(container, pipelines);
      container.Register(logger);
      container.UseHttpClientFactory(new NancyContext());
    }

    protected override void RequestStartup(
      TinyIoCContainer container,
      IPipelines pipelines,
      NancyContext context)
    {
      base.RequestStartup(container, pipelines, context);
      container.UseHttpClientFactory(context);
    }
  }

  public class CustomRootPathProvider : IRootPathProvider
  {
      public string GetRootPath()
      {
          return @"C:\Users\chors_000\Documents\horsdal\code\Chapter12\ApiGateway";
      }
  }
}

