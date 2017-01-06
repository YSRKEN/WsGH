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
	using dPoint = System.Drawing.Point;
	class ScreenshotProvider {
		public bool getPosFlg = false;
		public Rectangle screenshotRectangle;
		public ScreenshotProvider() {
			//! 全てのディスプレイにおけるスクリーンショットを取得
			var screenshotList = new List<Bitmap>();
			var displayRectangleList = new List<Rectangle>();
			foreach(var screen in Screen.AllScreens) {
				var temp = new Rectangle(screen.Bounds.Location, screen.Bounds.Size);
				displayRectangleList.Add(temp);
				var bmp = new Bitmap(screen.Bounds.Width, screen.Bounds.Height);
				using(var g = Graphics.FromImage(bmp)) {
					g.CopyFromScreen(screen.Bounds.Location, new dPoint(), bmp.Size);
				}
				screenshotList.Add(bmp);
			}
			// 全てのディスプレイを覆う仮想スクリーンを計算
			var virtualDisplayRectangle = displayRectangleList[0];
			foreach(var dr in displayRectangleList) {
				var X2 = Math.Max(virtualDisplayRectangle.Right, dr.Right);
				var Y2 = Math.Max(virtualDisplayRectangle.Bottom, dr.Bottom);
				virtualDisplayRectangle.X = Math.Min(virtualDisplayRectangle.X, dr.X);
				virtualDisplayRectangle.Y = Math.Min(virtualDisplayRectangle.Y, dr.Y);
				virtualDisplayRectangle.Width = X2 - virtualDisplayRectangle.X;
				virtualDisplayRectangle.Height = Y2 - virtualDisplayRectangle.Y;
			}
			var virtualDisplayBmp = new Bitmap(virtualDisplayRectangle.Width, virtualDisplayRectangle.Height);
			for(int i = 0; i < screenshotList.Count(); ++i) {
				using(var g = Graphics.FromImage(virtualDisplayBmp)) {
					g.DrawImage(
						screenshotList[i],
						displayRectangleList[i].X - virtualDisplayRectangle.X,
						displayRectangleList[i].Y - virtualDisplayRectangle.Y
					);
				}
			}
			// ダミーとなるウィンドウを展開し、クリックさせる
			//System.Windows.MessageBox.Show("Please click window of WarshipGirls.\n(Esc Key : Cancel)", "WsGH", MessageBoxButton.OK);
			var cw = new ClickWindow(virtualDisplayBmp, virtualDisplayRectangle);
			cw.Show();
		}
		// ClickWindowでクリックした後の処理
		static void AfterClickWindow(bool getPosFlg, dPoint mousePoint) {
			//getPosFlg = getPosFlg_;
			System.Windows.MessageBox.Show(mousePoint.ToString());
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
				AfterClickWindow(true, System.Windows.Forms.Control.MousePosition);
				Close();
			}
		}
	}
}
