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
using System.Diagnostics;

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

            var renderingTemplate = sysFolders.TemplateFile("default.scriban-html");
            if (!renderingTemplate.HasValue)
                throw new ArgumentException($"{renderingTemplate} does not exist");

            var watch = new Stopwatch();

            //These are the four default services available at Configure
            app.Run(async context =>
            {
                try
                {
                    watch.Start();

                    Option<string, Exception> opmlContent;

                    var remoteOpmlFile = context.Request.Query["opml"].FirstOrDefault();

                    if (string.IsNullOrEmpty(remoteOpmlFile))
                    {
                        var subscriptionListFile = sysFolders.SubscriptionsFile("tech.opml");
                        if (!subscriptionListFile.HasValue)
                            throw new ArgumentException($"{subscriptionListFile} does not exist");

                        opmlContent = await RenderPipeline.OpmlReadingAsync(subscriptionListFile.ValueOrFailure());
                    }
                    else
                    {
                        opmlContent = await RenderPipeline.OpmlReadingAsync(new Uri(remoteOpmlFile));
                    }

                    var uriList = opmlContent.Match(
                        some :  opmlXml =>  RenderPipeline.OpmlParsing(opmlXml).Match(
                            some :  opml => RenderPipeline.GetSyndicationUri(opml),
                            none: x => Option.None<List<Uri>, Exception>(x)
                        ),
                        none: x => Option.None<List<Uri>, Exception>(x)
                    );

                    var syndication = await uriList.Match(
                        some : uris => RenderPipeline.ProcessSyndicationAsync(uris),
                        none:  x => Task.FromResult(Option.None<List<ComplexSyndication>, Exception>(x))
                    );

                    //Read the template file and render the rss content
                    var output = await syndication.Match(
                        some : async syndicationList => {
                            var readTemplated = await RenderPipeline.TemplateReadingAsync(renderingTemplate.ValueOrFailure());
                            var match = readTemplated.Match(
                                some : template => 
                                {
                                    var props = new Dictionary<string, string>();
                                    watch.Stop();
                                    props["rendering_time"] = watch.ElapsedMilliseconds + " ms";

                                    return RenderPipeline.Render((template, syndicationList, props));
                                },
                                none:  x => Option.None<string, Exception>(x)
                            );
                            return match;
                        },
                        none: x => Task.FromResult(Option.None<string, Exception>(x))
                    );

                    await output.Match(
                        some : async doc => {
                            context.Response.Headers.Add("Content-Type", "text/html");
                            await context.Response.WriteAsync(doc);
                        },
                        none: async (ex) => {
                            context.Response.Headers.Add("Content-Type", "text/html");
                            await context.Response.WriteAsync($"{ex.Message}");
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