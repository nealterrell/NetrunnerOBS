using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLROBS;
using System.IO;
using System.Windows;
using System.Net;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Windows.Media;
using System.Windows.Media.Imaging;
//using System.Windows.Shapes;
using System.Drawing.Imaging;
using System.Net;
using System.Drawing;
using System.Reflection;


namespace NetrunnerOBS {
	public class NetrunnerPlugin : AbstractPlugin {
		public static string PluginDirectoryPath { get; private set; }

		public static string NetrunnerDataFilePath { get; private set; }

		public NetrunnerPlugin() {
			AppDomain.CurrentDomain.AssemblyResolve += (sender, ea) => {
				var resName = "NetrunnerOBS.Properties." + ea.Name.Split(',')[0] + ".dll";
				 using (Stream input = Assembly.GetExecutingAssembly().GetManifestResourceStream(resName)) {
                    return input != null
                         ? Assembly.Load(StreamToBytes(input))
                         : null;
                }
			};
			Name = "Card image plugin";
			PluginDirectoryPath = Path.Combine(API.Instance.GetPluginDataPath(), "Netrunner");
			NetrunnerDataFilePath =
				Path.Combine(PluginDirectoryPath, "cardData_netrunner.xml");
		}

		static byte[] StreamToBytes(Stream input) {
			var capacity = input.CanSeek ? (int)input.Length : 0;
			using (var output = new MemoryStream(capacity)) {
				int readLength;
				var buffer = new byte[4096];

				do {
					readLength = input.Read(buffer, 0, buffer.Length);
					output.Write(buffer, 0, readLength);
				}
				while (readLength != 0);

				return output.ToArray();
			}
		}

		public override bool LoadPlugin() {
			API.Instance.AddImageSourceFactory(new NetrunnerCardImageSourceFactory());

			if (!Directory.Exists(PluginDirectoryPath)) {
				Directory.CreateDirectory(PluginDirectoryPath);
			}

			if (!File.Exists(NetrunnerDataFilePath)) {

				WebClient client = new WebClient();
				client.DownloadStringCompleted += client_DownloadStringCompleted;
				client.DownloadStringAsync(new Uri("http://netrunnerdb.com/api/cards"));
			}
			return true;
		}

		void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e) {
			StringBuilder download = new StringBuilder(e.Result).Replace("\"code\"", "\"@code\"")
				.Insert(0, "{'card':").Append("}");
			XDocument doc = JsonConvert.DeserializeXNode(download.ToString(), "cards");
			doc.Save(NetrunnerDataFilePath);
		}

		private static string GetCardFileUrl(System.Xml.Linq.XElement card) {
			return "http://www.netrunnerdb.com" + card.Element("imagesrc").Value.ToString();
		}

		public static string DownloadCardFile(System.Xml.Linq.XElement card, bool overrideIfExists) {

			string remoteName = GetCardFileUrl(card);
			if (remoteName != null && !string.IsNullOrEmpty(card.Element("imagesrc").Value)) {
				string newFileName = System.IO.Path.Combine(
					NetrunnerPlugin.PluginDirectoryPath,
					System.IO.Path.GetFileNameWithoutExtension(remoteName)
				);

				if (overrideIfExists || !File.Exists(newFileName + "m.png")) {
					WebClient c = new WebClient();
					c.DownloadFile(remoteName, newFileName + ".png");

					System.Drawing.Image original = System.Drawing.Image.FromFile(newFileName + ".png");
					System.Drawing.Bitmap modified = new Bitmap(original);
					ImageCodecInfo imageCodecInfo = GetEncoderInfo(ImageFormat.Png);

					// Create an Encoder object for the Quality parameter.
					System.Drawing.Imaging.Encoder encoder = System.Drawing.Imaging.Encoder.Quality;

					// Create an EncoderParameters object. 
					EncoderParameters encoderParameters = new EncoderParameters(1);

					// Save the image as a JPEG file with quality level.
					EncoderParameter encoderParameter = new EncoderParameter(encoder, 100);
					encoderParameters.Param[0] = encoderParameter;
					modified.Save(newFileName + "m.png", imageCodecInfo, encoderParameters);
					modified.Dispose();
					original.Dispose();

					File.Delete(newFileName + ".png");
				}
				return newFileName;

			}
			return null;
		}
		private static ImageCodecInfo GetEncoderInfo(ImageFormat format) {
			return ImageCodecInfo.GetImageDecoders().SingleOrDefault(c => c.FormatID == format.Guid);
		}
	}
}
