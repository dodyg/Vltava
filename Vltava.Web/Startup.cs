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

namespace Vltava.Web
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory logger, IConfiguration configuration)
        {
            //These are the four default services available at Configure
            app.Run(async context =>
            {
                try
                {
                    var fileLocation = Path.Combine(env.ContentRootPath, "Subscriptions", "tech.opml");
                    var subscription = await File.ReadAllTextAsync(fileLocation);
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
                    
                    var str = new StringBuilder();
                    str.Append("<ul>");
                    foreach (var i in items.SelectMany(x => x.Items).OrderBy(x => x.Item.Published))
                    {
                        str.Append($"<li>{i.Item.Description} - <span style=\"color:red;\">");
                        if (i.Outline != null)
                        {
                            str.Append("<ul>");
                            foreach (var o in i.Outline.Attributes)
                            {
                                str.Append($"<li>{o.Key} - {o.Value}</li>");
                            }
                            str.Append("</ul>");
                        }
                        str.Append("</li>");
                    }
                    str.Append("</ul>");

                    context.Response.Headers.Add("Content-Type", "text/html");
                    await context.Response.WriteAsync($@"
                    <html>
                        <head>
                            <link rel=""stylesheet"" type=""text/css"" href=""http://fonts.googleapis.com/css?family=Germania+One"">
                            <style>
                            body {{
                                font-family: 'Germania One', serif;
                                font-size: 24px;
                            }}
                            </style>
                        </head>
                        <body>
                            {str.ToString()}
                        </body>
                    </html>
                    ");
                }
                catch (Exception ex)
                {
                    await context.Response.WriteAsync($"Error {ex.Message}");
                }
            });
        }
    }
}