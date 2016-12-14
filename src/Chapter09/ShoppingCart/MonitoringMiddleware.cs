namespace ShoppingCart.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LibOwin;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

  public class MonitoringMiddleware
  {
    private AppFunc next;
    private Func<Task<bool>> healthCheck;
    
    private static readonly PathString monitorPath = new PathString("/_monitor");
    private static readonly PathString monitorShallowPath = new PathString("/_monitor/shallow");
    private static readonly PathString monitorDeepPath = new PathString("/_monitor/deep");

    public MonitoringMiddleware(AppFunc next, Func<Task<bool>> healthCheck)
    {
      this.next = next;
      this.healthCheck = healthCheck;
    }
    
    public Task Invoke(IDictionary<string, object> env)
    {
      var context = new OwinContext(env);
      if (context.Request.Path.StartsWithSegments(monitorPath))
        return HandleMonitorEndpoint(context);
      else
        return this.next(env); 
    }

    private Task HandleMonitorEndpoint(OwinContext context)
    {
      if (context.Request.Path.StartsWithSegments(monitorShallowPath))
        return ShallowEndpoint(context);
      else if (context.Request.Path.StartsWithSegments(monitorDeepPath))
        return DeepEndpoint(context);
      return Task.FromResult(0);
    }

    private async Task DeepEndpoint(OwinContext context)
    {
      if (await this.healthCheck())
        context.Response.StatusCode = 204;
      else
        context.Response.StatusCode = 503;
    }

    private Task ShallowEndpoint(OwinContext context)
    {
      context.Response.StatusCode = 204;
      return Task.FromResult(0);
    }
  }
}
