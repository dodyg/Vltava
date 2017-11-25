using System;
using System.Collections.Generic;
using Scriban;
using Vltava.Core.Protocols;

namespace Vltava.Core.Features
{
    public class HtmlRender
    {
        public string Render (string template, List<ComplexSyndication> syndications)
        {
            if (string.IsNullOrWhiteSpace(template))
                throw new ArgumentNullException($"{nameof(template)}");
                
            var tmp = Template.Parse(template);
            
            var result = tmp.Render(new { syndications = syndications });

            return result;
        }
    }
}