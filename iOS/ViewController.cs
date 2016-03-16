using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Collections.Generic;
using UIKit;
using Foundation;
using Rivets;

namespace USANA.iOS
{
	public partial class ViewController : UIViewController
	{
		UITableView table;
		public static List<Item> tableItems { get; set; }

		public ViewController (IntPtr handle) : base (handle)
		{		
		}
			
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			table = new UITableView(View.Bounds); // defaults to Plain style
			table.Source = new TableSource(tableItems);
			Add (table);

			this.NavigationItem.SetRightBarButtonItem(
				new UIBarButtonItem(UIBarButtonSystemItem.Refresh, (sender,args) => {
					this.whichFeed = 0;
					BuildItemList ("http://itunes.apple.com/us/rss/toppodcasts/limit=25/xml");
				})
				, true);

			// Code to start the Xamarin Test Cloud Agent
			#if ENABLE_TEST_CLOUD
			Xamarin.Calabash.Start ();
			#endif



			var documentsPath = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
			var filePath = Path.Combine (documentsPath, "json.txt");
			string y = File.Exists(filePath) ? LoadText (filePath) : null;

			if (y == null) {
				this.whichFeed = 0;
				BuildItemList ("http://itunes.apple.com/us/rss/toppodcasts/limit=25/xml");
			} else {
				ConvertList cltj = new ConvertList ();
				aList = cltj.ConvertJTo (y);

				tableItems = aList;
				table.Source = new TableSource(tableItems);
				table.ReloadData();
			}

			var documentsPath2 = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
			var filePath2 = Path.Combine (documentsPath2, "update_time.txt");
			string lupdate = File.Exists(filePath2) ? LoadText (filePath2) : "";
			UILabel label = new UILabel();
			label.Frame = new CoreGraphics.CGRect(0,0,120,44);
			label.Font = UIKit.UIFont.FromName("Helvetica",(nfloat)12.0);
			label.TextAlignment = UIKit.UITextAlignment.Center;
			label.Text = lupdate;

			this.NavigationItem.TitleView = label;
		}

		public override void DidReceiveMemoryWarning ()
		{		
			base.DidReceiveMemoryWarning ();		
			// Release any cached data, images, etc that aren't in use.		
		}

		public class TableSource : UITableViewSource {

			List<Item> TableItems;
			string CellIdentifier = "TableCell";

			public TableSource (List<Item> items)
			{
				TableItems = items;
			}

			public override nint RowsInSection (UITableView tableview, nint section)
			{
				if (TableItems == null) {
					return 0;
				}else {
					return TableItems.Count;
				}

			}

			public override nfloat GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
			{
				return (nfloat)140.0;
			}

			public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
			{
				UITableViewCell cell = tableView.DequeueReusableCell (CellIdentifier);

				Item item = TableItems[indexPath.Row];

				//---- if there are no cells to reuse, create a new one
				if (cell == null)
				{ cell = new UITableViewCell (UITableViewCellStyle.Subtitle, CellIdentifier); }

				cell.TextLabel.Text = item.RssTitle;
				cell.DetailTextLabel.Lines = 7;
				cell.DetailTextLabel.LineBreakMode = UILineBreakMode.WordWrap;
				cell.DetailTextLabel.Text = item.RssSummary;
				cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;

				string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
				string localFilename = item.RssTitle + ".png";
				string localPath = Path.Combine (documentsPath, localFilename);

				cell.ImageView.Image = UIImage.FromFile (localPath);

				if (cell.ImageView.Image == null) {
					var client = new WebClient ();
					client.DownloadDataCompleted += (s, e) => {
						var bytes = e.Result; // get the downloaded data

						File.WriteAllBytes (localPath, bytes); // writes to local storage
						InvokeOnMainThread ( () => {

							NSIndexPath[] rowsToReload = new NSIndexPath[] {
								NSIndexPath.FromRowSection(indexPath.Row, indexPath.Section) // points to second row in the first section of the model
							};

							cell.ImageView.Image = UIImage.FromFile (localPath);
							tableView.ReloadRows(rowsToReload, UITableViewRowAnimation.Automatic);
						});
					};

					client.DownloadDataAsync(new Uri(item.RssImgUrl));
				}

				return cell;
			}

			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				Item item = TableItems[indexPath.Row];
				Console.WriteLine (item.RssAppStoreUrl);
				UIApplication.SharedApplication.OpenUrl (new Uri (item.RssAppStoreUrl));
			}
		}


		public static List<Item> ItemList { get; set; }
		int whichFeed;
		public static List<Item> aList { get; set; }

		public void BuildItemList(string url)
		{
			List<Item> returnValue = new List<Item> ();

			try {
				
				WebRequest webRequest = WebRequest.Create(url);
				WebResponse webResponse = webRequest.GetResponse();
				Stream stream = webResponse.GetResponseStream();
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(stream);
				XmlNodeList itemNodes = xmlDocument.ChildNodes[1].ChildNodes;
			
				for (int i = 0; i < itemNodes.Count; i++)
				{
					if (itemNodes[i].Name == "entry") {
						XmlNodeList subNodes = itemNodes[i].ChildNodes;
						Item feedItem = new Item();

						foreach (XmlNode n in subNodes) {
							if (this.whichFeed == 0) {
								feedItem.RssFeed = "Podcast";
							}

							if (n.Name == "title") {
								feedItem.RssTitle = n.InnerText;
							}else if (n.Name == "summary") {
								feedItem.RssSummary = n.InnerText;
							}else if (n.Name == "id") {
								feedItem.RssAppStoreUrl = n.InnerText;
							}else if (n.Name == "im:image") {
								feedItem.RssImgUrl = n.InnerText;
							}

						}
						returnValue.Add(
							feedItem
						);

					}

				}
				
			} catch (Exception) {
				UIAlertView _error = new UIAlertView ("Error", "We couldn't load your RSS feeds :(", null, "Ok", null);

				_error.Show ();
			}


			if (this.whichFeed == 0) {
				ItemList = returnValue;
				this.whichFeed = 1;
				BuildItemList ("http://ax.itunes.apple.com/WebObjects/MZStoreServices.woa/ws/RSS/topfreeapplications/limit=25/xml");
			} else {
				foreach (Item iem in returnValue) {
					ItemList.Add (iem);
				}

				ConvertList cltj = new ConvertList ();

				string j = cltj.ConvertLToJ (ItemList);

				SaveText ("json.txt", j);

				tableItems = ItemList;
				table.Source = new TableSource(tableItems);
				table.ReloadData();

				DateTime now = DateTime.Now.ToLocalTime();
				if (DateTime.Now.IsDaylightSavingTime () == true) {
					now = now.AddHours (1);
				}
				string lupdate = (string.Format ("Last Updated: {0}", now));
				SaveText ("update_time.txt", lupdate);

				UILabel label = new UILabel();
				label.Frame = new CoreGraphics.CGRect(0,0,120,44);
				label.Font = UIKit.UIFont.FromName("Helvetica",(nfloat)12.0);
				label.TextAlignment = UIKit.UITextAlignment.Center;
				label.Text = lupdate;

				this.NavigationItem.TitleView = label;

			}

		}

		public void SaveText (string filename, string text) {
			var documentsPath = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
			var filePath = Path.Combine (documentsPath, filename);
			System.IO.File.WriteAllText (filePath, text);
		}

		public string LoadText (string filename) {
			var documentsPath = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
			var filePath = Path.Combine (documentsPath, filename);
			return System.IO.File.ReadAllText (filePath);
		}

	}

}
