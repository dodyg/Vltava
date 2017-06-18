using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vlatava.Core.Protocols
{
    public class Outline
    {
        public Dictionary<string, string> Attributes { get; private set; }
        public Outline()
        {
            Attributes = new Dictionary<string, string>();
            Outlines = new List<Outline>();
        }

        public List<Outline> Outlines { get; private set; }
    }
}