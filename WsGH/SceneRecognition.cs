using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace WsGH {
	static class SceneRecognition {
		// 各種定数定義
		#region シーン認識用定数
		public enum SceneType { Unknown, Expedition, Build, Develop, Dock };
		public static Dictionary<SceneType, string> SceneString
			 = new Dictionary<SceneType, string> {
				{ SceneType.Unknown, "Unknown" },
				{ SceneType.Expedition, "Expedition" },
				{ SceneType.Build, "Build" },
				{ SceneType.Develop, "Develop" },
				{ SceneType.Dock, "Dock" },
			 };
		public static Dictionary<SceneType, string> SceneStringJapanese
			 = new Dictionary<SceneType, string> {
				{ SceneType.Unknown, "不明" },
				{ SceneType.Expedition, "遠征" },
				{ SceneType.Build, "建造" },
				{ SceneType.Develop, "開発" },
				{ SceneType.Dock, "入渠" },
			 };
		#endregion
		#region 遠征用定数
		// 遠征艦隊数
		static int ExpFleetCount = 4;
		// 遠征リストの縦幅
		static int ExpListHeight = 4;
		// 「艦隊派遣」ボタンのRect
		static RectangleF[] ExpButtonPosition = {
			new RectangleF(81.43f, 8.996f, 11.16f, 5.230f),
			new RectangleF(81.43f, 29.92f, 11.16f, 5.230f),
			new RectangleF(81.43f, 50.63f, 11.16f, 5.230f),
			new RectangleF(81.43f, 71.55f, 11.16f, 5.230f),
		};
		// 艦隊番号アイコンのRect
		static RectangleF[] ExpFleetIconPosition = {
			new RectangleF(86.60f, 6.695f, 0.9401f, 2.301f),
			new RectangleF(86.60f, 27.41f, 0.9401f, 2.301f),
			new RectangleF(86.60f, 48.33f, 0.9401f, 2.301f),
			new RectangleF(86.60f, 69.25f, 0.9401f, 2.301f),
		};
		// 艦隊番号アイコンのハッシュ値
		static ulong[] ExtFleetIconHash = {
			0x23373e38383e3e1c,
			0x313d8e0e3c7cfe1c,
			0x803d8c1c1c0e8e7c,
			0x991d1c9c9e9e1e1c,
		};
		// 遠征時間表示のRect
		static float[] ExpTimerDigitPX = {60.89f, 62.63f, 65.45f, 67.10f, 69.80f, 71.56f};
		static float[] ExpTimerDigitPY = {5.858f, 26.57f, 47.49f, 68.41f};
		static float ExpTimerDigitWX = 1.645f, ExpTimerDigitWY = 4.184f;
		#endregion
		#region 建造用定数
		// 建造リストの縦幅
		static int BuildListHeight = 4;
		// 画面左のステータス表示
		static RectangleF[] BuildStatusPosition = {
			new RectangleF(15.51f, 23.22f, 2.350f, 4.184f),
			new RectangleF(15.51f, 43.10f, 2.350f, 4.184f),
			new RectangleF(15.51f, 62.97f, 2.350f, 4.184f),
			new RectangleF(15.51f, 82.85f, 2.350f, 4.184f),
		};
		// 建造時間表示のRect
		static float[] BuildTimerDigitPX = {26.91f, 27.97f, 29.38f, 30.32f, 31.85f, 32.79f};
		static float[] BuildTimerDigitPY = {29.71f, 49.58f, 69.46f, 89.33f};
		static float BuildTimerDigitWX = 0.823f, BuildTimerDigitWY = 2.510f;
		#endregion
		#region 開発用定数
		// 開発リストの縦幅
		static int DevListHeight = 4;
		// 画面左のステータス表示
		static RectangleF[] DevStatusPosition = {
			new RectangleF(15.51f, 23.22f, 2.350f, 4.184f),
			new RectangleF(15.51f, 43.10f, 2.350f, 4.184f),
			new RectangleF(15.51f, 62.97f, 2.350f, 4.184f),
			new RectangleF(15.51f, 82.85f, 2.350f, 4.184f),
		};
		// 建造時間表示のRect
		static float[] DevTimerDigitPX = {26.91f, 27.97f, 29.38f, 30.32f, 31.85f, 32.79f};
		static float[] DevTimerDigitPY = {29.71f, 49.58f, 69.46f, 89.33f};
		static float DevTimerDigitWX = 0.823f, DevTimerDigitWY = 2.510f;
		#endregion
		#region 入渠用定数
		// 入渠リストの縦幅
		static int DockListHeight = 4;
		// 「選択」ボタンのRect
		static RectangleF[] DockButtonPosition = {
			new RectangleF(26.44f, 24.69f, 9.166f, 5.230f),
			new RectangleF(26.44f, 45.61f, 9.166f, 5.230f),
			new RectangleF(26.44f, 66.32f, 9.166f, 5.230f),
			new RectangleF(26.44f, 87.24f, 9.166f, 5.230f),
		};
		// 入渠時間表示のRect
		static float[] DockTimerDigitPX = {66.86f, 68.86f, 72.03f, 74.15f, 77.32f, 79.32f};
		static float[] DockTimerDigitPY = {24.69f, 45.61f, 66.32f, 87.24f};
		static float DockTimerDigitWX = 1.880f, DockTimerDigitWY = 5.021f;
		// 高速修復ボタンのRect
		static RectangleF[] DockFastRepairPosition = {
			new RectangleF(93.54f, 21.13f, 2.350f, 4.184f),
			new RectangleF(93.54f, 42.05f, 2.350f, 4.184f),
			new RectangleF(93.54f, 62.76f, 2.350f, 4.184f),
			new RectangleF(93.54f, 83.68f, 2.350f, 4.184f),
		};
		#endregion
		#region OCR用定数
		// OCRする際にリサイズするサイズ
		static Size TemplateSize1 = new Size(32, 32);
		// OCRする際にマッチングさせる元のサイズ
		static Size TemplateSize2 = new Size(TemplateSize1.Width + 2, TemplateSize1.Height + 2);
		// OCRする際にマッチングさせる先の画像
		static IplImage TemplateSource;
		#endregion
		#region その他定数
		static DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);
		#endregion
		// 認識用初期化
		public static void InitialSceneRecognition() {
			TemplateSource = BitmapConverter.ToIplImage(Properties.Resources.ocr_template);
		}
		#region DifferenceHash関係
		/// <summary>
		/// 画像の一部分におけるDifferenceHashを取得する
		/// 座標・大きさは画像に対する％指定なことに注意
		/// </summary>
		/// <param name="bitmap">画像</param>
		/// <param name="px_per">切り取る左座標</param>
		/// <param name="py_per">切り取る上座標</param>
		/// <param name="wx_per">切り取る幅</param>
		/// <param name="wy_per">切り取る高さ</param>
		/// <returns>64bitのDifferenceHash値</returns>
		static ulong getDifferenceHash(Bitmap bitmap, double px_per, double py_per, double wx_per, double wy_per) {
			// ％指定をピクセル指定に直す
			var bitmapWidth = bitmap.Width;
			var bitmapHeight = bitmap.Height;
			var px = (int)(bitmapWidth * px_per / 100 + 0.5);
			var py = (int)(bitmapHeight * py_per / 100 + 0.5);
			var wx = (int)(bitmapWidth * wx_per / 100 + 0.5);
			var wy = (int)(bitmapHeight * wy_per / 100 + 0.5);
			// 画像を切り取り、横9ピクセル縦8ピクセルにリサイズする
			// その際にグレースケール化も同時に施す
			var canvas = new Bitmap(9, 8);
			using(var g = Graphics.FromImage(canvas)) {
				// 切り取られる位置・大きさ
				var srcRect = new Rectangle(px, py, wx, wy);
				// 貼り付ける位置・大きさ
				var desRect = new Rectangle(0, 0, canvas.Width, canvas.Height);
				// グレースケール変換用のマトリックスを設定
				var cm = new ColorMatrix(
					new float[][]{
						new float[]{0.299f, 0.299f, 0.299f, 0 ,0},
						new float[]{0.587f, 0.587f, 0.587f, 0, 0},
						new float[]{0.114f, 0.114f, 0.114f, 0, 0},
						new float[]{0, 0, 0, 1, 0},
						new float[]{0, 0, 0, 0, 1}
					}
				);
				var ia = new ImageAttributes();
				ia.SetColorMatrix(cm);
				// 描画
				// ImageAttributesを設定しなければ、
				// g.DrawImage(bitmap, desRect, srcRect, GraphicsUnit.Pixel);
				// で済むのにMSェ……
				g.DrawImage(
					bitmap, desRect, srcRect.X, srcRect.Y,
					srcRect.Width, srcRect.Height, GraphicsUnit.Pixel, ia
				);
			}
			/*var canvas2 = new Bitmap(wx, wy);
			using(var g = Graphics.FromImage(canvas2)) {
				// 切り取られる位置・大きさ
				var srcRect = new Rectangle(px, py, wx, wy);
				// 貼り付ける位置・大きさ
				var desRect = new Rectangle(0, 0, canvas2.Width, canvas2.Height);
				// グレースケール変換用のマトリックスを設定
				var cm = new ColorMatrix(
					new float[][]{
						new float[]{0.299f, 0.299f, 0.299f, 0 ,0},
						new float[]{0.587f, 0.587f, 0.587f, 0, 0},
						new float[]{0.114f, 0.114f, 0.114f, 0, 0},
						new float[]{0, 0, 0, 1, 0},
						new float[]{0, 0, 0, 0, 1}
					}
				);
				var ia = new ImageAttributes();
				ia.SetColorMatrix(cm);
				// 描画
				// ImageAttributesを設定しなければ、
				// g.DrawImage(bitmap, desRect, srcRect, GraphicsUnit.Pixel);
				// で済むのにMSェ……
				g.DrawImage(
					bitmap, desRect, srcRect.X, srcRect.Y,
					srcRect.Width, srcRect.Height, GraphicsUnit.Pixel, ia
				);
			}
			canvas2.Save("canvas2.bmp");*/
			// 隣接ピクセルとの比較結果を符号化する
			ulong hash = 0;
			for(int y = 0; y < 8; ++y) {
				for(int x = 0; x < 8; ++x) {
					hash <<= 1;
					if(canvas.GetPixel(x, y).R > canvas.GetPixel(x + 1, y).R)
						hash |= 1;
				}
			}
			return hash;
		}
		/// <summary>
		/// 画像の一部分におけるDifferenceHashを取得する
		/// Rectは画像に対する％指定なことに注意
		/// </summary>
		/// <param name="bitmap">画像</param>
		/// <param name="rect">切り取るRect</param>
		/// <returns>64bitのDifferenceHash値</returns>
		static ulong getDifferenceHash(Bitmap bitmap, RectangleF rect) {
			return getDifferenceHash(bitmap, rect.X, rect.Y, rect.Width, rect.Height);
		}
		// ビットカウント
		static uint popcnt(ulong x) {
			x = ((x & 0xaaaaaaaaaaaaaaaa) >> 1) + (x & 0x5555555555555555);
			x = ((x & 0xcccccccccccccccc) >> 2) + (x & 0x3333333333333333);
			x = ((x & 0xf0f0f0f0f0f0f0f0) >> 4) + (x & 0x0f0f0f0f0f0f0f0f);
			x = ((x & 0xff00ff00ff00ff00) >> 8) + (x & 0x00ff00ff00ff00ff);
			x = ((x & 0xffff0000ffff0000) >> 16) + (x & 0x0000ffff0000ffff);
			x = ((x & 0xffffffff00000000) >> 32) + (x & 0x00000000ffffffff);
			return (uint)x;
		}
		// ハミング距離を計算する
		static uint getHummingDistance(ulong a, ulong b) {
			return popcnt(a ^ b);
		}
		#endregion
		#region OCR関係
		// 周囲をトリミングする
		static Rectangle getTrimmingRectangle(Bitmap bitmap) {
			var rect = new Rectangle(new Point(0, 0), bitmap.Size);
			// 上下左右の境界を取得する
			var borderColor = Color.FromArgb(255, 255, 255);
			// 左
			for(int x = 0; x < bitmap.Width; ++x) {
				bool borderFlg = false;
				for(int y = 0; y < bitmap.Height; ++y) {
					if(bitmap.GetPixel(x, y) != borderColor) {
						borderFlg = true;
						break;
					}
				}
				if(borderFlg) {
					rect.X = x;
					rect.Width -= x;
					break;
				}
			}
			// 上
			for(int y = 0; y < bitmap.Height; ++y) {
				bool borderFlg = false;
				for(int x = 0; x < bitmap.Width; ++x) {
					if(bitmap.GetPixel(x, y) != borderColor) {
						borderFlg = true;
						break;
					}
				}
				if(borderFlg) {
					rect.Y = y;
					rect.Height -= y;
					break;
				}
			}
			// 右
			for(int x = bitmap.Width - 1; x >= 0; --x) {
				bool borderFlg = false;
				for(int y = 0; y < bitmap.Height; ++y) {
					if(bitmap.GetPixel(x, y) != borderColor) {
						borderFlg = true;
						break;
					}
				}
				if(borderFlg) {
					rect.Width -= bitmap.Width - x - 1;
					break;
				}
			}
			// 下
			for(int y = bitmap.Height - 1; y >= 0; --y) {
				bool borderFlg = false;
				for(int x = 0; x < bitmap.Width; ++x) {
					if(bitmap.GetPixel(x, y) != borderColor) {
						borderFlg = true;
						break;
					}
				}
				if(borderFlg) {
					rect.Height -= bitmap.Height - y - 1;
					break;
				}
			}
			return rect;
		}
		// 数字認識を行う
		/// <summary>
		/// 数字認識を行う
		/// 座標・大きさは画像に対する％指定なことに注意
		/// </summary>
		/// <param name="bitmap">画像</param>
		/// <param name="px_per">切り取る左座標</param>
		/// <param name="py_per">切り取る上座標</param>
		/// <param name="wx_per">切り取る幅</param>
		/// <param name="wy_per">切り取る高さ</param>
		/// <param name="thresold">閾値。これより暗いと黒とみなす</param>
		/// <param name="negaFlg">trueだと色を反転させて判定する</param>
		/// <returns>読み取った数値。0～9および10(白色＝読めなかった)をListで返す</returns>
		static List<int> getDigitOCR(Bitmap bitmap, float[] px_arr, float py_per, float wx_per, float wy_per, int thresold, bool negaFlg) {
			var output = new List<int>();
			foreach(var px_per in px_arr) {
				// ％指定をピクセル指定に直す
				var bitmapWidth = bitmap.Width;
				var bitmapHeight = bitmap.Height;
				var px = (int)(bitmapWidth * px_per / 100 + 0.5);
				var py = (int)(bitmapHeight * py_per / 100 + 0.5);
				var wx = (int)(bitmapWidth * wx_per / 100 + 0.5);
				var wy = (int)(bitmapHeight * wy_per / 100 + 0.5);
				// 画像を切り取る
				var canvas = new Bitmap(wx, wy);
				using(var g = Graphics.FromImage(canvas)) {
					// 切り取られる位置・大きさ
					var srcRect = new Rectangle(px, py, wx, wy);
					// 貼り付ける位置・大きさ
					var desRect = new Rectangle(0, 0, canvas.Width, canvas.Height);
					g.DrawImage(bitmap, desRect, srcRect, GraphicsUnit.Pixel);
				}
				//canvas.Save("digit1.bmp");
				// 二値化する
				using(var image = BitmapConverter.ToIplImage(canvas))
				using(var image2 = new IplImage(image.Size, BitDepth.U8, 1)) {
					Cv.CvtColor(image, image2, ColorConversion.BgrToGray);
					if(negaFlg)
						Cv.Not(image2, image2);
					Cv.Threshold(image2, image2, thresold, 255, ThresholdType.Binary);
					canvas = image2.ToBitmap();
				}
				//canvas.Save("digit2.bmp");
				// 周囲をトリミングした上で、所定のサイズにリサイズする
				// 背景は赤色に塗りつぶすこと
				var rect = getTrimmingRectangle(canvas);
				var canvas2 = new Bitmap(TemplateSize2.Width, TemplateSize2.Height);
				using(var g = Graphics.FromImage(canvas2)) {
					// 事前にcanvas2を赤色に塗りつぶす
					g.FillRectangle(Brushes.Red, 0, 0, canvas2.Width, canvas2.Height);
					// 切り取られる位置・大きさ
					var srcRect = rect;
					// 貼り付ける位置・大きさ
					var desRect = new Rectangle(1, 1, TemplateSize1.Width, TemplateSize1.Height);
					g.DrawImage(canvas, desRect, srcRect, GraphicsUnit.Pixel);
				}
				//canvas2.Save("digit3.bmp");
				// マッチングを行う
				Point matchPosition;
				using(var image = BitmapConverter.ToIplImage(canvas2)) {
					var resultSize = new CvSize(TemplateSource.Width - image.Width + 1, TemplateSource.Height - image.Height + 1);
					using(var resultImage = Cv.CreateImage(resultSize, BitDepth.F32, 1)) {
						Cv.MatchTemplate(TemplateSource, image, resultImage, MatchTemplateMethod.SqDiff);
						CvPoint minPosition, maxPosition;
						Cv.MinMaxLoc(resultImage, out minPosition, out maxPosition);
						matchPosition = new Point(minPosition.X, minPosition.Y);
					}
				}
				// マッチング結果を数値に翻訳する
				int matchNumber = (int)Math.Round(1.0 * matchPosition.X / TemplateSize2.Width / 2, 0);
				matchNumber = (matchNumber < 0 ? 0 : matchNumber > 10 ? 10 : matchNumber);
				output.Add(matchNumber);
			}
			return output;
		}
		#endregion
		// 時刻を正規化する
		static uint getLeastSecond(List<int> timerDigit) {
			/*foreach(var d in timerDigit)
				Console.Write(d + " ");
			Console.WriteLine("");*/
			timerDigit[0] = (timerDigit[0] > 5 ? 0 : timerDigit[0]);
			timerDigit[1] = (timerDigit[1] > 9 ? 0 : timerDigit[1]);
			timerDigit[2] = (timerDigit[2] > 5 ? 0 : timerDigit[2]);
			timerDigit[3] = (timerDigit[3] > 9 ? 0 : timerDigit[3]);
			timerDigit[4] = (timerDigit[4] > 5 ? 0 : timerDigit[4]);
			timerDigit[5] = (timerDigit[5] > 9 ? 0 : timerDigit[5]);
			var hour = timerDigit[0] * 10 + timerDigit[1];
			var minute = timerDigit[2] * 10 + timerDigit[3];
			var second = timerDigit[4] * 10 + timerDigit[5];
			//Console.WriteLine(hour + ":" + minute + ":" + second);
			return (uint)((hour * 60 + minute) * 60 + second);
		}
		// UNIX時間を計算する
		public static ulong GetUnixTime(DateTime dt) {
			var dt2 = dt.ToUniversalTime();
			var elapsedTime = dt2 - UnixEpoch;
			return (ulong)elapsedTime.TotalSeconds;
		}
		// Scene判定
		public static SceneType JudgeScene(Bitmap bitmap) {
			if(IsExpeditionScene(bitmap))
				return SceneType.Expedition;
			if(IsBuildScene(bitmap))
				return SceneType.Build;
			if(IsDevelopScene(bitmap))
				return SceneType.Develop;
			if(IsDockScene(bitmap))
				return SceneType.Dock;
			return SceneType.Unknown;
		}
		#region 遠征関係
		// 遠征のシーンかを判定する
		public static bool IsExpeditionScene(Bitmap bitmap) {
			{
				// 左下の「母港」ボタンの近くにある装飾
				var hash = getDifferenceHash(bitmap, 10.11, 76.36, 3.525, 6.276);
				if(getHummingDistance(hash, 0x2d2f2ba7aaa22b2a) >= 20)
					return false;
			}
			{
				// 3・4段目の間の隙間にある模様の一部(左の方)
				var hash = getDifferenceHash(bitmap, 21.86, 62.76, 3.878, 1.883);
				if(getHummingDistance(hash, 0xd0b8a8a454565652) >= 20)
					return false;
			}
			{
				// 2段目の枠の左下の一部
				var hash = getDifferenceHash(bitmap, 14.69, 38.91, 2.350, 4.184);
				if(getHummingDistance(hash, 0xa0a8a091e8743834) >= 20)
					return false;
			}
			return true;
		}
		// 遠征タイマーを取得する
		public static Dictionary<int, ulong> getExpeditionTimer(Bitmap bitmap) {
			var output = new Dictionary<int, ulong>();
			var now_time = GetUnixTime(DateTime.Now);
			for(int li = 0; li < ExpListHeight; ++li) {
				// 「艦隊派遣」ボタンが出ていれば、その行に遠征艦隊はいない
				var bhash = getDifferenceHash(bitmap, ExpButtonPosition[li]);
				if(getHummingDistance(bhash, 0xb5424a525a6c516a) < 20)
					continue;
				// 遠征している艦隊の番号を取得する
				// ハッシュに対するハミング距離を計算した後、LINQで最小値のインデックス(艦隊番号)を取り出す
				var fhash = getDifferenceHash(bitmap, ExpFleetIconPosition[li]);
				var hd = new List<uint>();
				for(int fi = 0; fi < ExpFleetCount; ++fi) {
					hd.Add(getHummingDistance(fhash, ExtFleetIconHash[fi]));
				}
				int fleetIndex = hd
					.Select((val, idx) => new { V = val, I = idx })
					.Aggregate((min, working) => (min.V < working.V) ? min : working)
					.I;
				// 遠征時間を取得する
				//Console.WriteLine((li + 1) + "番目：第" + (fleetIndex + 1) + "艦隊");
				// 遠征完了時間を計算して書き込む
				//bitmap.Save("ss.png");
				var timerDigit = getDigitOCR(bitmap, ExpTimerDigitPX, ExpTimerDigitPY[li], ExpTimerDigitWX, ExpTimerDigitWY, 120, true);
				var leastSecond = getLeastSecond(timerDigit);
				output[fleetIndex] = now_time + leastSecond;
			}
			return output;
		}
		#endregion
		#region 建造関係
		// 建造のシーンかを判定する
		static bool IsBuildScene(Bitmap bitmap) {
			{
				// 建造ボタン
				var hash = getDifferenceHash(bitmap, 1.763, 7.322, 6.933, 5.649);
				if(getHummingDistance(hash, 0x225a551d98566290) >= 20)
					return false;
			}
			{
				// 建造枠
				var hash = getDifferenceHash(bitmap, 36.78, 21.34, 2.233, 3.975);
				if(getHummingDistance(hash, 0x56b94e5a5a52a55e) >= 20)
					return false;
			}
			{
				// 建造アイコン
				var hash = getDifferenceHash(bitmap, 27.50, 8.996, 1.880, 4.184);
				if(getHummingDistance(hash, 0xff555e5e807666c6) >= 20)
					return false;
			}
			return true;
		}
		// 建造タイマーを取得する
		public static Dictionary<int, ulong> getBuildTimer(Bitmap bitmap) {
			var output = new Dictionary<int, ulong>();
			var now_time = GetUnixTime(DateTime.Now);
			for(int li = 0; li < BuildListHeight; ++li) {
				// スタンバイ表示ならば、その行に入渠艦隊はいない
				var bhash = getDifferenceHash(bitmap, BuildStatusPosition[li]);
				if(getHummingDistance(bhash, 0x4147b56a9d33cb1c) < 20)
					continue;
				// 建造中でなければ、その行に入渠艦隊はいない
				if(getHummingDistance(bhash, 0x254565276737c138) >= 20)
					continue;
				// 建造時間を取得する
				var timerDigit = getDigitOCR(bitmap, BuildTimerDigitPX, BuildTimerDigitPY[li], BuildTimerDigitWX, BuildTimerDigitWY, 100, true);
				var leastSecond = getLeastSecond(timerDigit);
				//Console.WriteLine($"{li + 1}番目：{leastSecond}");
				output[li] = now_time + leastSecond;
			}
			return output;
		}
		#endregion
		#region 開発関係
		// 開発のシーンかを判定する
		static bool IsDevelopScene(Bitmap bitmap) {
			{
				// 開発ボタン
				var hash = getDifferenceHash(bitmap, 1.880, 33.26, 7.051, 6.276);
				if(getHummingDistance(hash, 0x02595a5aa939c8ab) >= 20)
					return false;
			}
			{
				// 開発枠
				var hash = getDifferenceHash(bitmap, 36.78, 21.34, 2.233, 3.975);
				if(getHummingDistance(hash, 0x56b94e5a5a52a55e) >= 20)
					return false;
			}
			{
				// 開発アイコン
				var hash = getDifferenceHash(bitmap, 27.50, 8.996, 1.880, 4.184);
				if(getHummingDistance(hash, 0xffdd5a8ddadcd896) >= 20)
					return false;
			}
			return true;
		}
		// 開発タイマーを取得する
		public static Dictionary<int, ulong> getDevTimer(Bitmap bitmap) {
			var output = new Dictionary<int, ulong>();
			var now_time = GetUnixTime(DateTime.Now);
			for(int li = 0; li < DevListHeight; ++li) {
				// スタンバイ表示ならば、その行に入渠艦隊はいない
				var bhash = getDifferenceHash(bitmap, DevStatusPosition[li]);
				if(getHummingDistance(bhash, 0x4147b56a9d33cb1c) < 20)
					continue;
				// 開発中でなければ、その行に入渠艦隊はいない
				if(getHummingDistance(bhash, 0x545471565654659a) >= 20)
					continue;
				// 建造時間を取得する
				var timerDigit = getDigitOCR(bitmap, DevTimerDigitPX, DevTimerDigitPY[li], DevTimerDigitWX, DevTimerDigitWY, 100, true);
				var leastSecond = getLeastSecond(timerDigit);
				Console.WriteLine($"{li + 1}番目：{leastSecond}");
				output[li] = now_time + leastSecond;
			}
			return output;
		}
		#endregion
		#region 入渠関係
		// 入渠のシーンかを判定する
		static bool IsDockScene(Bitmap bitmap) {
			{
				// 修復ボタン
				var hash = getDifferenceHash(bitmap, 1.880, 20.29, 6.933, 5.858);
				if(getHummingDistance(hash, 0x846db65c641d5526) >= 20)
					return false;
			}
			{
				// 修復アイコン
				var hash = getDifferenceHash(bitmap, 24.09, 9.205, 4.465, 4.393);
				if(getHummingDistance(hash, 0x8776335455555522) >= 20)
					return false;
			}
			return true;
		}
		// 入渠タイマーを取得する
		public static Dictionary<int, ulong> getDockTimer(Bitmap bitmap) {
			var output = new Dictionary<int, ulong>();
			var now_time = GetUnixTime(DateTime.Now);
			for(int li = 0; li < DockListHeight; ++li) {
				// 「選択」ボタンが出ていれば、その行に入渠艦隊はいない
				var bhash = getDifferenceHash(bitmap, DockButtonPosition[li]);
				if(getHummingDistance(bhash, 0x8d352d6d89354a80) < 20)
					continue;
				// 高速修復ボタンがなければ、その行に入渠艦隊はいない
				bhash = getDifferenceHash(bitmap, DockFastRepairPosition[li]);
				if(getHummingDistance(bhash, 0x29b86aaa98b2997c) >= 20)
					continue;
				// 入渠時間を取得する
				var timerDigit = getDigitOCR(bitmap, DockTimerDigitPX, DockTimerDigitPY[li], DockTimerDigitWX, DockTimerDigitWY, 50, true);
				var leastSecond = getLeastSecond(timerDigit);
				//Console.WriteLine($"{li + 1}番目：{leastSecond}");
				output[li] = now_time + leastSecond;
			}
			return output;
		}
		#endregion
	}
}
