namespace LoyaltyProgram
{
  using Microsoft.AspNetCore.Builder;
  using Nancy.Owin;

  public class Startup
  {
    public void Configure(IApplicationBuilder app)
    {
      app.UseOwin(buildFunc => buildFunc.UseNancy());
    }
  }
}
