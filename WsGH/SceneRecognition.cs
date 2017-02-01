using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WsGH {
	static class SceneRecognition {
		/// <summary>
		/// 画像の一部分におけるDifferenceHashを取得する
		/// 座標・大きさは画像に対する％指定なことに注意
		/// </summary>
		/// <param name="bitmap">画像</param>
		/// <param name="px">切り取る左座標</param>
		/// <param name="py">切り取る上座標</param>
		/// <param name="wx">切り取る幅</param>
		/// <param name="wy">切り取る高さ</param>
		/// <returns>64bitのDifferenceHash値</returns>
		static ulong getDifferenceHash(Bitmap bitmap, double px, double py, double wx, double wy) {
			//スタブ
			return 0;
		}
		// ビットカウント
		static uint popcnt(ulong x) {
			x = ((x & 0xaaaaaaaaaaaaaaaa) >> 1) + (x & 0x5555555555555555);
			x = ((x & 0xcccccccccccccccc) >> 2) + (x & 0x3333333333333333);
			x = ((x & 0xf0f0f0f0f0f0f0f0) >> 4) + (x & 0x0f0f0f0f0f0f0f0f);
			x = ((x & 0xff00ff00ff00ff00) >> 8) + (x & 0x00ff00ff00ff00ff);
			x = ((x & 0xffff0000ffff0000) >> 16) + (x & 0x0000ffff0000ffff);
			x = ((x & 0xffffffff00000000) >> 32) + (x & 0x00000000ffffffff);
			return x;
		}
		// ハミング距離を計算する
		static uint getHummingDistance(ulong a, ulong b) {
			return popcnt(a ^ b);
		}
		// 遠征のシーンかを判定する
		public static bool isExpeditionScene(Bitmap bitmap) {
			{
				var hash = getDifferenceHash(bitmap, 10.11, 76.36, 3.525, 6.276);
				if(getHummingDistance(hash, 0x2d2a17a726222a2a) >= 20)
					return false;
			}
			{
				var hash = getDifferenceHash(bitmap, 21.86, 62.76, 3.878, 1.883);
				if(getHummingDistance(hash, 0x94d072682d656476) >= 20)
					return false;
			}
			return true;
		}
	}
}
