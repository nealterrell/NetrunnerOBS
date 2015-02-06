using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLROBS;
using System.Windows;

namespace NetrunnerOBS {
	/// <summary>
	/// Factory registered with the OBS API so OBS can create our source object
	/// when needed.
	/// </summary>
	public class NetrunnerCardImageSourceFactory : AbstractImageSourceFactory {
		// Kind of hacky: a singleton CardWindow connects to all sources.
		static CardWindow mConfiguration;
		static List<NetrunnerCardImageSource> mSources = new List<NetrunnerCardImageSource>();

		public NetrunnerCardImageSourceFactory() {
			ClassName = "CardImageSourceClass";
			DisplayName = "Netrunner Card";
		}

		public override ImageSource Create(XElement data) {
			// Clear the file data, so the stream always starts with a blank card.
			data.SetString("file", "");
			
			var source = new NetrunnerCardImageSource(data);
			mSources.Add(source);
			return source;
		}

		public override bool ShowConfiguration(XElement data) {
			try {
				if (mConfiguration == null) {
					mConfiguration = new CardWindow(data);
					mConfiguration.CardChanged += mConfiguration_CardChanged;
				}
				mConfiguration.Show();
			}
			catch (Exception e) {
				MessageBox.Show(e.ToString());
			}
			return true;
		}

		void mConfiguration_CardChanged(object sender, CardChangedEventArgs e) {
			UpdateSourceSettings();
		}

		private static void UpdateSourceSettings() {
			foreach (var source in mSources)
				source.UpdateSettings();
		}
	}
}
