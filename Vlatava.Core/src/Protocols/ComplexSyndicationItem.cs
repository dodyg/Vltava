using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Rss;
using System.Linq;
using System;

namespace Vlatava.Core.Protocols
{
    public class ComplexSyndicationItem 
    {
        public ISyndicationItem Item {get; set;}

        public Outline Outline {get;set;}

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
}