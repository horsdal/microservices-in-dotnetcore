namespace MicroserviceNET.Platform
{
  using System.Security.Claims;
  using Nancy;
  using Nancy.Owin;
  using Nancy.TinyIoc;
  using LibOwin;

  public static class MicroservicePlatform
  {
    private static string TokenUrl;
    private static string ClientName;
    private static string ClientSecret;

    public static void Configure(string tokenUrl, string clientName, string clientSecret)
    {
      TokenUrl = tokenUrl;
      ClientName = clientName;
      ClientSecret = clientSecret;
    }

    public static TinyIoCContainer UseHttpClientFactory(this TinyIoCContainer self, NancyContext context)
    {
      var correlationToken =
        context.GetOwinEnvironment()?["correlationToken"] as string;
      object key = null;
      context.GetOwinEnvironment()?.TryGetValue(OwinConstants.RequestUser, out key);
      var principal = key as ClaimsPrincipal;
      var idToken = principal?.FindFirst("id_token");
      self.Register<IHttpClientFactory>(new HttpClientFactory(TokenUrl, ClientName, ClientSecret, correlationToken ?? "", idToken?.Value ?? ""));
      return self;
    }
  }
}