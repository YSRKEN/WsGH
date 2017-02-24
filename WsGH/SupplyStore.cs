using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WsGH {
	using SupplyPair = KeyValuePair<DateTime, int>;
	static class SupplyStore {
		static DateTime lastUpdate = Properties.Settings.Default.LastUpdate;
		static double MainSupplyIntervalMinute = 60.0;
		static Dictionary<string, List<SupplyPair>> MainSupply;
		#region 全体
		// データベースを初期化する
		static void InitialMainSupply() {
			MainSupply = new Dictionary<string, List<SupplyPair>>();
			MainSupply["Fuel"] = new List<SupplyPair>();
			MainSupply["Ammo"] = new List<SupplyPair>();
			MainSupply["Steel"] = new List<SupplyPair>();
			MainSupply["Bauxite"] = new List<SupplyPair>();
			MainSupply["Diamond"] = new List<SupplyPair>();
		}
		// 描画用のデータを書き出す
		public static Dictionary<string, List<SupplyPair>> MakeChartData() {
			return MainSupply;
		}
		#endregion
		#region MainSupply関係
		// MainSupplyをCSVから読み込む
		public static void ReadMainSupply() {
			var time = lastUpdate;
			// データベースを初期化
			InitialMainSupply();
			// CSVから読み込む
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
						var supplyFuel = int.Parse(match.Groups["Fuel"].Value);
						var supplyAmmo = int.Parse(match.Groups["Ammo"].Value);
						var supplySteel = int.Parse(match.Groups["Steel"].Value);
						var supplyBauxite = int.Parse(match.Groups["Bauxite"].Value);
						var supplyDiamond = int.Parse(match.Groups["Diamond"].Value);
						// データベースに入力
						MainSupply["Fuel"].Add(new SupplyPair(supplyDateTime, supplyFuel));
						MainSupply["Ammo"].Add(new SupplyPair(supplyDateTime, supplyAmmo));
						MainSupply["Steel"].Add(new SupplyPair(supplyDateTime, supplySteel));
						MainSupply["Bauxite"].Add(new SupplyPair(supplyDateTime, supplyBauxite));
						MainSupply["Diamond"].Add(new SupplyPair(supplyDateTime, supplyDiamond));
						if(time < supplyDateTime) {
							time = supplyDateTime;
						}
					} catch(Exception){
						InitialMainSupply();
						throw new Exception();
					}
				}
			}
			MainSupply["Fuel"].Sort((a, b) => (a.Key < b.Key ? -1 : 1));
			MainSupply["Ammo"].Sort((a, b) => (a.Key < b.Key ? -1 : 1));
			MainSupply["Steel"].Sort((a, b) => (a.Key < b.Key ? -1 : 1));
			MainSupply["Bauxite"].Sort((a, b) => (a.Key < b.Key ? -1 : 1));
			MainSupply["Diamond"].Sort((a, b) => (a.Key < b.Key ? -1 : 1));
			// 最終更新日時を更新
			lastUpdate = time;
			Properties.Settings.Default.LastUpdate = time;
			Properties.Settings.Default.Save();
		}
		// MainSupplyに追記できるかを判定する
		// (MainSupplyIntervalMinute分以上開けないと追記できない設定とした)
		public static bool CanAddMainSupply() {
			var nowTime = DateTime.Now;
			return ((nowTime - lastUpdate).TotalMinutes >= MainSupplyIntervalMinute);
		}
		// MainSupplyに追記する
		public static void AddMainSupply(DateTime time, List<int> supply) {
			// データを書き込み
			MainSupply["Fuel"].Add(new SupplyPair(time, supply[0]));
			MainSupply["Ammo"].Add(new SupplyPair(time, supply[1]));
			MainSupply["Steel"].Add(new SupplyPair(time, supply[2]));
			MainSupply["Bauxite"].Add(new SupplyPair(time, supply[3]));
			MainSupply["Diamond"].Add(new SupplyPair(time, supply[4]));
			// 最終更新日時を更新
			lastUpdate = time;
			Properties.Settings.Default.LastUpdate = time;
			Properties.Settings.Default.Save();
		}
		// MainSupplyを表示する
		public static void ShowMainSupply() {
			Console.WriteLine("資材ログ：");
			var count = MainSupply.First().Value.Count;
			for(int i = 0; i < count; ++i) {
				Console.WriteLine($"{MainSupply.First().Value[i].Key},{MainSupply["Fuel"][i].Value},{MainSupply["Ammo"][i].Value},{MainSupply["Steel"][i].Value},{MainSupply["Bauxite"][i].Value},{MainSupply["Diamond"][i].Value}");
			}
		}
		// MainSupplyをCSVに保存する
		public static void SaveMainSupply() {
			using(var sw = new System.IO.StreamWriter(@"MainSupply.csv")) {
				sw.WriteLine("時刻,燃料,弾薬,鋼材,ボーキサイト,ダイヤ");
				var count = MainSupply.First().Value.Count;
				for(int i = 0; i < count; ++i) {
					sw.WriteLine($"{MainSupply.First().Value[i].Key.ToString("yyyy/MM/dd HH:mm:ss")},{MainSupply["Fuel"][i].Value},{MainSupply["Ammo"][i].Value},{MainSupply["Steel"][i].Value},{MainSupply["Bauxite"][i].Value},{MainSupply["Diamond"][i].Value}");
				}
			}
		}
		#endregion
		#region SubSupply関係

		#endregion
	}
}
