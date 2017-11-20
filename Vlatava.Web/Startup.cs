using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Vlatava.Core.Protocols;
using System.Collections.Generic;
using System.Text;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace Vlatava.Web
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory logger, IConfiguration configuration)
        {
            //These are the four default services available at Configure
            app.Run(async context =>
            {
                var items = await SyndicationReader.Get(new Uri("http://scripting.com/rss.xml"));
                var str = new StringBuilder();
                str.Append("<ul>");
                foreach (var i in items)
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
            });
        }
    }
}