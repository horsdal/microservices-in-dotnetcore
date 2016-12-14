namespace LoyaltyProgram
{
  using System;
  using System.Collections.Generic;
  using System.Security.Claims;
  using Nancy;
  using Nancy.Bootstrapper;
  using Nancy.TinyIoc;
  using Nancy.Owin;

  public class Bootstrapper : DefaultNancyBootstrapper
  {
    protected override Func<ITypeCatalog, NancyInternalConfiguration> InternalConfiguration
     => NancyInternalConfiguration.WithOverrides(builder => builder.StatusCodeHandlers.Clear());

    protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
    {
      pipelines.OnError += (ctx, ex) =>
      {
        // write to central log store
        return null;
      };
    }
  }

  public class SetUser : IRequestStartup
  {
    public void Initialize(IPipelines pipelines, NancyContext context) =>
      context.CurrentUser = context.GetOwinEnvironment()["pos-end-user"] as ClaimsPrincipal;
  }
}
