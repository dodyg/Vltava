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
using System.Threading.Tasks.Dataflow;
using System.Threading.Tasks;
using Optional.Linq;

namespace Vltava.Web
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<SystemFolders>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory logger, IConfiguration configuration)
        {
            var sysFolders = app.ApplicationServices.GetService<SystemFolders>();

            var subscriptionListFile = sysFolders.SubscriptionsFile("tech.opml");
            if (!subscriptionListFile.HasValue)
                throw new ArgumentException($"{subscriptionListFile} does not exist");

            var opmlFile = sysFolders.TemplateFile("default.scriban-html");
            if (!opmlFile.HasValue)
                throw new ArgumentException($"{opmlFile} does not exist");

            //These are the four default services available at Configure
            app.Run(async context =>
            {
                try
                {
                    var syndication = Option.None<List<ComplexSyndication>>();
                    (await RenderPipeline.OpmlReadingAsync(subscriptionListFile.ValueOrFailure())).MatchSome
                        (opmlXml => RenderPipeline.OpmlParsing(opmlXml).MatchSome(
                            (opml => RenderPipeline.GetSyndicationUri(opml).MatchSome(
                                (async uris => (await RenderPipeline.ProcessSyndicationAsync(uris)).MatchSome(
                                    syndications => syndication = Option.Some(syndications)
                                ))
                            ))
                        )
                    );

                    var output = Option.None<string>();
                    (await RenderPipeline.TemplateReadingAsync(opmlFile.ValueOrFailure())).MatchSome
                        (template => RenderPipeline.Render((template, syndication.ValueOrFailure())).MatchSome(
                            o => output = Option.Some(o)
                        )
                    );

                    await output.Match(
                        some : async doc => {
                            context.Response.Headers.Add("Content-Type", "text/html");
                            await context.Response.WriteAsync(doc);
                        },
                        none: async () => {
                            context.Response.Headers.Add("Content-Type", "text/html");
                            await context.Response.WriteAsync("Error");
                        }
                    );
                }
                catch (Exception ex)
                {
                    await context.Response.WriteAsync($"Error {ex.Message}");
                }
            });
        }
    }
}