namespace MicroserviceNET.Logging
{
  using System;
  using System.Threading.Tasks;
  using Serilog;

   using BuildFunc = System.Action<System.Func<
    System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>,
    System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>>>;

  public static class BuildFuncExtensions
  {
    public static BuildFunc UseMonitoringAndLogging(
      this BuildFunc buildFunc,
      ILogger log,
      Func<Task<bool>> healthCheck)
    {
      buildFunc(next => GlobalErrorLogging.Middleware(next, log));
      buildFunc(next => CorrelationToken.Middleware(next));
      buildFunc(next => RequestLogging.Middleware(next, log));
      buildFunc(next => PerformanceLogging.Middleware(next, log));
      buildFunc(next => new MonitoringMiddleware(next, healthCheck).Invoke);
      return buildFunc;
    }
  }
}