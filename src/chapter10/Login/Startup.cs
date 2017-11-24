using IdentityServer4.Models;

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
      services.AddIdentityServer()
              .AddSigningCredential(cert)
              .AddInMemoryIdentityResources(new IdentityResource[]
              {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
                new IdentityResource
                {
                  Name = "loyalty_program_write",
                  DisplayName = "Loyalty Program write access"
                },
              })
              .AddInMemoryApiResources(new[] {new ApiResource("loyalty_program_write", "Loyalty Program"),})
              .AddInMemoryClients(Clients.Get());

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
