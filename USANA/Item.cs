using System;

namespace USANA
{
	public class Item
	{
		public string RssFeed { get; set; }
		public string RssTitle { get; set; }
		public string RssSummary { get; set; }
		public string RssAppStoreUrl { get; set; }
		public string RssImgUrl { get; set; }
		public Item(){}
		public Item(string rssFeed, string rssTitle, string rssSummary, string rssAppStoreUrl, string rssImgUrl)
		{
			RssFeed = rssFeed;
			RssTitle = rssTitle;
			RssSummary = rssSummary;
			RssAppStoreUrl = rssAppStoreUrl;
			RssImgUrl = rssImgUrl;
		}


	}
}

