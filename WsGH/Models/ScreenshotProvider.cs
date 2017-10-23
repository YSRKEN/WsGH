using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Windows;

namespace AzLH {
	using dPoint = System.Drawing.Point;
	delegate void AfterAction();
	delegate void AfterAction2(dPoint point);
	class ScreenshotProvider {
		Rectangle screenshotRectangle = new Rectangle(0, 0, 0, 0);
		static int MinGameWindowX = 800;
		static int MinGameWindowY = 450;
		Rectangle virtualDisplayRectangle;
		int clickPointX, clickPointY;
		Color backgroundColor;
		// コンストラクタ
		public ScreenshotProvider(AfterAction aa, Color backgroundColor) {
			this.backgroundColor = backgroundColor;
			virtualDisplayRectangle = CalcVirtualDisplayRectangle();
			var virtualDisplayBitmap = GetVirtualDisplayBitmap(virtualDisplayRectangle);
			var cw = new ClickWindow(virtualDisplayRectangle, virtualDisplayBitmap, aa, GetScreenshotRectangle);
			cw.Show();
		}
		// 全てのディスプレイを覆う仮想スクリーンの位置および大きさを計算
		Rectangle CalcVirtualDisplayRectangle() {
			// 仮想スクリーンの位置および大きさを、1枚目のディスプレイで初期化
			Rectangle vdr = Screen.AllScreens.First().Bounds;
			// 各ディスプレイについて計算を行う
			foreach(var screen in Screen.AllScreens) {
				// X1・Y1は1枚付け足した際の左端・上端(スクリーン座標)
				var X1 = Math.Min(vdr.Left, screen.Bounds.Left);
				var Y1 = Math.Min(vdr.Top, screen.Bounds.Top);
				// X2・Y2は1枚付け足した際の右端・下端(スクリーン座標)
				var X2 = Math.Max(vdr.Right, screen.Bounds.Right);
				var Y2 = Math.Max(vdr.Bottom, screen.Bounds.Bottom);
				// 計算結果を変数vdrに反映し直す
				vdr.X = X1;
				vdr.Y = Y1;
				vdr.Width = X2 - X1;
				vdr.Height = Y2 - Y1;
			}
			return vdr;
		}
		// 仮想スクリーンのビットマップを生成する
		Bitmap GetVirtualDisplayBitmap(Rectangle virtualDisplayRectangle) {
			var vdb = new Bitmap(virtualDisplayRectangle.Width, virtualDisplayRectangle.Height);
			using(var g = Graphics.FromImage(vdb)) {
				g.CopyFromScreen(virtualDisplayRectangle.Location, new System.Drawing.Point(), vdb.Size);
			}
			return vdb;
		}
		// スクショする位置および大きさを計算
		void GetScreenshotRectangle(dPoint mousePoint) {
			// クリックした座標を、クライアント座標基準で取得する
			clickPointX = mousePoint.X - virtualDisplayRectangle.Left;
			clickPointY = mousePoint.Y - virtualDisplayRectangle.Top;
			// クリックした座標から、ゲーム画面の座標を逆算する
			var vdb = GetVirtualDisplayBitmap(virtualDisplayRectangle);
			var gameWindowRectangle = GetGameWindowRectangle(vdb, backgroundColor, clickPointX, clickPointY);
			// ゲーム画面の座標をスクリーン座標に変換する
			screenshotRectangle.X = gameWindowRectangle.X + virtualDisplayRectangle.X;
			screenshotRectangle.Y = gameWindowRectangle.Y + virtualDisplayRectangle.Y;
			screenshotRectangle.Size = gameWindowRectangle.Size;
		}
		// 座標取得に成功したかを判定する
		public bool IsGetPosition() {
			if(screenshotRectangle.Width <= 0)
				return false;
			if(screenshotRectangle.Height <= 0)
				return false;
			return true;
		}
		// 取得した座標を表示する
		public string GetPositionStr() {
			return "(" + screenshotRectangle.X.ToString() + "," + screenshotRectangle.Y.ToString() + ") " + screenshotRectangle.Width.ToString() + "x" + screenshotRectangle.Height.ToString();
		}
		// スクショを取得する(スクショ出来ない際はnullが返る)
		public Bitmap GetScreenShot(bool forTwitterFlg = false) {
			if(!IsGetPosition())
				return null;
			var bitmap = new Bitmap(screenshotRectangle.Width, screenshotRectangle.Height);
			using(var g = Graphics.FromImage(bitmap)) {
				g.CopyFromScreen(screenshotRectangle.Location, new System.Drawing.Point(), bitmap.Size);
			}
			if(forTwitterFlg) {
				// Twitter用に画像を少しだけ加工する
				var color = bitmap.GetPixel(0, 0);
				var color_a = (color.A == 0 ? 1 : color.A - 1);
				var color_r = color.R;
				var color_g = color.G;
				var color_b = color.B;
				bitmap.SetPixel(0, 0, Color.FromArgb(color_a, color_r, color_g, color_b));
			}
			return bitmap;
		}
		// ズレを検知する
		public bool IsPositionShifting() {
			// 通常より1ピクセルだけ大きく取得する
			var bitmap = new Bitmap(screenshotRectangle.Width + 2, screenshotRectangle.Height + 2);
			using(var g = Graphics.FromImage(bitmap)) {
				g.CopyFromScreen(screenshotRectangle.X - 1, screenshotRectangle.Y - 1, 0, 0, bitmap.Size);
			}
			var backgroundColorArgb = backgroundColor.ToArgb();
			if((bitmap.GetPixel(0, 0).ToArgb() != backgroundColorArgb)
			&& (bitmap.GetPixel(0, screenshotRectangle.Height + 1).ToArgb() != backgroundColorArgb)
			&& (bitmap.GetPixel(screenshotRectangle.Width + 1, 0).ToArgb() != backgroundColorArgb)
			&& (bitmap.GetPixel(screenshotRectangle.Width + 1, screenshotRectangle.Height + 1).ToArgb() != backgroundColorArgb)) {
				return true;
			}
			return false;
		}
		// ズレを自動修正する
		public bool TryPositionShifting() {
			var vdb = GetVirtualDisplayBitmap(virtualDisplayRectangle);
			// クリックした座標から、ゲーム画面の座標を逆算する
			var gameWindowRectangle = GetGameWindowRectangle(vdb, backgroundColor, clickPointX, clickPointY);
			// ゲーム画面の座標をスクリーン座標に変換する
			screenshotRectangle.X = gameWindowRectangle.X + virtualDisplayRectangle.X;
			screenshotRectangle.Y = gameWindowRectangle.Y + virtualDisplayRectangle.Y;
			screenshotRectangle.Size = gameWindowRectangle.Size;
			// サイズ判定
			if(!IsGetPosition())
				return false;
			// ズレ判定
			if(IsPositionShifting())
				return false;
			return true;
		}
		// 色同士の距離を算出し、遠すぎる場合に「違う色」と判定する
		static bool IsNearColor(Color A, Color B) {
			int diffR = A.R - B.R;
			int diffG = A.G - B.G;
			int diffB = A.B - B.B;
			return (diffR * diffR + diffG * diffG + diffB * diffB) < 500;
		}
		static bool IsNearColor(Color A, Color B, int limit) {
			int diffR = A.R - B.R;
			int diffG = A.G - B.G;
			int diffB = A.B - B.B;
			return (diffR * diffR + diffG * diffG + diffB * diffB) < limit;
		}
		// ゲーム画面の座標を逆算する
		static Rectangle GetGameWindowRectangle(Bitmap bitmap, Color backgroundColor, int clickPointX, int clickPointY) {
			var gwr = new Rectangle(clickPointX, clickPointY, 0, 0);
			var borderColor = Color.FromArgb(backgroundColor.R, backgroundColor.G, backgroundColor.B);
			// 画面を探索し、ゲーム画面と思わしき部分を検出する
			for (int y = 0; y < Math.Min(bitmap.Height - MinGameWindowY, clickPointY); ++y) {
				for (int x = 0; x < Math.Min(bitmap.Width - MinGameWindowX, clickPointX); ++x) {
					// 枠の左上座標
					if (!IsNearColor(bitmap.GetPixel(x, y), borderColor, 1))
						continue;
					// その右
					if (!IsNearColor(bitmap.GetPixel(x + 1, y), borderColor, 1))
						continue;
					// その下
					if (!IsNearColor(bitmap.GetPixel(x, y + 1), borderColor, 1))
						continue;
					// その右下
					if (IsNearColor(bitmap.GetPixel(x + 1, y + 1), borderColor, 1))
						continue;
					// MinGameWindowX×MinGameWindowYまで走査
					{
						bool flg = true;
						for (int i = 2; i <= MinGameWindowX; ++i) {
							if (!IsNearColor(bitmap.GetPixel(x + i, y), borderColor, 1)) {
								flg = false;
								break;
							}
						}
						if (!flg)
							continue;
					}
					{
						bool flg = true;
						for (int i = 2; i <= MinGameWindowY; ++i) {
							if (!IsNearColor(bitmap.GetPixel(x, y + i), borderColor, 1)) {
								flg = false;
								break;
							}
						}
						if (!flg)
							continue;
					}
					// 反対側の辺についても調査
					for(int x2 = Math.Min(Math.Max(x + MinGameWindowX + 1, clickPointX + 1), bitmap.Width - 1); x2 < bitmap.Width; ++x2) {
						// 探索の起点となるy座標は逆算する
						int y2 = y + (x2 - x - 1) * MinGameWindowY / MinGameWindowX + 1;
						int width = x2 - x - 1;
						// クリック点を覆う範囲になりうるかをチェック
						if (y2 <= clickPointY)
							continue;
						// 探索開始
						if (!IsNearColor(bitmap.GetPixel(x2, y2), borderColor, 1))
							continue;
						if (!IsNearColor(bitmap.GetPixel(x2 - 1, y2), borderColor, 1))
							continue;
						if (!IsNearColor(bitmap.GetPixel(x2, y2 - 1), borderColor, 1))
							continue;
						if (IsNearColor(bitmap.GetPixel(x2 - 1, y2 - 1), borderColor, 1))
							continue;
						{
							bool flg = true;
							for (int x3 = x + MinGameWindowX + 1; x3 < x2; ++x3) {
								if (!IsNearColor(bitmap.GetPixel(x3, y2), borderColor, 1)) {
									flg = false;
									break;
								}
							}
							if (!flg)
								continue;
						}
						{
							bool flg = true;
							for (int y3 = y + MinGameWindowY + 1; y3 < y2; ++y3) {
								if (!IsNearColor(bitmap.GetPixel(x2, y3), borderColor, 1)) {
									flg = false;
									break;
								}
							}
							if (!flg)
								continue;
						}
						// 全てのチェックを通過したので座標登録
						gwr = new Rectangle(x + 1, y + 1, x2 - x - 1, y2 - y - 1);
						return gwr;
					}
				}
			}
			//bitmap.Save("hoge.png");
			return gwr;
		}
		// ゲーム画面の座標をクリックさせるためのインナークラス
		sealed class ClickWindow : Window {
			// 表示用ビットマップ
			Bitmap vdb;
			// 表示する座標
			Rectangle vdr;
			// クリック完了時に何とかするための奴
			AfterAction aa;
			AfterAction2 aa2;
			// BitmapSourceを引っ張るためにコレを使わざるを得ない現実
			internal static class NativeMethods {
				[System.Runtime.InteropServices.DllImport("gdi32.dll")]
				public static extern bool DeleteObject(IntPtr hObject);
			}
			// コンストラクタ
			public ClickWindow(Rectangle virtualDisplayRectangle, Bitmap virtualDisplayBitmap, AfterAction aa, AfterAction2 aa2) {
				// 仕方ないね
				vdr = virtualDisplayRectangle;
				// クリック完了時にGUIに反映するための細工
				this.aa = aa;
				this.aa2 = aa2;
				// 表示用に仮想スクリーンのビットマップを生成する
				vdb = virtualDisplayBitmap;
				// 画像を表示するためImageコントロールを用意する
				// (http://bacchus.ivory.ne.jp/gin/post-979/)
				var ScreenshotImage = new System.Windows.Controls.Image();
				var hbitmap = vdb.GetHbitmap();
				ScreenshotImage.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
					hbitmap, IntPtr.Zero, Int32Rect.Empty,
					System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
				NativeMethods.DeleteObject(hbitmap);
				ScreenshotImage.Height = vdr.Height;
				ScreenshotImage.Width = vdr.Width;
				// Gridを作成し、Imageコントロールを乗せる
				var grid = new System.Windows.Controls.Grid();
				grid.Children.Add(ScreenshotImage);
				// ウィンドウについての設定を行う
				// 縁無しに設定する
				// (http://5000164.jp/2014-03-wpf_practice_1/)
				WindowStyle = WindowStyle.None;
				AllowsTransparency = true;
				// 表示座標を決める
				Left = virtualDisplayRectangle.Left;
				Top = virtualDisplayRectangle.Top;
				Height = virtualDisplayRectangle.Height;
				Width = virtualDisplayRectangle.Width;
				// コンテンツを設定する
				Content = grid;
				// イベントを設定する
				MouseDown += ClickWindow_Click;
				KeyDown += ClickWindow_Key;
			}
			// クリックイベント
			private void ClickWindow_Click(object sender, RoutedEventArgs e) {
				// ScreenshotProviderにアクションを伝達させる
				aa2(Control.MousePosition);
				// GUIにアクションを伝達させる
				aa();
				Close();
			}
			// キーボードイベント
			private void ClickWindow_Key(object sender, System.Windows.Input.KeyEventArgs e) {
				// Escキーなら反応する
				if(e.Key == System.Windows.Input.Key.Escape) {
					aa();
					Close();
				}
			}
		}
	}
}
