using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AzLH {
	using SupplyPair = KeyValuePair<DateTime, int>;
	using SupplyList = List<KeyValuePair<DateTime, int>>;
	using System.Drawing;
	static class SupplyStore {
		#region MainSupply関係
		#region メンバ変数
		// MainSupplyの最終更新日時
		static DateTime lastUpdate = new DateTime();
		// MainSupplyの本体
		public static List<SupplyData> MainSupplyData = null;
		// MainSupplyの大きさ
		public static int MainSupplyTypeCount;
		public static int MainSupplyListCount = 0;
		// MainSupplyの更新間隔
		static double MainSupplyIntervalMinute = 10.0;
		#endregion
		// MainSupplyの初期化
		static void InitialMainSupply() {
			MainSupplyData = new List<SupplyData> {
				new SupplyData("Fuel", Color.DarkGray),
				new SupplyData("Money", Color.Gold),
				new SupplyData("Diamond", Color.Red),
			};
			MainSupplyTypeCount = MainSupplyData.Count;
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
					var pattern = @"(?<Year>\d+)/(?<Month>\d+)/(?<Day>\d+) (?<Hour>\d+):(?<Minute>\d+):(?<Second>\d+),(?<Fuel>\d+),(?<Money>\d+),(?<Diamond>\d+)";
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
							int.Parse(match.Groups["Money"].Value),
							int.Parse(match.Groups["Diamond"].Value)};
						// データベースに入力
						for(int ti = 0; ti < MainSupplyTypeCount; ++ti) {
							MainSupplyData[ti].List.Add(new SupplyPair(supplyDateTime, supplyData[ti]));
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
				supplyData.List.Sort((a, b) => (a.Key < b.Key ? -1 : 1));
			}
			MainSupplyListCount = MainSupplyData.First().List.Count;
			// 最終更新日時を更新
			SupplyStore.lastUpdate = lastUpdate;
		}
		// MainSupplyに追記できるかを判定する
		// (MainSupplyIntervalMinute分以上開けないと追記できない設定とした)
		public static bool CanAddMainSupply() {
			var nowTime = DateTime.Now;
			return ((nowTime - lastUpdate).TotalMinutes >= MainSupplyIntervalMinute);
		}
		// MainSupplyに追記する
		public static void AddMainSupply(DateTime supplyDateTime, List<int> supply) {
			// データを書き込み
			for(int ti = 0; ti < MainSupplyTypeCount; ++ti) {
				MainSupplyData[ti].List.Add(new SupplyPair(supplyDateTime, supply[ti]));
			}
			++MainSupplyListCount;
			// 最終更新日時を更新
			lastUpdate = supplyDateTime;
		}
		// MainSupplyを表示する
		public static void ShowMainSupply() {
			Console.WriteLine("資材ログ：");
			for(int li = 0; li < MainSupplyListCount; ++li) {
				Console.Write($"{MainSupplyData.First().List[li].Key}");
				for(int ti = 0; ti < MainSupplyTypeCount; ++ti) {
					Console.Write($",{MainSupplyData[ti].List[li].Value}");
				}
				Console.WriteLine("");
			}
		}
		// MainSupplyをCSVに保存する
		public static void SaveMainSupply() {
			using(var sw = new System.IO.StreamWriter(@"MainSupply.csv")) {
				sw.WriteLine("時刻,燃料,資金,ダイヤ");
				for(int li = 0; li < MainSupplyListCount; ++li) {
					sw.Write($"{MainSupplyData.First().List[li].Key}");
					for(int ti = 0; ti < MainSupplyTypeCount; ++ti) {
						sw.Write($",{MainSupplyData[ti].List[li].Value}");
					}
					sw.WriteLine("");
				}
			}
		}
		#endregion
		#region SubSupply 関係
		#region メンバ変数
		// SubSupply の種類数
		public static int SubSupplyTypes = 4;
		// SubSupply の最終更新日時
		static DateTime[] lastUpdateSub;
		// SubSupply の本体
		public static List<SupplyData> SubSupplyData = null;
		// SubSupply の大きさ
		public static int SubSupplyTypeCount;
		public static int[] SubSupplyListCount;
		// SubSupply の更新間隔
		static double SubSupplyIntervalMinute = 60.0;
		#endregion
		// SubSupplyの初期化
		static void InitialSubSupply() {
			lastUpdateSub = new DateTime[] { new DateTime(), new DateTime(), new DateTime(), new DateTime(), };
			SubSupplyData = new List<SupplyData> {
				new SupplyData("Cube", Color.SkyBlue),
				new SupplyData("Drill", Color.Gold),
				new SupplyData("Medal", Color.BlueViolet),
				new SupplyData("FurnitureCoin", Color.OrangeRed),
			};
			SubSupplyTypeCount = SubSupplyData.Count;
			SubSupplyListCount = new int[] { 0,0,0,0 };
		}
		// SubSupplyをCSVから読み込む
		public static void ReadSubSupply() {
			InitialSubSupply();
			// SubSupplyの種類毎にファイル名が違うので
			for (int ti = 0; ti < SubSupplyTypes; ++ti) {
				// CSVから読み込む
				var lastUpdate = new DateTime();
				using (var sr = new System.IO.StreamReader($"SubSupply{(ti + 1)}.csv")) {
					while (!sr.EndOfStream) {
						// 1行を読み込む
						var line = sr.ReadLine();
						// マッチさせてから各数値を取り出す
						var pattern = @"(?<Year>\d+)/(?<Month>\d+)/(?<Day>\d+) (?<Hour>\d+):(?<Minute>\d+):(?<Second>\d+),(?<Supply>\d+)";
						var match = Regex.Match(line, pattern);
						if (!match.Success) {
							continue;
						}
						// 取り出した数値を元に、SubSupplyDataに入力する
						try {
							// 読み取り
							var supplyDateTime = new DateTime(
								int.Parse(match.Groups["Year"].Value),
								int.Parse(match.Groups["Month"].Value),
								int.Parse(match.Groups["Day"].Value),
								int.Parse(match.Groups["Hour"].Value),
								int.Parse(match.Groups["Minute"].Value),
								int.Parse(match.Groups["Second"].Value));
							int supplyData = int.Parse(match.Groups["Supply"].Value);
							// データベースに入力
							SubSupplyData[ti].List.Add(new SupplyPair(supplyDateTime, supplyData));
							if (lastUpdate < supplyDateTime) {
								lastUpdate = supplyDateTime;
							}
						}
						catch (Exception) {
							InitialSubSupply();
							throw new Exception();
						}
					}
				}
				SubSupplyData[ti].List.Sort((a, b) => (a.Key < b.Key ? -1 : 1));
				SubSupplyListCount[ti] = SubSupplyData[ti].List.Count;
				// 最終更新日時を更新
				SupplyStore.lastUpdateSub[ti] = lastUpdate;
			}
		}
		// SubSupplyに追記できるかを判定する
		public static bool CanAddSubSupply(int ti) {
			var nowTime = DateTime.Now;
			return ((nowTime - lastUpdateSub[ti]).TotalMinutes >= SubSupplyIntervalMinute);
		}
		// SubSupplyに追記する
		public static void AddSubSupply(int ti, DateTime supplyDateTime, int supply) {
			// データを書き込み
			SubSupplyData[ti].List.Add(new SupplyPair(supplyDateTime, supply));
			++SubSupplyListCount[ti];
			// 最終更新日時を更新
			lastUpdateSub[ti] = supplyDateTime;
		}
		// SubSupplyを表示する
		public static void ShowSubSupply(int ti) {
			Console.WriteLine($"特殊資材ログ[{SubSupplyData[ti].Type}]：");
			for (int li = 0; li < SubSupplyListCount[ti]; ++li) {
				Console.Write($"{SubSupplyData[ti].List[li].Key}");
				Console.Write($",{SubSupplyData[ti].List[li].Value}");
				Console.WriteLine("");
			}
		}
		// SubSupplyをCSVに保存する
		public static void SaveSubSupply(int ti) {
			using (var sw = new System.IO.StreamWriter($"SubSupply{(ti + 1)}.csv")) {
				sw.WriteLine("時刻,資材量");
				for (int li = 0; li < SubSupplyListCount[ti]; ++li) {
					sw.Write($"{SubSupplyData[ti].List[li].Key}");
					sw.Write($",{SubSupplyData[ti].List[li].Value}");
					sw.WriteLine("");
				}
			}
		}
		#endregion
		#region 内部クラス
		public class SupplyData {
			// 時系列データ
			public SupplyList List;
			// 種別
			public string Type;
			// グラフの描画色
			public Color Color;
			// コンストラクタ
			public SupplyData(string type, Color color) {
				List = new SupplyList();
				Type = type;
				Color = color;
			}
		}
		#endregion
	}
}
