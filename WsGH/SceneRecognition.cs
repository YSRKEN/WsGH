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
		public enum SceneType { Unknown, Expedition, Build, Develop, Dock, Home };
		#endregion
		#region 遠征用定数
		// 遠征艦隊数
		static int ExpFleetCount = 4;
		// 遠征リストの縦幅
		static int ExpListHeight = 4;
		// 艦隊帰投ボタンのRect
		static RectangleF[] ExpButtonPosition = {
			new RectangleF(91.38f, 10.22f, 4.375f, 7.778f),
			new RectangleF(91.38f, 31.11f, 4.375f, 7.778f),
			new RectangleF(91.38f, 52.00f, 4.375f, 7.778f),
			new RectangleF(91.38f, 72.89f, 4.375f, 7.778f),
		};
		// 艦隊番号アイコンのRect
		static RectangleF[] ExpFleetIconPosition = {
			new RectangleF(86.38f, 6.667f, 0.8750f, 2.444f),
			new RectangleF(86.38f, 27.56f, 0.8750f, 2.444f),
			new RectangleF(86.38f, 48.44f, 0.8750f, 2.444f),
			new RectangleF(86.38f, 69.33f, 0.8750f, 2.444f),
		};
		// 艦隊番号アイコンのハッシュ値
		static ulong[] ExtFleetIconHash = {
			0x840e0e8e0e0f0f03,
			0x0f2363070f3f0302,
			0xc80763070703270f,
			0x6787173777270707,
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
		// 入渠時間表示のRect
		static float[] DockTimerDigitPX = {66.86f, 68.86f, 72.03f, 74.15f, 77.32f, 79.32f};
		static float[] DockTimerDigitPY = {24.69f, 45.61f, 66.32f, 87.24f};
		static float DockTimerDigitWX = 1.880f, DockTimerDigitWY = 5.021f;
		// 高速修復ボタンのRect
		static RectangleF[] DockFastRepairPosition = {
			new RectangleF(87.19f, 22.36f, 3.125f, 5.556f),
			new RectangleF(87.19f, 43.19f, 3.125f, 5.556f),
			new RectangleF(87.19f, 64.03f, 3.125f, 5.556f),
			new RectangleF(87.19f, 84.86f, 3.125f, 5.556f),
		};
		#endregion
		#region 資材用定数
		// 資源表示の横位置
		static float[] MainSupplyFuelDigitPX    = {39.30f, 40.39f, 41.48f, 42.58f, 43.67f, 44.69f};
		static float[] MainSupplyAmmoDigitPX    = {51.02f, 52.11f, 53.20f, 54.30f, 55.39f, 56.41f};
		static float[] MainSupplySteelDigitPX   = {62.73f, 63.83f, 64.92f, 66.02f, 67.11f, 68.13f};
		static float[] MainSupplyBauxiteDigitPX = {74.45f, 75.55f, 76.64f, 77.73f, 78.83f, 79.84f};
		static float[] MainSupplyDiamondDigitPX = {87.97f, 89.06f, 90.16f, 91.09f, 92.27f, 93.28f};
		static float[][] MainSupplyDigitPX = {
			MainSupplyFuelDigitPX,
			MainSupplyAmmoDigitPX,
			MainSupplySteelDigitPX,
			MainSupplyBauxiteDigitPX,
			MainSupplyDiamondDigitPX,
		};
		// 資源表示の縦位置・大きさ
		static float MainSupplyDigitPY = 1.389f, MainSupplyDigitWX = 0.9375f, MainSupplyDigitWY = 2.222f;
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
		static ulong GetDifferenceHash(Bitmap bitmap, double px_per, double py_per, double wx_per, double wy_per) {
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
		static ulong GetDifferenceHash(Bitmap bitmap, RectangleF rect) {
			return GetDifferenceHash(bitmap, rect.X, rect.Y, rect.Width, rect.Height);
		}
		// ビットカウント
		static uint Popcnt(ulong x) {
			x = ((x & 0xaaaaaaaaaaaaaaaa) >> 1) + (x & 0x5555555555555555);
			x = ((x & 0xcccccccccccccccc) >> 2) + (x & 0x3333333333333333);
			x = ((x & 0xf0f0f0f0f0f0f0f0) >> 4) + (x & 0x0f0f0f0f0f0f0f0f);
			x = ((x & 0xff00ff00ff00ff00) >> 8) + (x & 0x00ff00ff00ff00ff);
			x = ((x & 0xffff0000ffff0000) >> 16) + (x & 0x0000ffff0000ffff);
			x = ((x & 0xffffffff00000000) >> 32) + (x & 0x00000000ffffffff);
			return (uint)x;
		}
		// ハミング距離を計算する
		static uint GetHummingDistance(ulong a, ulong b) {
			return Popcnt(a ^ b);
		}
		#endregion
		#region OCR関係
		// 周囲をトリミングする
		static Rectangle GetTrimmingRectangle(Bitmap bitmap) {
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
		/// (色反転後に閾値処理を行う)
		/// </summary>
		/// <param name="bitmap">画像</param>
		/// <param name="px_per">切り取る左座標</param>
		/// <param name="py_per">切り取る上座標</param>
		/// <param name="wx_per">切り取る幅</param>
		/// <param name="wy_per">切り取る高さ</param>
		/// <param name="thresold">閾値。これより暗いと黒とみなす</param>
		/// <param name="negaFlg">trueだと色を反転させて判定する</param>
		/// <returns>読み取った数値。0～9および10(白色＝読めなかった)をListで返す</returns>
		static List<int> GetDigitOCR(Bitmap bitmap, float[] px_arr, float py_per, float wx_per, float wy_per, int thresold, bool negaFlg) {
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
				var rect = GetTrimmingRectangle(canvas);
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
		static uint GetLeastSecond(List<int> timerDigit) {
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
		// 資材を正規化する
		static int GetMainSupply(List<int> supplyDigit) {
			// スタブ
			int supplyValue = 0;
			supplyValue += (supplyDigit[0] > 9 ? 0 : supplyDigit[0]) * 100000;
			supplyValue += (supplyDigit[1] > 9 ? 0 : supplyDigit[1]) * 10000;
			supplyValue += (supplyDigit[2] > 9 ? 0 : supplyDigit[2]) * 1000;
			supplyValue += (supplyDigit[3] > 9 ? 0 : supplyDigit[3]) * 100;
			supplyValue += (supplyDigit[4] > 9 ? 0 : supplyDigit[4]) * 10;
			supplyValue += (supplyDigit[5] > 9 ? 0 : supplyDigit[5]) * 1;
			return supplyValue;
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
			if(IsHomeScene(bitmap))
				return SceneType.Home;
			return SceneType.Unknown;
		}
		#region 遠征関係
		// 遠征のシーンかを判定する
		static bool IsExpeditionScene(Bitmap bitmap) {
			{
				// 左下の「母港」ボタンの近くにある装飾
				var hash = GetDifferenceHash(bitmap, 10.11, 76.36, 3.525, 6.276);
				if(GetHummingDistance(hash, 0x2d2f2ba7aaa22b2a) >= 20)
					return false;
			}
			{
				// 3・4段目の間の隙間にある模様の一部(左の方)
				var hash = GetDifferenceHash(bitmap, 21.86, 62.76, 3.878, 1.883);
				if(GetHummingDistance(hash, 0xd0b8a8a454565652) >= 20)
					return false;
			}
			{
				// 2段目の枠の左下の一部
				var hash = GetDifferenceHash(bitmap, 14.69, 38.91, 2.350, 4.184);
				if(GetHummingDistance(hash, 0xa0a8a091e8743834) >= 20)
					return false;
			}
			return true;
		}
		// 遠征タイマーを取得する
		public static Dictionary<int, ulong> GetExpeditionTimer(Bitmap bitmap) {
			var output = new Dictionary<int, ulong>();
			var now_time = GetUnixTime(DateTime.Now);
			for(int li = 0; li < ExpListHeight; ++li) {
				// 艦隊帰投ボタンが出ていなければ、その行に遠征艦隊はいない
				var bhash = GetDifferenceHash(bitmap, ExpButtonPosition[li]);
				if(GetHummingDistance(bhash, 0x1b60c68aca2e5635) >= 20)
					continue;
				// 遠征している艦隊の番号を取得する
				// ハッシュに対するハミング距離を計算した後、LINQで最小値のインデックス(艦隊番号)を取り出す
				var fhash = GetDifferenceHash(bitmap, ExpFleetIconPosition[li]);
				var hd = new List<uint>();
				for(int fi = 0; fi < ExpFleetCount; ++fi) {
					hd.Add(GetHummingDistance(fhash, ExtFleetIconHash[fi]));
				}
				int fleetIndex = hd
					.Select((val, idx) => new { V = val, I = idx })
					.Aggregate((min, working) => (min.V < working.V) ? min : working)
					.I;
				// 遠征時間を取得する
				//Console.WriteLine((li + 1) + "番目：第" + (fleetIndex + 1) + "艦隊");
				// 遠征完了時間を計算して書き込む
				//bitmap.Save("ss.png");
				var timerDigit = GetDigitOCR(bitmap, ExpTimerDigitPX, ExpTimerDigitPY[li], ExpTimerDigitWX, ExpTimerDigitWY, 120, true);
				var leastSecond = GetLeastSecond(timerDigit);
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
				var hash = GetDifferenceHash(bitmap, 1.763, 7.322, 6.933, 5.649);
				if(GetHummingDistance(hash, 0x225a551d98566290) >= 20)
					return false;
			}
			{
				// 建造枠
				var hash = GetDifferenceHash(bitmap, 36.78, 21.34, 2.233, 3.975);
				if(GetHummingDistance(hash, 0x56b94e5a5a52a55e) >= 20)
					return false;
			}
			{
				// 建造アイコン
				var hash = GetDifferenceHash(bitmap, 27.50, 8.996, 1.880, 4.184);
				if(GetHummingDistance(hash, 0xff555e5e807666c6) >= 20)
					return false;
			}
			return true;
		}
		// 建造タイマーを取得する
		public static Dictionary<int, ulong> GetBuildTimer(Bitmap bitmap) {
			var output = new Dictionary<int, ulong>();
			var now_time = GetUnixTime(DateTime.Now);
			for(int li = 0; li < BuildListHeight; ++li) {
				// スタンバイ表示ならば、その行に入渠艦隊はいない
				var bhash = GetDifferenceHash(bitmap, BuildStatusPosition[li]);
				if(GetHummingDistance(bhash, 0x4147b56a9d33cb1c) < 20)
					continue;
				// 建造中でなければ、その行に入渠艦隊はいない
				if(GetHummingDistance(bhash, 0x254565276737c138) >= 20)
					continue;
				// 建造時間を取得する
				var timerDigit = GetDigitOCR(bitmap, BuildTimerDigitPX, BuildTimerDigitPY[li], BuildTimerDigitWX, BuildTimerDigitWY, 100, true);
				var leastSecond = GetLeastSecond(timerDigit);
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
				var hash = GetDifferenceHash(bitmap, 1.880, 33.26, 7.051, 6.276);
				if(GetHummingDistance(hash, 0x02595a5aa939c8ab) >= 20)
					return false;
			}
			{
				// 開発枠
				var hash = GetDifferenceHash(bitmap, 36.78, 21.34, 2.233, 3.975);
				if(GetHummingDistance(hash, 0x56b94e5a5a52a55e) >= 20)
					return false;
			}
			{
				// 開発アイコン
				var hash = GetDifferenceHash(bitmap, 27.50, 8.996, 1.880, 4.184);
				if(GetHummingDistance(hash, 0xffdd5a8ddadcd896) >= 20)
					return false;
			}
			return true;
		}
		// 開発タイマーを取得する
		public static Dictionary<int, ulong> GetDevTimer(Bitmap bitmap) {
			var output = new Dictionary<int, ulong>();
			var now_time = GetUnixTime(DateTime.Now);
			for(int li = 0; li < DevListHeight; ++li) {
				// スタンバイ表示ならば、その行に入渠艦隊はいない
				var bhash = GetDifferenceHash(bitmap, DevStatusPosition[li]);
				if(GetHummingDistance(bhash, 0x4147b56a9d33cb1c) < 20)
					continue;
				// 開発中でなければ、その行に入渠艦隊はいない
				if(GetHummingDistance(bhash, 0x545471565654659a) >= 20)
					continue;
				// 建造時間を取得する
				var timerDigit = GetDigitOCR(bitmap, DevTimerDigitPX, DevTimerDigitPY[li], DevTimerDigitWX, DevTimerDigitWY, 100, true);
				var leastSecond = GetLeastSecond(timerDigit);
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
				var hash = GetDifferenceHash(bitmap, 1.880, 20.29, 6.933, 5.858);
				if(GetHummingDistance(hash, 0x846db65c641d5526) >= 20)
					return false;
			}
			{
				// 修復アイコン
				var hash = GetDifferenceHash(bitmap, 24.09, 9.205, 4.465, 4.393);
				if(GetHummingDistance(hash, 0x8776335455555522) >= 20)
					return false;
			}
			return true;
		}
		// 入渠タイマーを取得する
		public static Dictionary<int, ulong> GetDockTimer(Bitmap bitmap) {
			var output = new Dictionary<int, ulong>();
			var now_time = GetUnixTime(DateTime.Now);
			for(int li = 0; li < DockListHeight; ++li) {
				// 高速修復ボタンがなければ、その行に入渠艦隊はいない
				var bhash = GetDifferenceHash(bitmap, DockFastRepairPosition[li]);
				if(GetHummingDistance(bhash, 0x62cd568d66b66d9a) >= 20)
					continue;
				// 入渠時間を取得する
				var timerDigit = GetDigitOCR(bitmap, DockTimerDigitPX, DockTimerDigitPY[li], DockTimerDigitWX, DockTimerDigitWY, 50, true);
				var leastSecond = GetLeastSecond(timerDigit);
				output[li] = now_time + leastSecond;
			}
			return output;
		}
		#endregion
		#region 母港関係
		// 母港のシーン(ボタンあり)かを判定する
		static bool IsHomeScene(Bitmap bitmap) {
			// 左上の表示
			var hash = GetDifferenceHash(bitmap, 6.933, 1.674, 2.350, 4.184);
			if(GetHummingDistance(hash, 0x0712092214489850) >= 20)
				return false;
			return true;
		}
		// 資材量が表示されているかを判定する
		public static bool CanReadMainSupply(Bitmap bitmap) {
			// 弾薬の表示
			var hash = GetDifferenceHash(bitmap, 47.12, 0.8368, 2.115, 3.766);
			if(GetHummingDistance(hash, 0xd52a264d9cbd6bd3) >= 20)
				return false;
			return true;
		}
		// 資材量を読み取る(MainSupply)
		public static List<int> GetMainSupply(Bitmap bitmap) {
			var output = new List<int>();
			// iの値により、燃料→弾薬→鋼材→ボーキサイト→ダイヤと読み取り対象が変化する
			for(int i = 0; i < MainSupplyDigitPX.Count(); ++i) {
				var supplyDigit = GetDigitOCR(bitmap, MainSupplyDigitPX[i], MainSupplyDigitPY, MainSupplyDigitWX, MainSupplyDigitWY, 110, true);
				var supplyVaue = GetMainSupply(supplyDigit);
				output.Add(supplyVaue);
			}
			return output;
		}
		#endregion
	}
}
