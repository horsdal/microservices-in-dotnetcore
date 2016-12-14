namespace ShoppingCart
{
    using System;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using System.Linq;
    using Dapper;
    using Nancy.Owin;
    using Serilog;
    using global::ShoppingCart.Infrastructure;
    using Nancy;
    using Nancy.Bootstrapper;
    using Nancy.TinyIoc;
    using System.Net.Http;
    using Serilog.Events;

  public interface IHttpClientFactory
  {
    HttpClient Create(Uri uri);
  }

  public class HttpClientFactory : IHttpClientFactory
  {
    private readonly string correlationToken;

    public HttpClientFactory(string correlationToken)
    {
      this.correlationToken = correlationToken;
    }

    public HttpClient Create(Uri uri)
    {
      var client = new HttpClient() { BaseAddress = uri } ;
      client.DefaultRequestHeaders.Add("Correlation-Token", this.correlationToken);
      return client;
    }
  }

  public class Bootstrapper : DefaultNancyBootstrapper
  {
    private readonly ILogger log;

    public Bootstrapper(ILogger log)
    {
      this.log = log;
    }

    protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
    {
      base.ApplicationStartup(container, pipelines);
      container.Register(this.log);
    }

    protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
    {
      base.RequestStartup(container, pipelines, context);
      var correlationToken = context.GetOwinEnvironment()["correlationToken"] as string;
      container.Register<IHttpClientFactory>(new HttpClientFactory(correlationToken));
    }
  }

  public class Startup
  {
    public void Configure(IApplicationBuilder app)
    {
      var log = ConfigureLogger();
      
      app.UseOwin(buildFunc => 
      {
        buildFunc(next => GlobalErrorLogging.Middleware(next, log));
        buildFunc(next => CorrelationToken.Middleware(next));
        buildFunc(next => RequestLogging.Middleware(next, log));
        buildFunc(next => PerformanceLogging.Middleware(next, log));
        buildFunc(next => new MonitoringMiddleware(next, HealthCheck).Invoke);
        buildFunc.UseNancy(opt => opt.Bootstrapper = new Bootstrapper(log));
      });
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

    private const string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=ShoppingCart;Integrated Security=True";
    private readonly int threshold = 1000;
    
    public async Task<bool> HealthCheck()
    {
      using (var conn = new SqlConnection(connectionString))
      {
        var count = (await conn.QueryAsync<int>("select count(ID) from ShoppingCart")).Single();
        return count > this.threshold;
      }
    }
  }
}
