using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLROBS;
using System.Threading;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;

namespace NetrunnerOBS {
	class NetrunnerCardImageSource : AbstractImageSource {
		private Object textureLock = new Object();
		private Texture mCardTexture = null;
		//private Texture mDebugTexture;
		private XElement mConfig;

		public NetrunnerCardImageSource(XElement config) {
			//MessageBox.Show("Construct");
			this.mConfig = config;
			UpdateSettings();

		}

		private void LoadTexture(String imageFile) {

			lock (textureLock) {
				if (mCardTexture != null) {
					mCardTexture.Dispose();
					mCardTexture = null;
				}

				if (File.Exists(imageFile)) {
					BitmapImage src = new BitmapImage();

					src.BeginInit();
					src.UriSource = new Uri(imageFile);
					src.EndInit();

					WriteableBitmap wb = new WriteableBitmap(src);

					mCardTexture = GS.CreateTexture((UInt32)wb.PixelWidth, (UInt32)wb.PixelHeight, GSColorFormat.GS_BGRA, null, false, false);

					mCardTexture.SetImage(wb.BackBuffer, GSImageFormat.GS_IMAGEFORMAT_BGRA, (UInt32)(wb.PixelWidth * 4));


					/*src = new BitmapImage();
					src.BeginInit();
					src.UriSource = new Uri(Path.Combine(CardImagePlugin.PluginDirectoryPath, "white.png"));
					src.EndInit();

					wb = new WriteableBitmap(src);
					mDebugTexture = GS.CreateTexture((UInt32)wb.PixelWidth, (UInt32)wb.PixelHeight, GSColorFormat.GS_BGRA, null, false, false);
					mDebugTexture.SetImage(wb.BackBuffer, GSImageFormat.GS_IMAGEFORMAT_BGRA, (UInt32)(wb.PixelWidth * 4));
					*/
					//config.Parent.SetInt("cx", wb.PixelWidth);
					//config.Parent.SetInt("cy", wb.PixelHeight);

					//Size.X = (float)wb.PixelWidth;
					//Size.Y = (float)wb.PixelHeight;

				}
				else {
					mCardTexture = null;
				}
			}
		}

		override public void UpdateSettings() {

			XElement dataElement = mConfig.GetElement("data");
			LoadTexture(mConfig.GetString("file"));

//			API.Instance.Log("!!!!!!!!!!!!!!!!!!!!!!!!!{0}", mConfig.Parent.
//);
			//Api.Log("!!!!!!!{0}", mConfig.Parent.Name);
			//foreach (var item in Enumerable.Range(0, mConfig.Parent.ElementCount())) {
			//	Api.Log("Property {0}: ", item);
			//	Api.Log("Value {0}", mConfig.Parent.GetElementById(item).Name);
			//}
			if (mCardTexture != null) {
				UInt32 width = (UInt32)mConfig.GetInt("width", (int)mCardTexture.Width);
				UInt32 height = (UInt32)mConfig.GetInt("height", (int)mCardTexture.Height);

				Size.X = width + 40;
				Size.Y = height + 40;

				//API.Instance.Log("**************************Size: {0} x {1}", width, height);
				mConfig.Parent.SetInt("cx", (Int32)Size.X);
				mConfig.Parent.SetInt("cy", (Int32)Size.Y);
			}
			//MessageBox.Show("Showing image " + config.GetString("file"));
		}

		override public void Render(float x, float y, float width, float height) {
			lock (textureLock) {

				if (mCardTexture != null) {
					GS.DrawSprite(mCardTexture, 0xFFFFFFFF, x + 10, y + 10,
						x + mCardTexture.Width + 10, y + mCardTexture.Height + 10);
				//	GS.DrawSprite(mDebugTexture, 0xFFFFFFFF, 0, 0, Size.X, Size.Y);
				}
			}
		}

		public void Dispose() {
			lock (textureLock) {
				if (mCardTexture != null) {
					mCardTexture.Dispose();
					mCardTexture = null;
				}
			}
		}
	}
}