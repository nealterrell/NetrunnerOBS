using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

namespace NetrunnerOBS {
	/// <summary>
	/// This model is bound to by the AutoCompleteTextBox, and serves as the
	/// list of cards to show to the user for a given query.
	/// General flow: user types in text box, which is bound to the QueryText
	/// property. Changes to QueryText causes a refresh to the QueryCollection
	/// property, which returns an IEnumerable of XElements for cards.
	/// </summary>
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
		
		/// <summary>
		/// Gets changed automatically by the auto complete text box. A change
		/// causes a refresh of the QueryCollection.
		/// </summary>
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
			
			// First select any cards that start with the search term.
			IEnumerable<XElement> startsWithResults =
				from card in mCardDocument.Element("cards").Elements("card")
				where card.Element("title").Value.ToLower().StartsWith(searchTerm)
				orderby card.Element("title").Value
				select card
			;

			if (searchTerm.Length >= 3) {
				// Then add any cards that contain the search term.
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
