
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Vlatava.Web
{
    public class Program
    {
        public static void Main()
        {
            var host = new WebHostBuilder()
            .UseKestrel()
            .Configure(app => app.Run(async context => await context.Response.WriteAsync("Hello World")))
            .Build();

            host.Run();
        }
    }
}