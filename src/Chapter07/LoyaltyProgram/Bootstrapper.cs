namespace LoyaltyProgram
{
  using System;
  using Nancy;
  using Nancy.Bootstrapper;
  using Nancy.TinyIoc;

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
}
