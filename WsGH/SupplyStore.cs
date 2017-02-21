using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WsGH {
	static class SupplyStore {
		static DateTime lastUpdate = new DateTime();
		// MainSupplyに追記できるかを判定する
		// (10分以上開けないと追記できない設定とした)
		public static bool CanAddMainSupply() {
			var nowTime = DateTime.Now;
			return ((lastUpdate - nowTime).TotalMinutes >= 10.0);
		}
	}
}
