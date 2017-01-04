using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Windows;

namespace WsGH {
	using System.Windows.Controls;
	using System.Windows.Media.Imaging;
	using cImage = System.Windows.Controls.Image;
	class ScreenshotProvider {
		System.Drawing.Rectangle ScreenshotRectangle;
		public ScreenshotProvider() {
			//! 全てのディスプレイにおけるスクリーンショットを取得
			var screenshotList = new List<Bitmap>();
			var displayRectangleList = new List<Rectangle>();
			foreach(var screen in Screen.AllScreens) {
				var temp = new Rectangle(screen.Bounds.Location, screen.Bounds.Size);
				displayRectangleList.Add(temp);
				var bmp = new Bitmap(screen.Bounds.Width, screen.Bounds.Height);
				using(var g = Graphics.FromImage(bmp)) {
					g.CopyFromScreen(screen.Bounds.Location, new System.Drawing.Point(), bmp.Size);
				}
				screenshotList.Add(bmp);
			}
			// ダミーとなるウィンドウを展開し、クリックさせる
			System.Windows.MessageBox.Show("Please click window of WarshipGirls.\n(Esc Key : Cancel)", "WsGH", MessageBoxButton.OK);
			var clickWindowList = new List<ClickWindow>();
			for(int i = 0; i < screenshotList.Count(); ++i) {
				var cw = new ClickWindow(screenshotList[i], displayRectangleList[i]);
				cw.Show();
				clickWindowList.Add(cw);
			}
			//! スクリーンショットを保存(テストコード)
			/*int index = 0;
			foreach(var screenshot in screenshotList) {
				screenshot.Save("SS_" + index.ToString() + "_" + displayRectangleList[index].ToString() + ".png");
				++index;
			}*/
		}
		// ゲーム画面の座標をクリックさせるためのインナークラス
		class ClickWindow : Window {
			cImage ScreenshotImage;

			[System.Runtime.InteropServices.DllImport("gdi32.dll")]
			public static extern bool DeleteObject(IntPtr hObject);

			public ClickWindow(Bitmap bitmap, Rectangle rectangle) {
				// 縁無しに設定する
				// (http://5000164.jp/2014-03-wpf_practice_1/)
				WindowStyle = WindowStyle.None;
				AllowsTransparency = true;

				// 表示座標を決める
				Left = rectangle.Left;
				Top = rectangle.Top;
				Height = rectangle.Height;
				Width = rectangle.Width;

				// 画像を表示するためImageコントロールを用意する
				// (http://bacchus.ivory.ne.jp/gin/post-979/)
				// BitmapからBitmapSourceに直接変換するメソッドを用意してないとか舐めてんのかM$
				ScreenshotImage = new cImage();
				var hbitmap = bitmap.GetHbitmap();
				ScreenshotImage.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
				DeleteObject(hbitmap);
				ScreenshotImage.Height = rectangle.Height;
				ScreenshotImage.Width = rectangle.Width;

				// Gridを作成し、Imageコントロールを乗せる
				var grid = new Grid();
				grid.Children.Add(ScreenshotImage);
				Content = grid;

				// クリックイベントを設定する
				MouseDown += ClickWindow_Click;
			}

			private void ClickWindow_Click(object sender, RoutedEventArgs e) {
				System.Windows.MessageBox.Show(System.Windows.Forms.Control.MousePosition.ToString());
				Close();
			}
		}
	}
}
