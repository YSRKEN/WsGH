using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;

namespace WsGH {
	static class SceneRecognition {
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
			var px = (int)(bitmapWidth * px_per / 100);
			var py = (int)(bitmapHeight * py_per / 100);
			var wx = (int)(bitmapWidth * wx_per / 100);
			var wy = (int)(bitmapHeight * wy_per / 100);
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
		// 遠征のシーンかを判定する
		public static bool isExpeditionScene(Bitmap bitmap) {
			{
				var hash = getDifferenceHash(bitmap, 10.11, 76.36, 3.525, 6.276);
				if(getHummingDistance(hash, 0x2d2e2ba5aaa22a2a) >= 20)
					return false;
			}
			{
				var hash = getDifferenceHash(bitmap, 21.86, 62.76, 3.878, 1.883);
				if(getHummingDistance(hash, 0xd2d0b8a8a4545656) >= 20)
					return false;
			}
			return true;
		}
	}
}
