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

            var opmlReading = new TransformBlock<string, string>(async fileName =>
                              {
                                  System.Console.WriteLine("OPML READING");

                                  var subscriptionListFile = sysFolders.SubscriptionsFile(fileName);
                                  if (!subscriptionListFile.HasValue)
                                      throw new ArgumentException($"{subscriptionListFile} does not exist");

                                  var subscription = await File.ReadAllTextAsync(subscriptionListFile.ValueOrFailure());

                                  return subscription;
                              });

            var opmlParsing = new TransformBlock<string, Opml>(subscription =>
            {
                var opml = Opml.Parse(subscription);

                if (opml.IsFalse)
                    throw opml.ExceptionObject;

                return opml.Value;
            });

            var syndicationSourceUrls = new TransformBlock<Opml, List<Uri>>(opml =>
            {
                var outlines = opml.Find("type", "rss");
                return outlines.Where(o => o.Attributes.ContainsKey("url")).Select(o => new Uri(o["url"])).ToList();
            });

            var syndications = new TransformBlock<List<Uri>, List<ComplexSyndication>>(async uris =>
            {
                var items = await SyndicationReader.Get(uris.ToArray());
                return items;
            });

            SyndicationReader.SyndicationItemStream.Subscribe(x => Console.WriteLine(x.Item.Title));

            var template = new TransformBlock<string, string>(async filename =>
            {
                System.Console.WriteLine("Template Processing");
                var templateFile = sysFolders.TemplateFile(filename);
                if (!templateFile.HasValue)
                    throw new ArgumentException($"{templateFile} does not exist");

                var tmplt = await File.ReadAllTextAsync(templateFile.ValueOrFailure());
                return tmplt;
            });

            var output = new TransformBlock<Tuple<string, List<ComplexSyndication>>, string>(input =>
           {
               System.Console.WriteLine("Rendering");
               var r = new HtmlRender();
               return r.Render(input.Item1, input.Item2);
           });

            opmlReading.LinkTo(opmlParsing);
            opmlParsing.LinkTo(syndicationSourceUrls);
            syndicationSourceUrls.LinkTo(syndications);

            var join = new JoinBlock<string, List<ComplexSyndication>>();
            template.LinkTo(join.Target1);
            syndications.LinkTo(join.Target2);
            join.LinkTo(output);

            opmlReading.Completion.ContinueWith(t => 
                opmlParsing.Complete());

            opmlParsing.Completion.ContinueWith(t => 
                syndicationSourceUrls.Complete());

            syndicationSourceUrls.Completion.ContinueWith(t =>
                syndications.Complete());

            Task.WhenAll(syndications.Completion, template.Completion)
                .ContinueWith(t => join.Complete());

            join.Completion.ContinueWith(t => 
                output.Complete());

            //These are the four default services available at Configure
            app.Run(async context =>
            {
                try
                {
                    var render = new ActionBlock<string>(async input =>
                    {
                        context.Response.Headers.Add("Content-Type", "text/html");
                        await context.Response.WriteAsync(input);
                    });

                    output.LinkTo(render);

                    await output.Completion.ContinueWith(t => render.Complete());

                    template.Post("default.scriban-html");
                    opmlReading.Post("tech.opml");

                    await render.Completion;
                }
                catch (Exception ex)
                {
                    await context.Response.WriteAsync($"Error {ex.Message}");
                }
            });
        }
    }
}