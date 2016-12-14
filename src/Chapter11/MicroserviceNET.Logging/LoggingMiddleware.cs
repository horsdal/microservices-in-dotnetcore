namespace MicroserviceNET.Logging
{
  using System;
  using System.Diagnostics;
  using LibOwin;
  using Serilog;
  using Serilog.Context;

  using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

  public class RequestLogging
  {
    public static AppFunc Middleware(AppFunc next, ILogger log)
    {
      return async env =>
      {
        var owinContext = new OwinContext(env);
        log.Information("Incoming request: {@Method}, {@Path}, {@Headers}",
                        owinContext.Request.Method,
                        owinContext.Request.Path,
                        owinContext.Request.Headers);
        await next(env).ConfigureAwait(false);
        log.Information("Outgoing response: {@StatusCode}, {@Headers}",
                        owinContext.Response.StatusCode,
                        owinContext.Response.Headers);
      };
    }
  }

  public class PerformanceLogging
  {
    public static AppFunc Middleware(AppFunc next, ILogger log)
    {
      return async env =>
      {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        await next(env).ConfigureAwait(false);
        stopWatch.Stop();
        var owinContext = new OwinContext(env);
        log.Information("Request: {@Method} {@Path} executed in {RequestTime:000} ms",
                        owinContext.Request.Method, owinContext.Request.Path,
                        stopWatch.ElapsedMilliseconds);
      };
    }
  }

  public class CorrelationToken
  {
    public static AppFunc Middleware(AppFunc next)
    {
      return async env =>
      {
        Guid correlationToken;
        var owinContext = new OwinContext(env);
        if (!(owinContext.Request.Headers["Correlation-Token"] != null
              && Guid.TryParse(owinContext.Request.Headers["Correlation-Token"], out correlationToken)))
          correlationToken = Guid.NewGuid();

        owinContext.Set("correlationToken", correlationToken.ToString());
        using (LogContext.PushProperty("CorrelationToken", correlationToken))
          await next(env).ConfigureAwait(false);
      };
    }
  }

  public class GlobalErrorLogging
  {
    public static AppFunc Middleware(AppFunc next, ILogger log)
    {
      return async env =>
      {
        try
        {
          await next(env).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
          log.Error(ex, "Unhandled exception");
        }
      };
    }
  }
}