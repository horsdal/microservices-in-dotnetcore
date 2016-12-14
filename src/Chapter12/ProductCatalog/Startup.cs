namespace ProductCatalog
{
  using Microsoft.AspNetCore.Builder;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.Extensions.Logging;
  using Nancy.Owin;
  
  public class Startup
  {
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
      app.UseOwin().UseNancy();
    }
  }
}
