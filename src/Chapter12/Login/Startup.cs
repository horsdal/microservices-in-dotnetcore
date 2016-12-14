namespace Login
{
  using System.IO;
  using Microsoft.AspNetCore.Builder;
  using Microsoft.AspNetCore.Hosting;
  using System.Security.Cryptography.X509Certificates;
  using Microsoft.AspNetCore.Http;
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.DependencyInjection;

  using Configuration;

  public class Startup
  {
    private readonly IHostingEnvironment environment;

    public Startup(IHostingEnvironment env)
    {
      this.environment = env;
    }

    public void ConfigureServices(IServiceCollection services)
    {
      var cert = new X509Certificate2(Path.Combine(this.environment.ContentRootPath, "idsrv3test.pfx"), "idsrv3test");

      services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
      var builder = services
        .AddIdentityServer()
        .SetSigningCredential(cert)
        .AddInMemoryClients(Clients.Get())
        .AddInMemoryScopes(Scopes.Get())
        .AddInMemoryUsers(Users.Get());

      services.AddMvc();
    }

    public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
    {
      loggerFactory.AddConsole(LogLevel.Trace);
      loggerFactory.AddDebug(LogLevel.Trace);

      app.UseCookieAuthentication(new CookieAuthenticationOptions
      {
        AuthenticationScheme = "Temp",
        AutomaticAuthenticate = false,
        AutomaticChallenge = false
      });

      app.UseIdentityServer();

      app.UseStaticFiles();
      app.UseMvcWithDefaultRoute();
    }
  }
}
