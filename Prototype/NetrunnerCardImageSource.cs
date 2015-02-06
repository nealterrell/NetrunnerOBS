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
		private XElement mConfig;

		private const int DEFAULT_WIDTH = 300;
		private const int DEFAULT_HEIGHT = 418;

		public NetrunnerCardImageSource(XElement config) {
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

					mCardTexture = GS.CreateTexture((UInt32)wb.PixelWidth, (UInt32)wb.PixelHeight, 
						GSColorFormat.GS_BGRA, null, false, false);

					mCardTexture.SetImage(wb.BackBuffer, GSImageFormat.GS_IMAGEFORMAT_BGRA,
						(UInt32)(wb.PixelWidth * 4));

					
				}
				else {
					mCardTexture = null;
				}
			}
		}

		override public void UpdateSettings() {

			XElement dataElement = mConfig.GetElement("data");
			LoadTexture(mConfig.GetString("file"));

			if (mCardTexture != null) {
				int width = mConfig.GetInt("width", (int)mCardTexture.Width);
				int height = mConfig.GetInt("height", (int)mCardTexture.Height);
				mConfig.SetInt("width", width);
				mConfig.SetInt("height", height);

				int cx = mConfig.Parent.GetInt("cx", width);
				int cy = mConfig.Parent.GetInt("cy", height);
				//Api.Log("cx: {0}, cy: {1}", cx, cy);

				mConfig.Parent.SetInt("cx", cx == 1 ? width : cx);
				mConfig.Parent.SetInt("cy", cy == 1 ? height : cy); ;

				Size.X = width;
				Size.Y = height;
				//Api.Log("SIZE: {0}x{1}", Size.X, Size.Y);
			}
			else {
				mConfig.Parent.SetInt("cx", DEFAULT_WIDTH);
				mConfig.Parent.SetInt("cy", DEFAULT_HEIGHT);
				mConfig.SetInt("width", DEFAULT_WIDTH);
				mConfig.SetInt("height", DEFAULT_HEIGHT);
				Size.X = 300;
				Size.Y = 418;
				//Api.Log("SIZE: {0}x{1}", Size.X, Size.Y);
			}
		}

		override public void Render(float x, float y, float width, float height) {
			lock (textureLock) {

				if (mCardTexture != null) {
					GS.DrawSprite(mCardTexture, 0xFFFFFFFF, x, y,
						x + width, y + height);
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