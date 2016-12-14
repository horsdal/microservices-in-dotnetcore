namespace LoyaltyProgram
{
  using System.Collections.Generic;
  using System.IdentityModel.Tokens.Jwt;
  using System.Text.Encodings.Web;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Builder;
  using Microsoft.IdentityModel.Tokens;
  using LibOwin;
  using Microsoft.Extensions.DependencyInjection;
  using Nancy.Owin;

  public class Startup
  {
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddTransient(provider => UrlEncoder.Default);
    }

    public void Configure(IApplicationBuilder app)
    {
      JwtSecurityTokenHandler.DefaultInboundClaimTypeMap = new Dictionary<string, string>();

      var options = new JwtBearerOptions
      {
        Authority = "http://localhost:5001",
        RequireHttpsMetadata = false,
        Audience = "http://localhost:5001/resources",
        AutomaticAuthenticate = true
      };
      app.UseJwtBearerAuthentication(options);

      app.UseOwin(buildFunc =>
      {
        buildFunc(next => env =>
        {
          var ctx = new OwinContext(env);
          var principal = ctx.Request.User;
          if (principal?.HasClaim("scope", "loyalty_program_write") ?? false)
            return next(env);
          ctx.Response.StatusCode = 403;
          return Task.FromResult(0);
        });
        buildFunc(next => env =>
        {
          var ctx = new OwinContext(env);
          var idToken = ctx.Request.User?.FindFirst("id_token");
          if (idToken != null)
          {
            ctx.Set("pos-end-user-token", idToken);
          }
          return next(env);
        });
        buildFunc(next => env =>
        {
          var ctx = new OwinContext(env);
          if (ctx.Request.Headers.ContainsKey("pos-end-user"))
          {
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token;
            var userPrincipal =
              tokenHandler.ValidateToken(ctx.Request.Headers["pos-end-user"],
                                         new TokenValidationParameters(),
                                         out token);
            ctx.Set("pos-end-user", userPrincipal);
          }
          return next(env);
        });
        buildFunc.UseNancy();
      });
    }
  }
}
