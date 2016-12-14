namespace MicroserviceNET.Auth
{
  using System.IdentityModel.Tokens.Jwt;
  using System.Threading.Tasks;
  using LibOwin;
  using Microsoft.IdentityModel.Tokens;

  using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

  public class Authorization
  {
    public static AppFunc Middleware(AppFunc next, string requiredScope)
    {
      return env =>
      {
        var ctx = new OwinContext(env);
        var principal = ctx.Request.User;
        if (principal.HasClaim("scope", requiredScope))
          return next(env);
        ctx.Response.StatusCode = 403;
        return Task.FromResult(0);
      };
    }
  }

  public class IdToken
  {
    public static AppFunc Middleware(AppFunc next)
    {
      return env =>
      {
        var ctx = new OwinContext(env);
        if (ctx.Request.Headers.ContainsKey("microservice.NET-end-user"))
        {
          var tokenHandler = new JwtSecurityTokenHandler();
          SecurityToken token;
          var userPrincipal =
            tokenHandler.ValidateToken(ctx.Request.Headers["microservice.NET-end-user"],
                                       new TokenValidationParameters(), out token);
          ctx.Set("pos-end-user", userPrincipal);
        }
        return next(env);
      };
    }
  }
}
