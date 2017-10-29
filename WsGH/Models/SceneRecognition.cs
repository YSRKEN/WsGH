﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace AzLH {
	static class SceneRecognition {
		// 各種定数定義
		#region シーン認識用定数
		public enum SceneType { Unknown, Expedition, Build, Home, Building, Support, FShop };
		#endregion
		#region 遠征用定数
		// 遠征艦隊数
		static int ExpFleetCount = 4;
		// グローウォームのアイコンのRect
		static RectangleF[] ExpGlowwormPosition = {
			new RectangleF(23.05f, 36.11f, 2.344f, 4.167f),
			new RectangleF(23.05f, 49.17f, 2.344f, 4.167f),
			new RectangleF(23.05f, 62.36f, 2.344f, 4.167f),
			new RectangleF(23.05f, 75.42f, 2.344f, 4.167f),
		};
		static RectangleF[] ExpTimerPosition = {
			new RectangleF(30.94f, 41.25f, 7.200f, 2.778f),
			new RectangleF(30.94f, 54.31f, 7.200f, 2.778f),
			new RectangleF(30.94f, 67.50f, 7.200f, 2.778f),
			new RectangleF(30.94f, 80.56f, 7.200f, 2.778f),
		};
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
		// メイン資材表示の位置・大きさ
		static RectangleF MainSupplyDigitPositionF = new RectangleF(55.55f, 3.111f, 7.625f, 2.667f);
		static RectangleF MainSupplyDigitPositionM = new RectangleF(71.75f, 3.111f, 7.625f, 2.667f);
		static RectangleF MainSupplyDigitPositionD = new RectangleF(87.88f, 3.111f, 7.500f, 2.667f);
		static RectangleF[] MainSupplyDigitPosition = {
			MainSupplyDigitPositionF,
			MainSupplyDigitPositionM,
			MainSupplyDigitPositionD,
		};
		// サブ資材表示の位置・大きさ
		static RectangleF SubSupplyDigitPositionC = new RectangleF(59.06f, 14.03f, 4.844f, 3.333f);
		static RectangleF SubSupplyDigitPositionD = new RectangleF(57.50f, 13.75f, 5.234f, 3.472f);
		static RectangleF SubSupplyDigitPositionM = new RectangleF(58.98f, 14.17f, 6.484f, 3.472f);
		static RectangleF SubSupplyDigitPositionF = new RectangleF(65.47f, 10.00f, 8.594f, 4.167f);
		static RectangleF[] SubSupplyDigitPosition = {
			SubSupplyDigitPositionC,
			SubSupplyDigitPositionD,
			SubSupplyDigitPositionM,
			SubSupplyDigitPositionF,
		};
		#endregion
		#region OCR用定数
		// OCRする際にリサイズするサイズ
		static Size TemplateSize1 = new Size(32, 32);
		// OCRする際にマッチングさせる元のサイズ
		static Size TemplateSize2 = new Size(TemplateSize1.Width + 2, TemplateSize1.Height + 2);
		// OCRする際にマッチングさせる先の画像
		static IplImage TemplateSource, TemplateSource2, TemplateSource3;
		#endregion
		#region その他定数
		static DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);
		#endregion
		// 認識用初期化
		public static void InitialSceneRecognition() {
			TemplateSource = BitmapConverter.ToIplImage(Properties.Resources.ocr_template);
			TemplateSource2 = BitmapConverter.ToIplImage(Properties.Resources.ocr_template_2);
			TemplateSource3 = BitmapConverter.ToIplImage(Properties.Resources.ocr_template_alh);
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
			// Rangeを意図的に1スタートにしているのは、
			// FirstOrDefaultメソッドが検索して見つからなかった際、
			// Rangeが参照型ではなく値型なので0が帰ってくるから。
			// つまり、発見できず0が返ってくるのか、
			// 座標0で発見したから0が返ってくるのかが判別できない。
			// なのであえて1スタートにしている
			var rect = new Rectangle(new Point(0, 0), bitmap.Size);
			var xRange = Enumerable.Range(1, bitmap.Width);
			var yRange = Enumerable.Range(1, bitmap.Height);
			// 上下左右の境界を取得する
			var borderColor = Color.FromArgb(255, 255, 255);
			// 左
			foreach(var x in xRange) {
				// borderColorと等しくない色を発見した場合、pos >= 0になる
				var pos = yRange.FirstOrDefault(y => bitmap.GetPixel(x - 1, y - 1) != borderColor);
				if(pos >= 1) {
					rect.X = x - 1;
					rect.Width -= rect.X;
					break;
				}
			}
			// 上
			foreach(var y in yRange) {
				var pos = xRange.FirstOrDefault(x => bitmap.GetPixel(x - 1, y - 1) != borderColor);
				if(pos >= 1) {
					rect.Y = y - 1;
					rect.Height -= rect.Y;
					break;
				}
			}
			// 右
			foreach(var x in xRange.Reverse()) {
				var pos = yRange.FirstOrDefault(y => bitmap.GetPixel(x - 1, y - 1) != borderColor);
				if(pos >= 1) {
					rect.Width -= bitmap.Width - x;
					break;
				}
			}
			// 下
			foreach(var y in yRange.Reverse()) {
				var pos = xRange.FirstOrDefault(x => bitmap.GetPixel(x - 1, y - 1) != borderColor);
				if(pos >= 1) {
					rect.Height -= bitmap.Height - y;
					break;
				}
			}
			return rect;
		}
		/// <summary>
		/// 数字認識を行う
		/// 座標・大きさは画像に対する％指定なことに注意
		/// (色反転後に閾値処理を行う)
		/// </summary>
		/// <param name="bitmap">画像</param>
		/// <param name="px_arr">切り取る左座標</param>
		/// <param name="py_per">切り取る上座標</param>
		/// <param name="wx_per">切り取る幅</param>
		/// <param name="wy_per">切り取る高さ</param>
		/// <param name="thresold">閾値。これより暗いと黒とみなす</param>
		/// <param name="negaFlg">trueだと色を反転させて判定する</param>
		/// <param name="templateSourceIndex">数値の上限・下限を設定する</param>
		/// <returns>読み取った数値。0～9および10(白色＝読めなかった)をListで返す</returns>
		static List<int> GetDigitOCR(Bitmap bitmap, float[] px_arr, float py_per, float wx_per, float wy_per, int thresold, bool negaFlg, int templateSourceIndex = 0) {
			var output = new List<int>();
			foreach(float px_per in px_arr) {
				// ％指定をピクセル指定に直す
				int bitmapWidth = bitmap.Width;
				int bitmapHeight = bitmap.Height;
				int px = (int)(bitmapWidth * px_per / 100 + 0.5);
				int py = (int)(bitmapHeight * py_per / 100 + 0.5);
				int wx = (int)(bitmapWidth * wx_per / 100 + 0.5);
				int wy = (int)(bitmapHeight * wy_per / 100 + 0.5);
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
					var templateSource = (templateSourceIndex == 0 ? TemplateSource : TemplateSource2);
					var resultSize = new CvSize(templateSource.Width - image.Width + 1, templateSource.Height - image.Height + 1);
					using(var resultImage = Cv.CreateImage(resultSize, BitDepth.F32, 1)) {
						Cv.MatchTemplate(templateSource, image, resultImage, MatchTemplateMethod.SqDiff);
						CvPoint minPosition, maxPosition;
						Cv.MinMaxLoc(resultImage, out minPosition, out maxPosition);
						matchPosition = new Point(minPosition.X, minPosition.Y);
					}
				}
				// マッチング結果を数値に翻訳する
				if(templateSourceIndex == 0) {
					int matchNumber = (int)Math.Round(1.0 * matchPosition.X / TemplateSize2.Width / 2, 0);
					matchNumber = (matchNumber < 0 ? 0 : matchNumber > 10 ? 10 : matchNumber);
					output.Add(matchNumber);
				} else {
					int matchNumber = (int)Math.Round(1.0 * matchPosition.X / TemplateSize2.Width / 2, 0);
					matchNumber = (matchNumber < 0 ? 0 : matchNumber >= ExpFleetCount ? 3 : matchNumber);
					output.Add(matchNumber);
				}
			}
			return output;
		}
		static int GetValueOCR(Bitmap bitmap, RectangleF rect, int thresold, bool negaFlg, bool diamondFlg, bool debugFlg = false) {
			// ％指定をピクセル指定に直す
			int bitmapWidth = bitmap.Width;
			int bitmapHeight = bitmap.Height;
			int px = (int)(bitmapWidth * rect.X / 100 + 0.5);
			int py = (int)(bitmapHeight * rect.Y / 100 + 0.5);
			int wx = (int)(bitmapWidth * rect.Width / 100 + 0.5);
			int wy = (int)(bitmapHeight * rect.Height / 100 + 0.5);
			// 画像を切り取る
			var canvas = new Bitmap(wx, wy);
			using (var g = Graphics.FromImage(canvas)) {
				// 切り取られる位置・大きさ
				var srcRect = new Rectangle(px, py, wx, wy);
				// 貼り付ける位置・大きさ
				var desRect = new Rectangle(0, 0, canvas.Width, canvas.Height);
				g.DrawImage(bitmap, desRect, srcRect, GraphicsUnit.Pixel);
			}
			if(debugFlg) canvas.Save("digit1.bmp");
			// ダイヤを勘定する時だけ、黄色部分を黒く塗りつぶす処理を行う
			if (diamondFlg) {
				for(int y = 0; y < canvas.Height; ++y) {
					for (int x = 0; x < canvas.Width; ++x) {
						var color = canvas.GetPixel(x, y);
						if (color.R > 200 && color.G > 200 && color.B < 100)
							canvas.SetPixel(x, y, Color.FromArgb(255, 255, 255));
					}
				}
			}
			// 二値化する
			using (var image = BitmapConverter.ToIplImage(canvas))
			using (var image2 = new IplImage(image.Size, BitDepth.U8, 1)) {
				Cv.CvtColor(image, image2, ColorConversion.BgrToGray);
				if (negaFlg)
					Cv.Not(image2, image2);
				if (debugFlg) image2.ToBitmap().Save("digit2.bmp");
				Cv.Threshold(image2, image2, thresold, 255, ThresholdType.Binary);
				canvas = image2.ToBitmap();
			}
			if (debugFlg) canvas.Save("digit3.bmp");
			// カット部分を検出する
			var whiteCount = Enumerable.Repeat(0, canvas.Width).ToArray();
			for(int x = 0; x < canvas.Width; ++x) {
				for (int y = 0; y < canvas.Height; ++y) {
					if(canvas.GetPixel(x, y).R > 128) {
						++whiteCount[x];
					}
				}
			}
			var whiteCountDiff = Enumerable.Repeat(canvas.Height, canvas.Width).ToArray();
			for (int x = 0; x < canvas.Width - 1; ++x) {
				whiteCountDiff[x] = whiteCount[x] - whiteCount[x + 1];
			}
			// カット時に使う左座標の一覧を抽出する
			int lastPos = -1;
			var cutLeftPos = new List<int>();
			for (int x = 1; x < canvas.Width; ++x) {
				// カット部分じゃない場合は無視する
				if (whiteCountDiff[x] < (int)(0.1 * canvas.Height)) continue;
				if (whiteCount[x - 1] < (int)(canvas.Height)) continue;
				// 前回のカット部分より一定以上離れてないと検知しない
				if (lastPos != -1 && x - lastPos < (int)(0.2 * canvas.Height)) continue;
				// カット実行
				cutLeftPos.Add(x);
				lastPos = x;
			}
			// 各カット毎に数値認識を行う
			var digit = new List<int>();
			for(int k = 0; k < cutLeftPos.Count - 1; ++k) {
				// 1つの数字分だけ取り出す
				var canvas2 = new Bitmap(cutLeftPos[k + 1] - cutLeftPos[k] - 1, canvas.Height);
				using (var g = Graphics.FromImage(canvas2)) {
					// 切り取られる位置・大きさ
					var srcRect = new Rectangle(cutLeftPos[k] + 1, 0, cutLeftPos[k + 1] - cutLeftPos[k] - 1, canvas.Height);
					// 貼り付ける位置・大きさ
					var desRect = new Rectangle(0, 0, canvas2.Width, canvas2.Height);
					g.DrawImage(canvas, desRect, srcRect, GraphicsUnit.Pixel);
				}
				if (debugFlg) canvas2.Save($"digit4-{k + 1}-1.bmp");
				// 認識用の大きさにリサイズする
				var canvas3 = new Bitmap(TemplateSize2.Width, TemplateSize2.Height);
				using (var g = Graphics.FromImage(canvas3)) {
					// 事前にcanvas3を赤色に塗りつぶす
					g.FillRectangle(Brushes.Red, 0, 0, canvas3.Width, canvas3.Height);
					// 切り取られる位置・大きさ
					var srcRect = GetTrimmingRectangle(canvas2);
					// 貼り付ける位置・大きさ
					var desRect = new Rectangle(1, 1, TemplateSize1.Width, TemplateSize1.Height);
					g.DrawImage(canvas2, desRect, srcRect, GraphicsUnit.Pixel);
				}
				if (debugFlg) canvas3.Save($"digit4-{k+1}-2.bmp");
				// マッチングを行う
				Point matchPosition;
				using (var image = BitmapConverter.ToIplImage(canvas3)) {
					var templateSource = TemplateSource;
					var resultSize = new CvSize(templateSource.Width - image.Width + 1, templateSource.Height - image.Height + 1);
					using (var resultImage = Cv.CreateImage(resultSize, BitDepth.F32, 1)) {
						Cv.MatchTemplate(templateSource, image, resultImage, MatchTemplateMethod.SqDiff);
						CvPoint minPosition, maxPosition;
						Cv.MinMaxLoc(resultImage, out minPosition, out maxPosition);
						matchPosition = new Point(minPosition.X, minPosition.Y);
					}
				}
				// マッチング結果を数値に翻訳する
				int matchNumber = (int)Math.Round(1.0 * matchPosition.X / TemplateSize2.Width / 2, 0);
				matchNumber = (matchNumber < 0 ? 0 : matchNumber > 10 ? 10 : matchNumber);
				digit.Add(matchNumber);
			}
			// 結果を数値化する
			int retVal = 0;
			foreach(var x in digit) {
				retVal *= 10;
				retVal += x;
			}
			return retVal;
		}
		static List<int> GetTimeOCR(Bitmap bitmap, RectangleF rect, int thresold, bool negaFlg, bool debugFlg = false) {
			// ％指定をピクセル指定に直す
			int bitmapWidth = bitmap.Width;
			int bitmapHeight = bitmap.Height;
			int px = (int)(bitmapWidth * rect.X / 100 + 0.5);
			int py = (int)(bitmapHeight * rect.Y / 100 + 0.5);
			int wx = (int)(bitmapWidth * rect.Width / 100 + 0.5);
			int wy = (int)(bitmapHeight * rect.Height / 100 + 0.5);
			// 画像を切り取る
			var canvas = new Bitmap(wx, wy);
			using (var g = Graphics.FromImage(canvas)) {
				// 切り取られる位置・大きさ
				var srcRect = new Rectangle(px, py, wx, wy);
				// 貼り付ける位置・大きさ
				var desRect = new Rectangle(0, 0, canvas.Width, canvas.Height);
				g.DrawImage(bitmap, desRect, srcRect, GraphicsUnit.Pixel);
			}
			if (debugFlg) canvas.Save("digit1.bmp");
			// 二値化する
			using (var image = BitmapConverter.ToIplImage(canvas))
			using (var image2 = new IplImage(image.Size, BitDepth.U8, 1)) {
				Cv.CvtColor(image, image2, ColorConversion.BgrToGray);
				if (negaFlg)
					Cv.Not(image2, image2);
				if (debugFlg) image2.ToBitmap().Save("digit2.bmp");
				Cv.Threshold(image2, image2, thresold, 255, ThresholdType.Binary);
				canvas = image2.ToBitmap();
			}
			if (debugFlg) canvas.Save("digit3.bmp");
			// カット部分を検出する
			var whiteCount = Enumerable.Repeat(0, canvas.Width).ToArray();
			for (int x = 0; x < canvas.Width; ++x) {
				for (int y = 0; y < canvas.Height; ++y) {
					if (canvas.GetPixel(x, y).R > 128) {
						++whiteCount[x];
					}
				}
			}
			var whiteCountDiff = Enumerable.Repeat(canvas.Height, canvas.Width).ToArray();
			for (int x = 0; x < canvas.Width - 1; ++x) {
				whiteCountDiff[x] = whiteCount[x] - whiteCount[x + 1];
			}
			// カット時に使う左座標の一覧を抽出する
			int lastPos = -1;
			var cutLeftPos = new List<int>();
			for (int x = 1; x < canvas.Width; ++x) {
				// カット部分じゃない場合は無視する
				if (whiteCountDiff[x] < (int)(0.1 * canvas.Height)) continue;
				if (whiteCount[x - 1] < (int)(0.9 * canvas.Height)) continue;
				// 前回のカット部分より一定以上離れてないと検知しない
				if (lastPos != -1 && x - lastPos < (int)(0.2 * canvas.Height)) continue;
				// カット実行
				cutLeftPos.Add(x);
				lastPos = x;
			}
			// 各カット毎に数値認識を行う
			var digit = new List<int>();
			for (int k = 0; k < cutLeftPos.Count - 1; ++k) {
				// 1つの数字分だけ取り出す
				var canvas2 = new Bitmap(cutLeftPos[k + 1] - cutLeftPos[k] - 1, canvas.Height);
				using (var g = Graphics.FromImage(canvas2)) {
					// 切り取られる位置・大きさ
					var srcRect = new Rectangle(cutLeftPos[k] + 1, 0, cutLeftPos[k + 1] - cutLeftPos[k] - 1, canvas.Height);
					// 貼り付ける位置・大きさ
					var desRect = new Rectangle(0, 0, canvas2.Width, canvas2.Height);
					g.DrawImage(canvas, desRect, srcRect, GraphicsUnit.Pixel);
				}
				if (debugFlg) canvas2.Save($"digit4-{k + 1}-1.bmp");
				// 認識用の大きさにリサイズする
				var canvas3 = new Bitmap(TemplateSize2.Width, TemplateSize2.Height);
				using (var g = Graphics.FromImage(canvas3)) {
					// 事前にcanvas3を赤色に塗りつぶす
					g.FillRectangle(Brushes.Red, 0, 0, canvas3.Width, canvas3.Height);
					// 切り取られる位置・大きさ
					var srcRect = GetTrimmingRectangle(canvas2);
					// 貼り付ける位置・大きさ
					var desRect = new Rectangle(1, 1, TemplateSize1.Width, TemplateSize1.Height);
					g.DrawImage(canvas2, desRect, srcRect, GraphicsUnit.Pixel);
				}
				if (debugFlg) canvas3.Save($"digit4-{k + 1}-2.bmp");
				// マッチングを行う
				Point matchPosition;
				using (var image = BitmapConverter.ToIplImage(canvas3)) {
					var templateSource = TemplateSource3;
					var resultSize = new CvSize(templateSource.Width - image.Width + 1, templateSource.Height - image.Height + 1);
					using (var resultImage = Cv.CreateImage(resultSize, BitDepth.F32, 1)) {
						Cv.MatchTemplate(templateSource, image, resultImage, MatchTemplateMethod.SqDiff);
						CvPoint minPosition, maxPosition;
						Cv.MinMaxLoc(resultImage, out minPosition, out maxPosition);
						matchPosition = new Point(minPosition.X, minPosition.Y);
					}
				}
				// マッチング結果を数値に翻訳する
				int matchNumber = (int)Math.Round(1.0 * matchPosition.X / TemplateSize2.Width / 2, 0);
				matchNumber = (matchNumber < 0 ? 0 : matchNumber > 11 ? 11 : matchNumber);
				digit.Add(matchNumber);
			}
			var output = new List<int>();
			if(digit.Count == 8) {
				output.Add(digit[0]);
				output.Add(digit[1]);
				output.Add(digit[3]);
				output.Add(digit[4]);
				output.Add(digit[6]);
				output.Add(digit[7]);
			}
			/*Console.Write("・");
			for (int i = 0; i < digit.Count; ++i)
				Console.Write($"{digit[i]} ");
			Console.WriteLine("");*/
			return output;
		}
		#endregion
		// 時刻を正規化する
		static uint GetLeastSecond(List<int> timerDigit) {
			timerDigit[0] = (timerDigit[0] > 5 ? 0 : timerDigit[0]);
			timerDigit[1] = (timerDigit[1] > 9 ? 0 : timerDigit[1]);
			timerDigit[2] = (timerDigit[2] > 5 ? 0 : timerDigit[2]);
			timerDigit[3] = (timerDigit[3] > 9 ? 0 : timerDigit[3]);
			timerDigit[4] = (timerDigit[4] > 5 ? 0 : timerDigit[4]);
			timerDigit[5] = (timerDigit[5] > 9 ? 0 : timerDigit[5]);
			var hour = timerDigit[0] * 10 + timerDigit[1];
			var minute = timerDigit[2] * 10 + timerDigit[3];
			var second = timerDigit[4] * 10 + timerDigit[5];
			return (uint)((hour * 60 + minute) * 60 + second);
		}
		// 資材を正規化する
		static int GetMainSupply(List<int> supplyDigit) {
			int supplyValue = 0;
			supplyValue += (supplyDigit[0] > 9 ? 0 : supplyDigit[0]) * 1000000;
			supplyValue += (supplyDigit[1] > 9 ? 0 : supplyDigit[0]) * 100000;
			supplyValue += (supplyDigit[2] > 9 ? 0 : supplyDigit[1]) * 10000;
			supplyValue += (supplyDigit[3] > 9 ? 0 : supplyDigit[2]) * 1000;
			supplyValue += (supplyDigit[4] > 9 ? 0 : supplyDigit[3]) * 100;
			supplyValue += (supplyDigit[5] > 9 ? 0 : supplyDigit[4]) * 10;
			supplyValue += (supplyDigit[6] > 9 ? 0 : supplyDigit[5]) * 1;
			return supplyValue;
		}
		// 特殊資材を正規化する
		static int GetSubSupply(List<int> supplyDigit) {
			int supplyValue = 0;
			supplyValue += (supplyDigit[0] > 9 ? 0 : supplyDigit[0]) * 10000;
			supplyValue += (supplyDigit[1] > 9 ? 0 : supplyDigit[1]) * 1000;
			supplyValue += (supplyDigit[2] > 9 ? 0 : supplyDigit[2]) * 100;
			supplyValue += (supplyDigit[3] > 9 ? 0 : supplyDigit[3]) * 10;
			supplyValue += (supplyDigit[4] > 9 ? 0 : supplyDigit[4]) * 1;
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
			if(IsHomeScene(bitmap))
				return SceneType.Home;
			if (IsBuildingScene(bitmap))
				return SceneType.Building;
			if (IsSupportScene(bitmap))
				return SceneType.Support;
			if (IsFShopScene(bitmap))
				return SceneType.FShop;
			return SceneType.Unknown;
		}
		#region 遠征関係
		// 遠征のシーンかを判定する
		static bool IsExpeditionScene(Bitmap bitmap) {
			{
				// 海軍食堂
				var hash = GetDifferenceHash(bitmap, 3.047, 21.94, 3.438, 6.111);
				if(GetHummingDistance(hash, 0xb58b09d51a4ecc4d) >= 20)
					return false;
			}
			{
				// 海軍売店
				var hash = GetDifferenceHash(bitmap, 25.94, 20.28, 3.438, 6.111);
				if(GetHummingDistance(hash, 0x3ca8527a19944b27) >= 20)
					return false;
			}
			return true;
		}
		// 遠征タイマーを取得する
		public static Dictionary<int, ulong> GetExpeditionTimer(Bitmap bitmap) {
			var output = new Dictionary<int, ulong>();
			ulong now_time = GetUnixTime(DateTime.Now);
			for(int fi = 0; fi < ExpFleetCount; ++fi) {
				// グローウォームのアイコンが出ていなければ、その行に遠征艦隊はいない
				// 艦隊帰投ボタンが出ていなければ、その行に遠征艦隊はいない
				ulong bhash = GetDifferenceHash(bitmap, ExpGlowwormPosition[fi]);
				if(GetHummingDistance(bhash, 0x4e199ca52aa29732) >= 20)
					continue;
				// 遠征時間を取得する
				var timerDigit = GetTimeOCR(bitmap, ExpTimerPosition[fi], 10, true);
				if (timerDigit.Count != 6)
					continue;
				// 遠征完了時間を計算して書き込む
				uint leastSecond = GetLeastSecond(timerDigit);
				output[fi] = now_time + leastSecond;
			}
			return output;
		}
		#endregion
		#region 建造関係
		// 建造のシーンかを判定する
		static bool IsBuildScene(Bitmap bitmap) {
			{
				// 資金アイコン
				var hash = GetDifferenceHash(bitmap, 81.48, 65.56, 2.813, 5.000);
				if(GetHummingDistance(hash, 0x83190a8e55a02162) >= 20)
					return false;
			}
			{
				// 建造アイコン
				var hash = GetDifferenceHash(bitmap, 2.734, 16.81, 3.281, 5.833);
				if(GetHummingDistance(hash, 0x850504cc4051f449) >= 20)
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
				if(GetHummingDistance(bhash, 0x4147b56a9d33cb1c) < 20) {
					output[li] = 0;
					continue;
				}
				// 建造中でなければ、その行に入渠艦隊はいない
				if (GetHummingDistance(bhash, 0x254565276737c138) >= 20) {
					output[li] = 0;
					continue;
				}
				// 建造時間を取得する
				var timerDigit = GetDigitOCR(bitmap, BuildTimerDigitPX, BuildTimerDigitPY[li], BuildTimerDigitWX, BuildTimerDigitWY, 100, true);
				var leastSecond = GetLeastSecond(timerDigit);
				output[li] = now_time + leastSecond;
			}
			return output;
		}
		// 建造中のシーンかを判定する
		static bool IsBuildingScene(Bitmap bitmap) {
			{
				// 建造中アイコン
				ulong hash = GetDifferenceHash(bitmap, 2.656, 31.25, 3.438, 6.111);
				if (GetHummingDistance(hash, 0xc693524a496d50a5) >= 20)
					return false;
			}
			{
				// ドリルアイコン
				ulong hash = GetDifferenceHash(bitmap, 46.88, 13.89, 2.031, 3.611);
				if (GetHummingDistance(hash, 0xd18c566de4551595) >= 20)
					return false;
			}
			return true;
		}
		// 支援のシーンかを判定する
		static bool IsSupportScene(Bitmap bitmap) {
			{
				// 支援アイコン
				ulong hash = GetDifferenceHash(bitmap, 2.578, 46.39, 3.125, 5.556);
				if (GetHummingDistance(hash, 0xc3a1c2591a054280) >= 20)
					return false;
			}
			{
				// 勲章アイコン
				ulong hash = GetDifferenceHash(bitmap, 47.89, 15.56, 1.250, 2.222);
				if (GetHummingDistance(hash, 0x324ab361521a0840) >= 20)
					return false;
			}
			return true;
		}
		// 家具屋のシーンかを判定する
		static bool IsFShopScene(Bitmap bitmap) {
			{
				// 屋根
				ulong hash = GetDifferenceHash(bitmap, 7.813, 8.194, 1.953, 3.472);
				if (GetHummingDistance(hash, 0x0000056c4151396d) >= 20)
					return false;
			}
			{
				// 家具アイコン
				ulong hash = GetDifferenceHash(bitmap, 62.50, 9.861, 1.563, 2.778);
				if (GetHummingDistance(hash, 0x264bad0e33e12161) >= 20)
					return false;
			}
			return true;
		}
		#endregion
		#region 母港関係
		// 母港のシーン(ボタンあり)かを判定する
		static bool IsHomeScene(Bitmap bitmap) {
			{
				// 目玉表示
				var hash = GetDifferenceHash(bitmap, 2.750, 18.89, 2.250, 4.444);
				if (GetHummingDistance(hash, 0x07718e133b13cc07) >= 20)
					return false;
			}
			{
				// 出撃ボタン
				var hash = GetDifferenceHash(bitmap, 78.75, 51.33, 5.375, 5.111);
				if (GetHummingDistance(hash, 0x45a851a9c84b5566) >= 20)
					return false;
			}
			return true;
		}
		// 資材量が表示されているかを判定する
		public static bool CanReadMainSupply(Bitmap bitmap) {
			// 資金の表示
			var hash = GetDifferenceHash(bitmap, 68.75, 2.889, 1.875, 3.333);
			if(GetHummingDistance(hash, 0x12131bdf63e10192) >= 20)
				return false;
			return true;
		}
		// 資材量を読み取る(MainSupply)
		public static List<int> GetMainSupply(Bitmap bitmap) {
			var output = new List<int>();
			// iの値により、燃料→資金→ダイヤと読み取り対象が変化する
			for (int i = 0; i < MainSupplyDigitPosition.Length; ++i) {
				output.Add(GetValueOCR(bitmap, MainSupplyDigitPosition[i], 15, true, (i == 2)));
			}
			return output;
		}
		// 資材量を読み取る(GetSubSupply)
		public static int GetSubSupply(int ti, Bitmap bitmap) {
			return GetValueOCR(bitmap, SubSupplyDigitPosition[ti], 128, (ti != 3), false);
		}
		#endregion
	}
}
