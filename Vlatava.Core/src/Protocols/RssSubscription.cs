using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vlatava.Core.Protocols
{
    public class RssSubscriptionItem
    {
        public string Text { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Uri HtmlUri { get; set; }
        public Uri XmlUri { get; set; }
    }

    /// <summary>
    /// Hold information of an RSS subscription list written in OPML format
    /// </summary>
    public class RssSubscription
    {
        public string Title { get; set; }
        public string OwnerName { get; set; }
        public string OwnerEmail { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
        public List<RssSubscriptionItem> Items { get; set; }

        public RssSubscription()
        {
            Items = new List<RssSubscriptionItem>();
            ParsingErrors = new List<string>();
        }

        /// <summary>
        /// List errors in parsing opml attributes
        /// </summary>
        public List<string> ParsingErrors
        {
            get;
            set;
        }

        public RssSubscription(Opml opml): this()
        {
            Title = opml.Title;
            DateCreated = opml.DateCreated;
            DateModified = opml.DateModified;

            var line = 0;
            foreach (var x in opml.Outlines)
            {
                line++;
                var item = new RssSubscriptionItem();
                foreach (var y in x.Attributes)
                {
                    try
                    {
                        if (y.Key == "text")
                            item.Text = y.Value;
                        else if (y.Key == "description")
                            item.Description = y.Value;
                        else if (y.Key == "title")
                            item.Title = y.Value;
                        else if (y.Key == "name")
                            item.Name = y.Value;
                        else if (y.Key == "htmlUrl" && !string.IsNullOrWhiteSpace(y.Value))
                            item.HtmlUri = new Uri(y.Value);
                        else if (y.Key == "xmlUrl" && !string.IsNullOrWhiteSpace(y.Value))
                            item.XmlUri = new Uri(y.Value);
                    }
                    catch (Exception ex)
                    {
                        ParsingErrors.Add("Error at line " + line + " in processing attribute " 
                            + y.Key + " with value " + y.Value + " " +  ex.Message);
                    }
                }

                Items.Add(item);
            }
        }

        public Opml ToOpml()
        {
            var opml = new Opml
            {
                Title = Title,
                OwnerName = OwnerName,
                OwnerEmail = OwnerEmail,
                DateCreated = DateCreated,
                DateModified = DateModified
            };

            foreach (var i in Items.Select(
                x =>
                {
                    var item = new Outline();
                    item.Attributes["text"] = x.Text;
                    item.Attributes["type"] = "rss";
                    if (!string.IsNullOrWhiteSpace(x.Name))
                        item.Attributes["name"] = x.Name;
                    if (!string.IsNullOrWhiteSpace(x.Description))
                        item.Attributes["description"] = x.Description;
                    if (x.HtmlUri != null)
                        item.Attributes["htmlUrl"] = x.HtmlUri.ToString();
                    if (x.XmlUri != null)
                        item.Attributes["xmlUrl"] = x.XmlUri.ToString();

                    return item;
                }))
            {
                opml.Outlines.Add(i);
            }

            return opml;
        }
    }
}