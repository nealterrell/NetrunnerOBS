using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using System.Xml.Linq;

using OBSElement = CLROBS.XElement;
using System.Windows.Threading;

namespace NetrunnerOBS {
	public class CardChangedEventArgs : EventArgs {
		public OBSElement Config { get; set; }
	}

	/// <summary>
	/// Card selection window for Netrunner plugin. 
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
			mConfig.SetString("file", "");
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

		private void mCardText_KeyUp(object sender, KeyEventArgs e) {
			// Shows the first card in the auto-complete list when Enter is pressed
			if (e.Key == Key.Enter && mCardText.Text.Length > 0) {
				// ItemsSource is not LINQ compatible, have to manually use 
				// an enumerator.
				var items = mCardText.ItemsSource;
				var enumerator = items.GetEnumerator();
				if (enumerator.MoveNext()) {
					var first = enumerator.Current as XElement;
					if (first != null)
						SubmitCardName(first);
				}
			}
		}

		/// <summary>
		/// Returns the first card (ordered by title) that starts with the given
		/// prefix string.
		/// </summary>
		private XElement GetCardWithPrefix() {
			var model = this.FindResource("vm") as CardViewModel;
			return (
				from card in model.CardDocument.Element("cards").Elements("card")
				where card.Element("title").Value.ToLower().StartsWith(mCardText.Text.ToLower())
				orderby card.Element("title").Value
				select card
			).FirstOrDefault();
		}

		private void mSubmitBtn_Click(object sender, RoutedEventArgs e) {
			var card = GetCardWithPrefix();
			if (card != null && mCardText.Text.Length > 0)
				SubmitCardName(card);
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
