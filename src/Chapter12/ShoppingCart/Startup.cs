namespace ShoppingCart
{
    using Microsoft.AspNetCore.Builder;
    using Nancy;
    using Nancy.Configuration;
    using Nancy.Owin;

    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseOwin().UseNancy(opt => opt.Bootstrapper = new TracingBootstrapper());
        }
    }    
    
    public class TracingBootstrapper : Nancy.DefaultNancyBootstrapper
    {
        public override void Configure(INancyEnvironment env)
        {
            env.Tracing(enabled: true, displayErrorTraces: true);            
        }
    } 
}
