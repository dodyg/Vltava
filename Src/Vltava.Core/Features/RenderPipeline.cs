
using System;
using System.Threading.Tasks;
using Optional;
using Optional.Unsafe;
using System.IO;
using Vltava.Core.Protocols;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Vltava.Core.Features
{
    public static class RenderPipeline
    {
        public static async Task<Option<string, Exception>> OpmlReadingAsync(string fileName)
        {
            try
            {
                var subscription = await File.ReadAllTextAsync(fileName);

                return Option.Some<string, Exception>(subscription);
            }
            catch (Exception ex)
            {
                return Option.None<string, Exception>(ex);
            }
        }

        public static async Task<Option<string, Exception>> OpmlReadingAsync(Uri url)
        {
            using(var client = new HttpClient())
            {
                try
                {
                    var data = await client.GetStringAsync(url);

                    return Option.Some<string, Exception>(data);
                }
                catch (Exception ex)
                {
                    var exceptionInfo = new Exception($"{url} {ex.Message}");
                    return Option.None<string, Exception>(exceptionInfo);
                }
            }
        }

        public static Option<Opml, Exception> OpmlParsing(string subscription)
        {
            try
            {
                var opml = Opml.Parse(subscription);

                if (opml.IsFalse)
                    throw opml.ExceptionObject;

                return Option.Some<Opml, Exception>(opml.Value);
            }
            catch (Exception ex)
            {
                return Option.None<Opml, Exception>(ex);
            }
        }

        public static Option<List<Uri>, Exception> GetSyndicationUri(Opml opml)
        {
            try
            {
                var outlines = opml.Find("type", "rss");
                var uriList = outlines.Where(o => o.Attributes.ContainsKey("url")).Select(o => new Uri(o["url"])).ToList();

                if (uriList.Count == 0)
                    uriList = outlines.Where(o => o.Attributes.ContainsKey("xmlUrl")).Select(o => new Uri(o["xmlUrl"])).ToList();

                if (uriList.Count > 10)
                    uriList = uriList.Take(10).ToList();

                return Option.Some<List<Uri>, Exception>(uriList);
            }
            catch (Exception ex)
            {
                return Option.None<List<Uri>, Exception>(ex);
            }
        }

        public static async Task<Option<List<ComplexSyndication>, Exception>> ProcessSyndicationAsync(List<Uri> uris)
        {
            try
            {
                var items = await SyndicationReader.Get(uris.ToArray());

                return Option.Some<List<ComplexSyndication>, Exception>(items);
            }
            catch (Exception ex)
            {
                return Option.None<List<ComplexSyndication>, Exception>(ex);
            }
        }

        public static async Task<Option<string, Exception>> TemplateReadingAsync(string fileName)
        {
            try
            {
                var subscription = await File.ReadAllTextAsync(fileName);

                return Option.Some<string, Exception>(subscription);
            }
            catch (Exception ex)
            {
                return Option.None<string, Exception>(ex);
            }
        }

        public static Option<string, Exception> Render((string template, List<ComplexSyndication> syndications, Dictionary<string, string> properties) input)
        {
            try
            {
                var r = new HtmlRender();
                var result = r.Render(input.template, input.syndications, input.properties);
                return Option.Some<string, Exception>(result);
            }
            catch (Exception ex)
            {
                return Option.None<string, Exception>(ex);
            }
        }
    }
}