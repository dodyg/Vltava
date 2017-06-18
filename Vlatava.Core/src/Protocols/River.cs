using System.Collections.Generic;

namespace Vlatava.Core.Protocols
{
    public class FeedsCollection
    {
        public List<RiverSite> UpdatedFeed { get; set; }
    }

    public class RiverSite
    {
        public string FeedUrl { get; set; }
        public string WebsiteUrl { get; set; }
        public string FeedTitle { get; set; }
        public string FeedDescription { get; set; }
        public string WhenLastUpdate { get; set; }
        public List<RiverItem> Item { get; set; }
    }

    public class RiverItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string PermaLink { get; set; }
        public string PubDate { get; set; }
        public string Link { get; set; }
        public string Comments { get; set; }
        public List<RiverImage> Thumbnail { get; set; }
    }

    public class RiverImage
    {
        public string Url { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
    }
}