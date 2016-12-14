using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using LibOwin;


namespace OwinMiddlewareTests
{
    
    using AppFunc = Func<IDictionary<string, object>, Task>;
    
    // see example explanation on xUnit.net website:
    // https://xunit.github.io/docs/getting-started-dnx.html
    public class SampleTest
    {
        private AppFunc noOp = env => Task.FromResult(0);
        
        private Func<AppFunc, AppFunc> MW =
         next => async env => 
        {
          var ctx = new OwinContext(env);
          if (ctx.Request.Path.Value == "/test/path")
            ctx.Response.StatusCode = 404;
          else
            await next(env);
        };
        
        private Func<AppFunc, AppFunc> ConsoleMV =
          next => env =>
          {
            var context = new OwinContext(env);
            var method = context.Request.Method;
            var path = context.Request.Path;
            System.Console.WriteLine($"Got request: {method} {path}");
            return next(env);
          };

        [Fact]
        public void CreateInvokeLambdaMw()
        {
          var ctx = new OwinContext();
          ctx.Request.Scheme = LibOwin.Infrastructure.Constants.Https;
          ctx.Request.Path = new PathString("/test/path");
          ctx.Request.Method = "GET";
          
          var pipeline = ConsoleMV(MW(noOp));
          
          var env = ctx.Environment;
          pipeline(env);
          
          Assert.Equal(404, ctx.Response.StatusCode);
        }

        int Add(int x, int y)
        {
            return x + y;
        }
    }
    
    public class ConsoleMiddleware
    {
      private AppFunc next;
      
      public ConsoleMiddleware(AppFunc next)
      {
        this.next = next;
      }

      public Task Invoke(IDictionary<string, object> env)
      {        
        var context = new OwinContext(env);
        var method = context.Request.Method;
        var path = context.Request.Path;
        System.Console.WriteLine($"Got request: {method} {path}");
        return next(env);
      }
    }
}
