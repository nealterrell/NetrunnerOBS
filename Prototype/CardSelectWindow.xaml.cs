using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Xml.Linq;
using System.Drawing.Imaging;
using System.Net;
using System.Drawing;

using OBSElement = CLROBS.XElement;
using System.Timers;
using System.Windows.Threading;

namespace NetrunnerOBS {
	public class CardChangedEventArgs : EventArgs {
		public OBSElement Config { get; set; }
	}

	/// <summary>
	/// Interaction logic for CardWindow.xaml
	/// </summary>
	public partial class CardWindow : Window {
		private List<DispatcherTimer> mActiveTimers = new List<DispatcherTimer>();
		
		private OBSElement mConfig;

		public event EventHandler<CardChangedEventArgs> CardChanged;
		public CardWindow() {
			InitializeComponent();
		}


		public CardWindow(OBSElement config) {

			mConfig = config; 
			InitializeComponent();
			this.Loaded += CardWindow_Loaded;
			this.Closing += CardWindow_Closing;

		}

		void CardWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			e.Cancel = true;
			Hide();
		}

		void CardWindow_Loaded(object sender, RoutedEventArgs e) {
			mCardText.Focus();
			mCardText.SelectAll();
			mCardText.ItemSelected += mCardText_ItemSelected;
		}

		void mCardText_ItemSelected(object sender, Aviad.WPF.Controls.ItemSelectEventArgs e) {
			var model = this.FindResource("vm") as CardViewModel;
			if (e.SelectedItem != null)
				SubmitCardName(e.SelectedItem as XElement);
		}


		private void SubmitCardName(XElement card) {
			try {
				StopAutoHideTimers();
				string newFileName = NetrunnerPlugin.DownloadCardFile(card, false);
				if (newFileName != null) {
					mConfig.SetString("file", newFileName + "m.png");

					if (mAutoHideBox.IsChecked.Value) {
						StartAutoHideTimer();
					}
				}
				else {
					mConfig.SetString("file", "");
				}
				DispatchCardChanged();
			}
			catch (Exception ex) {
				MessageBox.Show(ex.Message);

			}
			mCardText.Text = card.Element("title").Value;
			mCardText.SelectAll();
			mCardText.HidePopup();
		}

		private void StartAutoHideTimer() {
			if (!object.ReferenceEquals(mConfig, null) && mHideUpDown != null && mHideUpDown.Value.HasValue) {
				var timer = new DispatcherTimer();
				timer.Interval = TimeSpan.FromSeconds((double)mHideUpDown.Value.Value);
				timer.Tick += (o, ea) => {
					mConfig.SetString("file", "");
					DispatchCardChanged();
					((DispatcherTimer)o).Stop();

					lock (mActiveTimers) {
						mActiveTimers.Remove((DispatcherTimer)o);
					}
				};
				lock (mActiveTimers) {
					mActiveTimers.Add(timer);
				}
				timer.Start();
			}
		}

		private void StopAutoHideTimers() {
			lock (mActiveTimers) {
				foreach (var timer in mActiveTimers) {
					timer.Stop();
					timer.IsEnabled = false;
				}
				mActiveTimers.Clear();
			}
		}

		private void DispatchCardChanged() {
			if (CardChanged != null)
				CardChanged(this, new CardChangedEventArgs() {
					Config = mConfig
				});
		}

		

		

		private void filenameText_KeyUp(object sender, KeyEventArgs e) {
			if (e.Key == Key.Enter) {
				var items = mCardText.ItemsSource;
				var enumerator = items.GetEnumerator();
				if (enumerator.MoveNext()) {
					var first = enumerator.Current as XElement;
					if (first != null)
						SubmitCardName(first);
				}

			}

		}

		private XElement GetCardWithPrefix() {
			var model = this.FindResource("vm") as CardViewModel;
			var x = (
				from card in model.CardDocument.Element("cards").Elements("card")
				where card.Element("title").Value.ToLower().StartsWith(mCardText.Text.ToLower())
				orderby card.Element("title").Value
				select card
			).FirstOrDefault();
			return x;
		}


		private void mSubmitBtn_Click(object sender, RoutedEventArgs e) {
			var x = GetCardWithPrefix();
			if (x != null)
				SubmitCardName(x);
		}


		private void mCardText_TextChanged(object sender, TextChangedEventArgs e) {
			//var model = this.FindResource("vm") as AnrCardViewModel;
			//var x = (
			//	 from card in model.CardDocument.Element("cards").Elements("card")
			//	 where card.Element("title").Value.ToLower() == mCardText.Text.ToLower()
			//	 select card
			// ).FirstOrDefault();
			//if (x != null)
			//	SubmitCardName(x);
		}

		private void mAutoHideBox_Checked(object sender, RoutedEventArgs e) {
			if (!mAutoHideBox.IsChecked.Value) {
				StopAutoHideTimers();
			}
			else if (mAutoHideBox.IsChecked.HasValue) {
				StartAutoHideTimer();
			}
		}

		private void mAdvancedBtn_Click(object sender, RoutedEventArgs e) {
			new NetrunnerAdvancedSettings(this.FindResource("vm") as CardViewModel).ShowDialog();
		}
	}
}
