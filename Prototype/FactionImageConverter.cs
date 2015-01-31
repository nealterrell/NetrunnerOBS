using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace NetrunnerOBS {
	class FactionImageConverter : IValueConverter {
		static Dictionary<string, BitmapImage> mImages = new Dictionary<string, BitmapImage>();

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			
			try {
				string faction = value as string;
				if (mImages.ContainsKey(faction))
					return mImages[faction];

				var assembly = Assembly.GetExecutingAssembly();
				using (Stream stream = assembly.GetManifestResourceStream("NetrunnerOBS.Properties.faction_" + faction + ".png")) {
					var image = new BitmapImage();
					image.BeginInit();
					image.StreamSource = stream;
					image.EndInit();
					mImages[faction] = image;
					return image;
				}
			}
			catch (Exception e) {
				MessageBox.Show(e.ToString());
				return null;
			}
		}


		public object ConvertBack(object value, Type targetType,
				object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
