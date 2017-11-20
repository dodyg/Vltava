using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Rss;
using System.Linq;
using System;
using System.Xml;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.IO;
using System.Text;

namespace Vltava.Core.Protocols
{
    public class ComplexSyndication
    {
        public List<ComplexSyndicationItem> Items { get; set; } = new List<ComplexSyndicationItem>();
    }

    public class ComplexSyndicationItem
    {
        public ISyndicationItem Item { get; set; }

        public Outline Outline { get; set; }

        public ComplexSyndicationItem(ISyndicationItem item, ISyndicationContent outline = null)
        {
            Item = item;
            if (outline != null)
            {
                Outline = new Outline
                {
                    Attributes = outline.Attributes.ToDictionary(x => x.Name, x => x.Value)
                };
            }
        }
    }

    public static class SyndicationReader
    {
        public static async Task<List<ComplexSyndication>> Get(params Uri[] url)
        {
            var parser = new RssParser();

            var toBeProcessed = new List<Task<HttpResponseMessage>>();

            var httpClient = HttpClientFactory.Get();

            foreach (var u in url)
            {
                var t = httpClient.GetAsync(u);
                toBeProcessed.Add(t);
            }

            Task.WaitAll(toBeProcessed.ToArray());

            var syndications = new List<ComplexSyndication>();

            foreach (var result in toBeProcessed)
            {
                var res = result.Result;
                var resultContent = await res.Content.ReadAsStringAsync();

                using (var xmlReader = XmlReader.Create(new MemoryStream(Encoding.UTF8.GetBytes(resultContent))))
                {
                    var feedReader = new RssFeedReader(xmlReader);

                    var syndication = new ComplexSyndication();

                    while (await feedReader.Read())
                    {

                        switch (feedReader.ElementType)
                        {
                            case SyndicationElementType.Item:
                                //ISyndicationContent is a raw representation of the feed
                                ISyndicationContent content = await feedReader.ReadContent();

                                ISyndicationItem item = parser.CreateItem(content);
                                ISyndicationContent outline = content.Fields.FirstOrDefault(f => f.Name == "source:outline");

                                syndication.Items.Add(new ComplexSyndicationItem(item, outline));
                                break;
                            default:
                                break;
                        }
                    }

                    syndications.Add(syndication);
                }
            }

            return syndications;
        }
    }
}