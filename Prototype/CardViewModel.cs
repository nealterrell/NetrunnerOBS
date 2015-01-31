using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections;
using System.Web;
using System.Xml;
using System.Net;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using System.Xml.Linq;
using System.Windows;

namespace NetrunnerOBS {
	public class CardViewModel : INotifyPropertyChanged {
		private XDocument mCardDocument;
		private List<string> mWaitMessage = new List<string>() { "Please Wait..." };
		private string mQueryText;
		
		public CardViewModel() {
			LoadCardsFromDocument(NetrunnerPlugin.NetrunnerDataFilePath);
		}

		private void LoadCardsFromDocument(string path) {
			try {
				mCardDocument = XDocument.Load(path);
			}
			catch (Exception e) {
				MessageBox.Show(e.ToString());
			}
		}

		public XDocument CardDocument {
			get { return mCardDocument; }
		}

		public IEnumerable WaitMessage {
			get { return mWaitMessage; }
		}
		
		public string QueryText {
			get { return mQueryText; }
			set {
				if (mQueryText != value) {
					mQueryText = value;
					OnPropertyChanged("QueryText");
					OnPropertyChanged("QueryCollection");
				}
			}
		}

		public IEnumerable QueryCollection {
			get {
				return SelectCardsForQuery(QueryText);
			}
		}

		private IEnumerable SelectCardsForQuery(string searchTerm) {
			searchTerm = searchTerm.ToLower();
			IEnumerable<XElement> startsWithResults =
				from card in mCardDocument.Element("cards").Elements("card")
				where card.Element("title").Value.ToLower().StartsWith(searchTerm)
				orderby card.Element("title").Value
				select card
			;

			if (searchTerm.Length >= 3) {
				var containsResults =
					from card in mCardDocument.Element("cards").Elements("card")
					where card.Element("title").Value.ToLower().Contains(searchTerm)
					orderby card.Element("title").Value
					select card
				;
				startsWithResults = startsWithResults.Union(containsResults);
			}
			return startsWithResults;
		}


		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		protected void OnPropertyChanged(string prop) {
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(prop));
		}
	}
}
