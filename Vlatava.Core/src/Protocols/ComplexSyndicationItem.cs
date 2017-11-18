using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Rss;
using System.Linq;
using System;
using System.Xml;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Vlatava.Core.Protocols
{
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
        public static async Task<ComplexSyndicationItem> Get(Uri url)
        {
            var parser = new RssParser();
            var items = new List<ComplexSyndicationItem>();

            using (var xmlReader = XmlReader.Create(url.ToString(), new XmlReaderSettings { Async = true }))
            {
                var feedReader = new RssFeedReader(xmlReader);

                while (await feedReader.Read())
                {
                    switch (feedReader.ElementType)
                    {
                        case SyndicationElementType.Item:
                            //ISyndicationContent is a raw representation of the feed
                            ISyndicationContent content = await feedReader.ReadContent();

                            ISyndicationItem item = parser.CreateItem(content);
                            ISyndicationContent outline = content.Fields.FirstOrDefault(f => f.Name == "source:outline");

                            items.Add(new ComplexSyndicationItem(item, outline));
                            break;
                        default:
                            break;
                    }
                }
            }

            return items;
        }
    }
}