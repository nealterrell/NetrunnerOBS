using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Linq;

namespace NetrunnerOBS {
	/// <summary>
	/// Interaction logic for NetrunnerAdvancedSettings.xaml
	/// </summary>
	public partial class NetrunnerAdvancedSettings : Window {
		private const int GWL_STYLE = -16;
		private const int WS_SYSMENU = 0x80000;
		[DllImport("user32.dll", SetLastError = true)]
		private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
		[DllImport("user32.dll")]
		private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

		private CardViewModel mModel;

		public NetrunnerAdvancedSettings() {
			InitializeComponent();
		}

		public NetrunnerAdvancedSettings(CardViewModel model) : this() {
			mModel = model;
		}

		private void mCardListBtn_Click(object sender, RoutedEventArgs e) {
			if (MessageBox.Show("Do you want to refresh the list of cards by downloading a new " +
				"card list from Netrunnerdb? This may take a moment.",
				"Refresh card list?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {

				Window waitWindow = new Window() {
					SizeToContent = System.Windows.SizeToContent.WidthAndHeight,
					//Height = 300,
					//Width = 300,
					WindowState = System.Windows.WindowState.Normal,
					WindowStyle = System.Windows.WindowStyle.SingleBorderWindow,

					ResizeMode = System.Windows.ResizeMode.NoResize,
					ShowInTaskbar = false,
					WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner,

					//Background = new SolidColorBrush(Color.FromArgb(255, 127, 127, 0)),
					Title = "Downloading..."
				};

				var stack = new StackPanel() {
					Orientation = Orientation.Vertical,
					Margin = new Thickness(10),
				};


				var label = new Label() {
					Content = "Please wait...",
				};
				stack.Children.Add(label);

				waitWindow.Content = stack;
				waitWindow.Loaded += (send, ea) => {
					var hwnd = new WindowInteropHelper(waitWindow).Handle;
					SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);

					Task task = new Task(() => {
						try {
							WebClient client = new WebClient();
							var ret = client.DownloadString("http://netrunnerdb.com/api/cards");
							var download = new StringBuilder(ret).Replace("\"code\"", "\"@code\"")
								.Insert(0, "{'card':").Append("}");
							XDocument doc = JsonConvert.DeserializeXNode(download.ToString(), "cards");
							doc.Save(NetrunnerPlugin.NetrunnerDataFilePath);

							waitWindow.Dispatcher.BeginInvoke((Action)(() => {
								waitWindow.Close();
								MessageBox.Show("Please restart the program to load the new card list.", "Restart required",
									MessageBoxButton.OK, MessageBoxImage.Information);
							}));
							//waitWindow.Close();
						}
						catch (Exception ex) {
							MessageBox.Show(ex.ToString());
						}
					});
					task.Start();
				};

				waitWindow.ShowDialog();

			}
		}

		private void mAllImagesBtn_Click(object sender, RoutedEventArgs e) {
			if (MessageBox.Show("Do you want to pre-fetch all card images from Netrunnerdb? This will take a short while.",
							"Download all card images?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {

				Window waitWindow = new Window() {
					SizeToContent = System.Windows.SizeToContent.WidthAndHeight,
					//Height = 300,
					//Width = 300,
					WindowState = System.Windows.WindowState.Normal,
					WindowStyle = System.Windows.WindowStyle.SingleBorderWindow,

					ResizeMode = System.Windows.ResizeMode.NoResize,
					ShowInTaskbar = false,
					WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner,

					//Background = new SolidColorBrush(Color.FromArgb(255, 127, 127, 0)),
					Title = "Downloading..."
				};

				var stack = new StackPanel() {
					Orientation = Orientation.Vertical,
					Margin = new Thickness(10),
				};

				var progress = new ProgressBar() {
					Maximum = mModel.CardDocument.Element("cards").Elements().Count(),
					Minimum = 0,
					Width = 300,
					Height = 12,
				};

				var label = new Label() {
					Content = "Please wait...",
				};
				stack.Children.Add(progress);

				waitWindow.Content = stack;
				waitWindow.Loaded += (send, ea) => {
					try {
						var hwnd = new WindowInteropHelper(waitWindow).Handle;
						SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
						var downloadTasks = (
							from card in mModel.CardDocument.Element("cards").Elements()
							select Task.Run(() => {
								CLROBS.API.Instance.Log("Downloading {0}", card.Element("title").Value);
								try {
									NetrunnerPlugin.DownloadCardFile(card, true);
								}
								catch { }
								lock (progress) {
									progress.Dispatcher.Invoke((Action)(() => { progress.Value++; }));
								}
							})
						).ToArray();

						Task.Run(() => {
							Task.WaitAll(downloadTasks);
							waitWindow.Dispatcher.BeginInvoke((Action)(() => {
								waitWindow.Close();
							})) ;
						});
					}
					catch (Exception ex) {
						MessageBox.Show(ex.ToString());
					}
				};

				waitWindow.ShowDialog();

			}
		}
	}
}