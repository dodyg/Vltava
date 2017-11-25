using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Vltava.Core.Protocols;
using System.Collections.Generic;
using System.Text;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.IO;
using Vltava.Core.Framework;
using Microsoft.Extensions.DependencyInjection;
using Optional;
using Optional.Unsafe;
using Vltava.Core.Features;

namespace Vltava.Web
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection  services)
        {
            services.AddSingleton<SystemFolders>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory logger, IConfiguration configuration)
        {
            var sysFolders = app.ApplicationServices.GetService<SystemFolders>();

            //These are the four default services available at Configure
            app.Run(async context =>
            {
                try
                {
                    var subscriptionListFile =  sysFolders.SubscriptionsFile("tech.opml");
                    if (!subscriptionListFile.HasValue)
                        throw new ArgumentException($"{subscriptionListFile} does not exist");

                    var subscription = await File.ReadAllTextAsync(subscriptionListFile.ValueOrFailure());
                    var opml = Opml.Parse(subscription);

                    if (opml.IsFalse)
                    {
                        await context.Response.WriteAsync("Error in loading");
                        return;
                    }

                    var outlines = opml.Value.Find("type", "rss");
                    var urls = outlines.Where(o => o.Attributes.ContainsKey("url")).Select(o => new Uri(o["url"]));

                    var items = await SyndicationReader.Get(urls.ToArray());
                    SyndicationReader.SyndicationItemStream.Subscribe(x => Console.WriteLine(x.Item.Title));

                    var templateFile = sysFolders.TemplateFile("default.scriban-html");
                    if (!templateFile.HasValue)
                        throw new ArgumentException($"{templateFile} does not exist");

                    var template = await File.ReadAllTextAsync(templateFile.ValueOrFailure());

                    var render = new HtmlRender();
                    var output = render.Render(template, items);

                    context.Response.Headers.Add("Content-Type", "text/html");
                    await context.Response.WriteAsync(output);
                }
                catch (Exception ex)
                {
                    await context.Response.WriteAsync($"Error {ex.Message}");
                }
            });
        }
    }
}