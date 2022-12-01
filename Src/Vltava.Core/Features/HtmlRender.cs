using System;
using System.Collections.Generic;
using Scriban;
using Vltava.Core.Protocols;
using System.Linq;

namespace Vltava.Core.Features
{
    public class HtmlRender
    {
        public string Render (string template, List<ComplexSyndication> syndications, Dictionary<string, string> props)
        {
            if (string.IsNullOrWhiteSpace(template))
                throw new ArgumentNullException($"{nameof(template)}");
                
            var tmp = Template.Parse(template);
            
            var result = tmp.Render(new { syndications = syndications.SelectMany(x => x.Items).OrderByDescending(x => x.Item.Published), properties = props });

            return result;
        }
    }
}