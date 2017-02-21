using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WsGH {
	static class SupplyStore {
		static DateTime lastUpdate = new DateTime();// Properties.Settings.Default.LastUpdate;
		static SupplyStoreDataContext db = new SupplyStoreDataContext(Properties.Settings.Default.SupplyStoreConnectionString);
		// MainSupplyに追記できるかを判定する
		// (10分以上開けないと追記できない設定とした)
		public static bool CanAddMainSupply() {
			var nowTime = DateTime.Now;
			return ((nowTime - lastUpdate).TotalMinutes >= 0.1);
		}
		// MainSupplyに追記する
		public static void AddMainSupply(DateTime time, List<int> supply) {
			// データを書き込み
			var supplyData = new MainSupplyTable();
			supplyData.DateTime = time;
			supplyData.Fuel    = supply[0];
			supplyData.Ammo    = supply[1];
			supplyData.Steel   = supply[2];
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
	}
}
