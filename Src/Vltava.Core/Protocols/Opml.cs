using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Vltava.Core.Framework;

namespace Vltava.Core.Protocols
{
    public class Opml
    {
        public string Title { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
        public string OwnerName { get; set; }
        public string OwnerEmail { get; set; }
        public Uri OwnerId { get; set; }
        public Uri Docs { get; set; }
        public string ExpansionState { get; set; }
        public int? VertScrollState { get; set; }
        public int? WindowTop { get; set; }
        public int? WindowLeft { get; set; }
        public int? WindowBottom { get; set; }
        public int? WindowRight { get; set; }
        public List<Outline> Outlines { get; private set; }

        public Opml()
        {
            Outlines = new List<Outline>();
        }

        public static Result<Opml> Parse(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
                throw new ArgumentNullException($"{nameof(xml)} is null");

            var opml = new Opml();
            var res = opml.LoadFromXML(xml);

            if (res.IsTrue)
                return Result<Opml>.True(opml);
            else
                return Result<Opml>.False(res.ExceptionObject);
        }

        public Result<None> LoadFromXML(string xml)
        {
            try
            {
                var elements = XElement.Parse(xml);
                var heads = elements.Element("head").Descendants();

                string selectString(string filter) =>
                    heads.Where(x => x.Name == filter).Select(x => x.Value).FirstOrDefault();

                int? selectInt(string filter) =>
                    heads.Where(x => x.Name == filter).Select(x => Convert.ToInt32(x.Value)).FirstOrDefault();

                DateTime? selectDate(string filter)  => 
                    heads.Where(x => x.Name == filter).Select(x => Convert.ToDateTime(x.Value)).FirstOrDefault();

                Uri selectUri(string filter) =>
                    heads.Where(x => x.Name == filter).Select(x => new Uri(x.Value)).FirstOrDefault();

                Title = selectString("title");
                DateCreated = selectDate("dateCreated");
                DateModified = selectDate("dateModified");
                OwnerName = selectString("ownerName");
                OwnerEmail = selectString("ownerEmail");
                OwnerId = selectUri("ownerId");
                Docs = selectUri("docs");
                ExpansionState = selectString("expansionState");
                VertScrollState = selectInt("vertScrollState");
                WindowTop = selectInt("windowTop");
                WindowLeft = selectInt("windowLeft");
                WindowBottom = selectInt("windowBottom");
                WindowRight = selectInt("windowRight");

                var bodies = elements.Element("body").Elements();
                //todo: make it recursive
                foreach (var b in bodies)
                {
                    var o = new Outline();
                    Outlines.Add(o);
                    TraverseBody(b, o);
                }

                return None.True(); //operation successful
            }
            catch (Exception ex)
            {
                return None.False(ex);
            }
        }

        public List<Outline> Find(string attributeName, string attributeValue)
        {
            if (string.IsNullOrWhiteSpace(attributeName))
                throw new ArgumentNullException($"{nameof(attributeName)}");
                
            var list = new List<Outline>();

            IEnumerable<Outline> Find2(Outline o) 
            {
                if (o.Attributes.ContainsKey(attributeName) && o.Attributes[attributeName] == attributeValue)
                    yield return o;

                foreach(var oo in o.Outlines)
                    foreach(var ooo in Find2(oo))
                        yield return ooo;
            }

            foreach(var o in Outlines)
                foreach(var m in Find2(o))
                    list.Add(m);

            return list;                
        }

        private void TraverseBody(XElement outline, Outline ot)
        {
            if (outline != null)
            {
                foreach (var att in outline.Attributes())
                {
                    ot.Attributes[att.Name.ToString()] = att.Value;
                }
                foreach (var x in outline.Elements())
                {
                    var o = new Outline();
                    ot.Outlines.Add(o);
                    TraverseBody(x, o);
                }
            }
        }

        public XElement ToXML()
        {
            var root = new XElement("opml",
                new XAttribute("version", "2.0"),
                    new XElement("head",
                        new XElement("title", this.Title),
                        (this.DateCreated.HasValue) ? new XElement("dateCreated", this.DateCreated.Value.ToString("R")) : null,
                        (this.DateModified.HasValue) ? new XElement("dateModified", this.DateModified.Value.ToString("R")) : null,
                        (!string.IsNullOrWhiteSpace(this.OwnerName)) ? new XElement("ownerName", this.OwnerName) : null,
                        (!string.IsNullOrWhiteSpace(this.OwnerEmail)) ? new XElement("ownerEmail", this.OwnerEmail) : null
                        ));

            var body = new XElement("body");
            foreach (var x in this.Outlines)
            {
                XElement newOutline = new XElement("outline");
                AddRecursiveChild(newOutline, x);
                body.Add(newOutline);
            }

            root.Add(body);

            return root;
        }

        private void AddRecursiveChild(XElement element, Outline o)
        {

            element.Add(from y in o.Attributes
                        select new XAttribute(y.Key, y.Value));

            foreach (var oo in o.Outlines)
            {
                XElement newOutline = new XElement("outline");

                element.Add(newOutline);
                AddRecursiveChild(newOutline, oo);
            }
        }
    }
}