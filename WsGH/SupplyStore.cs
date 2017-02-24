using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WsGH {
	using SupplyPair = KeyValuePair<DateTime, int>;
	using SupplyList = List<KeyValuePair<DateTime, int>>;
	static class SupplyStore {
		#region MainSupply関係
		// MainSupplyの最終更新日時
		static DateTime LastUpdate = Properties.Settings.Default.LastUpdate;
		// MainSupplyの本体
		public static List<SupplyList> MainSupplyData = null;
		// MainSupplyの種類
		public static string[] MainSupplyType = { "Fuel", "Ammo", "Steel", "Bauxite", "Diamond" };
		// MainSupplyの大きさ
		public static int MainSupplyTypeCount = MainSupplyType.Count();
		public static int MainSupplyListCount = 0;
		// MainSupplyの初期化
		static void InitialMainSupply() {
			MainSupplyData = new List<SupplyList> {
				new SupplyList(), new SupplyList(), new SupplyList(), new SupplyList(), new SupplyList()
			};
			MainSupplyListCount = 0;
		}
		// MainSupplyをCSVから読み込む
		public static void ReadMainSupply() {
			// CSVから読み込む
			var lastUpdate = new DateTime();
			InitialMainSupply();
			using(var sr = new System.IO.StreamReader(@"MainSupply.csv")) {
				while(!sr.EndOfStream) {
					// 1行を読み込む
					var line = sr.ReadLine();
					// マッチさせてから各数値を取り出す
					var pattern = @"(?<Year>\d+)/(?<Month>\d+)/(?<Day>\d+) (?<Hour>\d+):(?<Minute>\d+):(?<Second>\d+),(?<Fuel>\d+),(?<Ammo>\d+),(?<Steel>\d+),(?<Bauxite>\d+),(?<Diamond>\d+)";
					var match = Regex.Match(line, pattern);
					if(!match.Success) {
						continue;
					}
					// 取り出した数値を元に、MainSupplyDataに入力する
					try {
						// 読み取り
						var supplyDateTime = new DateTime(
							int.Parse(match.Groups["Year"].Value),
							int.Parse(match.Groups["Month"].Value),
							int.Parse(match.Groups["Day"].Value),
							int.Parse(match.Groups["Hour"].Value),
							int.Parse(match.Groups["Minute"].Value),
							int.Parse(match.Groups["Second"].Value));
						int[] supplyData = {
							int.Parse(match.Groups["Fuel"].Value),
							int.Parse(match.Groups["Ammo"].Value),
							int.Parse(match.Groups["Steel"].Value),
							int.Parse(match.Groups["Bauxite"].Value),
							int.Parse(match.Groups["Diamond"].Value)};
						// データベースに入力
						for(int ti = 0; ti < MainSupplyTypeCount; ++ti) {
							MainSupplyData[ti].Add(new SupplyPair(supplyDateTime, supplyData[ti]));
						}
						if(lastUpdate < supplyDateTime) {
							lastUpdate = supplyDateTime;
						}
					} catch(Exception){
						InitialMainSupply();
						throw new Exception();
					}
				}
			}
			foreach(var supplyData in MainSupplyData){
				supplyData.Sort((a, b) => (a.Key < b.Key ? -1 : 1));
			}
			MainSupplyListCount = MainSupplyData.First().Count;
			// 最終更新日時を更新
			Properties.Settings.Default.LastUpdate = LastUpdate = lastUpdate;
			Properties.Settings.Default.Save();
		}
		// MainSupplyに追記できるかを判定する
		// (10分以上開けないと追記できない設定とした)
		public static bool CanAddMainSupply() {
			var nowTime = DateTime.Now;
			return ((nowTime - LastUpdate).TotalMinutes >= 10.0);
		}
		// MainSupplyに追記する
		public static void AddMainSupply(DateTime supplyDateTime, List<int> supply) {
			// データを書き込み
			for(int ti = 0; ti < MainSupplyTypeCount; ++ti) {
				MainSupplyData[ti].Add(new SupplyPair(supplyDateTime, supply[ti]));
			}
			++MainSupplyListCount;
			// 最終更新日時を更新
			Properties.Settings.Default.LastUpdate = LastUpdate = supplyDateTime;
			Properties.Settings.Default.Save();
		}
		// MainSupplyを表示する
		public static void ShowMainSupply() {
			Console.WriteLine("資材ログ：");
			for(int li = 0; li < MainSupplyListCount; ++li) {
				Console.Write($"{MainSupplyData.First()[li].Key}");
				for(int ti = 0; ti < MainSupplyTypeCount; ++ti) {
					Console.Write($",{MainSupplyData[ti][li].Value}");
				}
				Console.WriteLine("");
			}
		}
		// MainSupplyをCSVに保存する
		public static void SaveMainSupply() {
			using(var sw = new System.IO.StreamWriter(@"MainSupply.csv")) {
				sw.WriteLine("時刻,燃料,弾薬,鋼材,ボーキサイト,ダイヤ");
				for(int li = 0; li < MainSupplyListCount; ++li) {
					sw.Write($"{MainSupplyData.First()[li].Key}");
					for(int ti = 0; ti < MainSupplyTypeCount; ++ti) {
						sw.Write($",{MainSupplyData[ti][li].Value}");
					}
					sw.WriteLine("");
				}
			}
		}
		#endregion
		#region SubSupply関係

		#endregion
	}
}
