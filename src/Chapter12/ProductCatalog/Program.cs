namespace ProductCatalog
{
  using System.IO;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.Extensions.Configuration;
  public class Program
  {
    public static void Main(string[] args)
    {
      var config = new ConfigurationBuilder()
           .AddEnvironmentVariables(prefix: "ASPNETCORE_")
           .Build();
       
       var host = new WebHostBuilder()
           .UseConfiguration(config)
           .UseKestrel()
           .UseUrls("http://localhost:5100")
           .UseContentRoot(Directory.GetCurrentDirectory())
           .UseIISIntegration()
           .UseStartup<Startup>()
           .Build();
      host.Run();
    }
  }
}
