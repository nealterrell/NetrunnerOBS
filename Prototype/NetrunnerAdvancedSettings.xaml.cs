using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Xml.Linq;

namespace NetrunnerOBS {
	/// <summary>
	/// Shows advanced options for Netrunner plugin.
	/// </summary>
	public partial class NetrunnerAdvancedSettings : Window {
		// For hiding the "close" and upper-left "contol" window icons.
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

		public NetrunnerAdvancedSettings(CardViewModel model)
			: this() {
			mModel = model;
		}

		private void mCardListBtn_Click(object sender, RoutedEventArgs e) {
			if (MessageBox.Show("Do you want to refresh the list of cards by downloading a new " +
				"card list from Netrunnerdb? This may take a moment.",
				"Refresh card list?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {

				Window waitWindow = new Window() {
					SizeToContent = System.Windows.SizeToContent.WidthAndHeight,
					WindowState = System.Windows.WindowState.Normal,
					WindowStyle = System.Windows.WindowStyle.SingleBorderWindow,

					ResizeMode = System.Windows.ResizeMode.NoResize,
					ShowInTaskbar = false,
					WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner,

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

					// Async download the new card data so the window thread doesn't block.
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
						}
						catch (Exception ex) {
							// TODO: better error handling
							MessageBox.Show(ex.ToString());
							waitWindow.Close();
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
					WindowState = System.Windows.WindowState.Normal,
					WindowStyle = System.Windows.WindowStyle.SingleBorderWindow,

					ResizeMode = System.Windows.ResizeMode.NoResize,
					ShowInTaskbar = false,
					WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner,

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

				stack.Children.Add(progress);

				waitWindow.Content = stack;
				waitWindow.Loaded += (send, ea) => {
					try {
						var hwnd = new WindowInteropHelper(waitWindow).Handle;
						SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);

						// Create an async Task for each card
						var downloadTasks = (
							from card in mModel.CardDocument.Element("cards").Elements()
							select Task.Run(() => {
								CLROBS.API.Instance.Log("Downloading {0}", card.Element("title").Value);
								try {
									NetrunnerPlugin.DownloadCardFile(card, true);
								}
								catch {
									// TODO: better error handling
								}
								lock (progress) {
									progress.Dispatcher.Invoke((Action)(() => { progress.Value++; }));
								}
							})
						).ToArray();

						// Once all the tasks have run, close the window.
						// This is another Task so the window thread doesn't block.
						Task.Run(() => {
							Task.WaitAll(downloadTasks);
							waitWindow.Dispatcher.BeginInvoke((Action)(() => {
								waitWindow.Close();
							}));
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