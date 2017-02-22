using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WsGH {
	static class SupplyStore {
//		static DateTime lastUpdate = new DateTime();
		static DateTime lastUpdate = Properties.Settings.Default.LastUpdate;
		static SupplyStoreDataContext db = new SupplyStoreDataContext(Properties.Settings.Default.SupplyStoreConnectionString);
		#region 全体
		// 描画用のデータを書き出す
		public static Dictionary<string, List<KeyValuePair<DateTime, int>>> MakeChartData() {
			// 中間データをListに書き出す
			var listFuel    = new List<KeyValuePair<DateTime, int>>();
			var listAmmo    = new List<KeyValuePair<DateTime, int>>();
			var listSteel   = new List<KeyValuePair<DateTime, int>>();
			var listBauxite = new List<KeyValuePair<DateTime, int>>();
			var listDiamond = new List<KeyValuePair<DateTime, int>>();
			foreach(var supplyData in db.MainSupplyTable) {
				listFuel.Add(new KeyValuePair<DateTime, int>(supplyData.DateTime, supplyData.Fuel));
				listAmmo.Add(new KeyValuePair<DateTime, int>(supplyData.DateTime, supplyData.Ammo));
				listSteel.Add(new KeyValuePair<DateTime, int>(supplyData.DateTime, supplyData.Steel));
				listBauxite.Add(new KeyValuePair<DateTime, int>(supplyData.DateTime, supplyData.Bauxite));
				listDiamond.Add(new KeyValuePair<DateTime, int>(supplyData.DateTime, supplyData.Diamond));
			}
			// Listをソート
			listFuel.Sort((a, b) => (a.Key < b.Key ? -1 : 1));
			listAmmo.Sort((a, b) => (a.Key < b.Key ? -1 : 1));
			listSteel.Sort((a, b) => (a.Key < b.Key ? -1 : 1));
			listBauxite.Sort((a, b) => (a.Key < b.Key ? -1 : 1));
			listDiamond.Sort((a, b) => (a.Key < b.Key ? -1 : 1));
			// 出力データにAddで書き足す
			var output = new Dictionary<string, List<KeyValuePair<DateTime, int>>>();
			output.Add(Properties.Resources.SupplyTypeFuel, listFuel);
			output.Add(Properties.Resources.SupplyTypeAmmo, listAmmo);
			output.Add(Properties.Resources.SupplyTypeSteel, listSteel);
			output.Add(Properties.Resources.SupplyTypeBauxite, listBauxite);
			output.Add(Properties.Resources.SupplyTypeDiamond, listDiamond);
			return output;
		}
		#endregion
		#region MainSupply関係
		// MainSupplyをCSVから読み込む
		public static void ReadMainSupply() {
			var time = lastUpdate;
			using(var sr = new System.IO.StreamReader(@"MainSupply.csv")) {
				int index = 0;
				while(!sr.EndOfStream) {
					// 1行を読み込む
					var line = sr.ReadLine();
					// マッチさせてから各数値を取り出す
					var pattern = @"(?<Year>\d+)/(?<Month>\d+)/(?<Day>\d+) (?<Hour>\d+):(?<Minute>\d+):(?<Second>\d+),(?<Fuel>\d+),(?<Ammo>\d+),(?<Steel>\d+),(?<Bauxite>\d+),(?<Diamond>\d+)";
					var match = Regex.Match(line, pattern);
					if(!match.Success) {
						continue;
					}
					// 取り出した数値を元に、MainSupplyに入力する
					var supplyData = new MainSupplyTable();
					supplyData.Id = index;
					supplyData.DateTime = new DateTime(
						int.Parse(match.Groups["Year"].Value),
						int.Parse(match.Groups["Month"].Value),
						int.Parse(match.Groups["Day"].Value),
						int.Parse(match.Groups["Hour"].Value),
						int.Parse(match.Groups["Minute"].Value),
						int.Parse(match.Groups["Second"].Value));
					supplyData.Fuel = int.Parse(match.Groups["Fuel"].Value);
					supplyData.Ammo = int.Parse(match.Groups["Ammo"].Value);
					supplyData.Steel = int.Parse(match.Groups["Steel"].Value);
					supplyData.Bauxite = int.Parse(match.Groups["Bauxite"].Value);
					supplyData.Diamond = int.Parse(match.Groups["Diamond"].Value);
					db.MainSupplyTable.InsertOnSubmit(supplyData);
					if(time < supplyData.DateTime) {
						time = supplyData.DateTime;
					}
					++index;
				}
			}
			db.SubmitChanges();
			// 最終更新日時を更新
			lastUpdate = time;
			Properties.Settings.Default.LastUpdate = time;
			Properties.Settings.Default.Save();
		}
		// MainSupplyに追記できるかを判定する
		// (10分以上開けないと追記できない設定とした)
		public static bool CanAddMainSupply() {
			var nowTime = DateTime.Now;
//			return ((nowTime - lastUpdate).TotalMinutes >= 0.1);
			return ((nowTime - lastUpdate).TotalMinutes >= 10.0);
		}
		// MainSupplyに追記する
		public static void AddMainSupply(DateTime time, List<int> supply) {
			// データを書き込み
			var supplyData = new MainSupplyTable();
			supplyData.Id = db.MainSupplyTable.Count();
			supplyData.DateTime = time;
			supplyData.Fuel = supply[0];
			supplyData.Ammo = supply[1];
			supplyData.Steel = supply[2];
			supplyData.Bauxite = supply[3];
			supplyData.Diamond = supply[4];
			db.MainSupplyTable.InsertOnSubmit(supplyData);
			db.SubmitChanges();
			// 最終更新日時を更新
			lastUpdate = time;
			Properties.Settings.Default.LastUpdate = time;
			Properties.Settings.Default.Save();
		}
		// MainSupplyを表示する
		public static void ShowMainSupply() {
			Console.WriteLine("資材ログ：");
			foreach(var supplyData in db.MainSupplyTable) {
				Console.WriteLine($"{supplyData.DateTime},{supplyData.Fuel},{supplyData.Ammo},{supplyData.Steel},{supplyData.Bauxite},{supplyData.Diamond}");
			}
		}
		// MainSupplyをCSVに保存する
		public static void SaveMainSupply() {
			using(var sw = new System.IO.StreamWriter(@"MainSupply.csv")) {
				sw.WriteLine("時刻,燃料,弾薬,鋼材,ボーキサイト,ダイヤ");
				foreach(var supplyData in db.MainSupplyTable) {
					sw.WriteLine($"{supplyData.DateTime.ToString("yyyy/MM/dd HH:mm:ss")},{supplyData.Fuel},{supplyData.Ammo},{supplyData.Steel},{supplyData.Bauxite},{supplyData.Diamond}");
				}
			}
		}
		#endregion
		#region SubSupply関係

		#endregion
	}
}
