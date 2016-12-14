namespace LoyaltyProgram
{
  using System;
  using Nancy;
  using Nancy.Bootstrapper;

  public class Bootstrapper : DefaultNancyBootstrapper
  {
    protected override Func<ITypeCatalog, NancyInternalConfiguration> InternalConfiguration
     => NancyInternalConfiguration.WithOverrides(builder => builder.StatusCodeHandlers.Clear()); 
  }
}
