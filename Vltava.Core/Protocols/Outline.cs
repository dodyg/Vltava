using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vltava.Core.Protocols
{
    public class Outline
    {
        public Dictionary<string, string> Attributes { get; set; }

        public string this[string key] => Attributes.ContainsKey(key) ? Attributes[key] : null;

        public string Text => this["text"];

        public Outline()
        {
            Attributes = new Dictionary<string, string>();
            Outlines = new List<Outline>();
        }

        public List<Outline> Outlines { get; private set; }
    }
}