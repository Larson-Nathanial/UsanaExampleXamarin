using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace USANA
{
	public class RSSFeeds
	{

		public string ConvertListToJson(List<Item> theList) 
		{

		}

		public void SaveText (string filename, List<Item> items) {



			var documentsPath = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
			var filePath = Path.Combine (documentsPath, filename);
			System.IO.File.WriteAllText (filePath, text);
		}
		public string LoadText (string filename) {
			var documentsPath = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
			var filePath = Path.Combine (documentsPath, filename);
			return System.IO.File.ReadAllText (filePath);
		}


		public RSSFeeds ()
		{
			BeginReadXMLStream("http://itunes.apple.com/us/rss/toppodcasts/limit=25/xml");
		}


	}
}

