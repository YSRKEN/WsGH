using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WsGH {
	/// <summary>
	/// SupplyWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class SupplyWindow : Window {
		static int[] ChartScale = {1, 7, 14, 30, 60, 90, 180, 365};
		static DateTime LeastChartTime;
		ObservableCollection<SupplyDiff> SupplyDiffList;
		// コンストラクタ
		public SupplyWindow() {
			InitializeComponent();
			MouseLeftButtonDown += (o, e) => DragMove();
			SupplyDiffList = new ObservableCollection<SupplyDiff>();
			SupplyDiffListView.DataContext = SupplyDiffList;
			DrawChart();
		}
		// ウィンドウ位置復元
		protected override void OnSourceInitialized(EventArgs e) {
			base.OnSourceInitialized(e);
			try {
				NativeMethods.SetWindowPlacementHelper(this, Properties.Settings.Default.SupplyWindowPlacement);
			} catch { }
		}
		// ウィンドウ位置保存
		protected override void OnClosing(CancelEventArgs e) {
			base.OnClosing(e);
			// メインウィンドウが閉じられたことによって閉じる場合でなければ、
			// ShowSupplyWindowFlgをfalseにする
			if(Application.Current.MainWindow != null) {
				Properties.Settings.Default.ShowSupplyWindowFlg = false;
			}
			// TimerWindowの位置を保存する
			Properties.Settings.Default.SupplyWindowPlacement = NativeMethods.GetWindowPlacementHelper(this);
			Properties.Settings.Default.Save();
		}
		// 与えられたデータからグラフを描画する
		public void DrawChart() {
			// データが空なら描画しない
			if(SupplyStore.MainSupplyListCount == 0)
				return;
			// グラフの要素を消去する
			SupplyChart.Series.Clear();
			SupplyChart.Legends.Clear();
			// グラフの軸ラベルおよび罫線の色を設定する
			var chartArea = SupplyChart.ChartAreas[0];
			chartArea.AxisY.Title = Properties.Resources.SupplyChartYTitle;
			chartArea.AxisY2.Title = Properties.Resources.SupplyChartY2Title;
			chartArea.AxisX.MajorGrid.LineColor = Color.LightGray;
			chartArea.AxisY.MajorGrid.LineColor = Color.LightGray;
			chartArea.AxisX2.MajorGrid.LineColor = Color.LightGray;
			chartArea.AxisY2.MajorGrid.LineColor = Color.LightGray;
			// グラフの凡例を設定する
			var SupplyChartLegends = new Dictionary<string, string> {
				{"Fuel", Properties.Resources.SupplyTypeFuel },
				{"Ammo", Properties.Resources.SupplyTypeAmmo },
				{"Steel", Properties.Resources.SupplyTypeSteel },
				{"Bauxite", Properties.Resources.SupplyTypeBauxite },
				{"Diamond", Properties.Resources.SupplyTypeDiamond },
				{"Bucket", Properties.Resources.SupplyTypeBucket },
				{"Burner", Properties.Resources.SupplyTypeBurner },
				{"ShipBlueprint", Properties.Resources.SupplyTypeShipBlueprint },
				{"WeaponBlueprint", Properties.Resources.SupplyTypeWeaponBlueprint },
			};
			// グラフを追加する
			int index = 0;
			foreach(var data in SupplyStore.MainSupplyData) {
				var series = new Series();
				// 名前を設定する
				series.Name = SupplyChartLegends[data.Type];
				// 折れ線グラフに設定する
				series.ChartType = SeriesChartType.Line;
				// 横軸を「時間」とする
				series.XValueType = ChartValueType.DateTime;
				// 表示用データを追加する
				foreach(var Column in data.List) {
					series.Points.AddXY(Column.Key.ToOADate(), Column.Value);
				}
				// 表示位置を調整
				if(data.Type == "Diamond") {
					series.YAxisType = AxisType.Secondary;
				}
				// 表示色を選択
				series.Color = data.Color;
				series.BorderWidth = 2;
				// SupplyChartに追加する
				SupplyChart.Series.Add(series);
				// 凡例の設定
				var legend = new Legend();
				legend.DockedToChartArea = "ChartArea";
				legend.Alignment = StringAlignment.Far;
				SupplyChart.Legends.Add(legend);
				// インクリメント
				++index;
			}
			foreach (var data in SupplyStore.SubSupplyData) {
				var series = new Series();
				// 名前を設定する
				series.Name = SupplyChartLegends[data.Type];
				// 折れ線グラフに設定する
				series.ChartType = SeriesChartType.Line;
				// 横軸を「時間」とする
				series.XValueType = ChartValueType.DateTime;
				// 表示用データを追加する
				foreach (var Column in data.List) {
					series.Points.AddXY(Column.Key.ToOADate(), Column.Value);
				}
				// 表示位置を調整
				series.YAxisType = AxisType.Secondary;
				// 表示色を選択
				series.Color = data.Color;
				series.BorderWidth = 2;
				// SupplyChartに追加する
				SupplyChart.Series.Add(series);
				// 凡例の設定
				var legend = new Legend();
				legend.DockedToChartArea = "ChartArea";
				legend.Alignment = StringAlignment.Far;
				SupplyChart.Legends.Add(legend);
				// インクリメント
				++index;
			}
			// グラフのスケールを設定する
			var leastChartTime1 = SupplyStore.MainSupplyData.Max(p => p.List.Max(d => d.Key));
			try {
				var leastChartTime2 = SupplyStore.SubSupplyData.Max(p => p.List.Max(d => d.Key));
				LeastChartTime = (leastChartTime1 < leastChartTime2 ? leastChartTime2 : leastChartTime1);
			}
			catch {
				LeastChartTime = leastChartTime1;
			}
			chartArea.AxisX.Maximum = LeastChartTime.ToOADate();
			ChangeChartScale();
			// ListViewを再描画する
			DrawListView();
		}
		// 与えられたデータから増分値を描画する
		void DrawListView() {
			// データが空なら描画しない
			if (SupplyStore.MainSupplyListCount == 0)
				return;
			// 描画分を消去する
			SupplyDiffList.Clear();
			// 順次追加していく
			int diffIndex = SupplyStore.MainSupplyListCount - 1;
			var alias = SupplyStore.MainSupplyData.First();	//冗長部分を一時的に略す
			for (int i = SupplyStore.MainSupplyListCount - 2; i >= 0; --i) {
				var diffIndexDate = alias.List[diffIndex].Key.Date;
				var newDate = alias.List[i].Key.Date;
				if (diffIndexDate != newDate) {
					var sd = new SupplyDiff() {
						Date = diffIndexDate.ToString("MM/dd"),
						Fuel = (SupplyStore.MainSupplyData[0].List[diffIndex].Value - SupplyStore.MainSupplyData[0].List[i].Value).ToString(),
						Ammo = (SupplyStore.MainSupplyData[1].List[diffIndex].Value - SupplyStore.MainSupplyData[1].List[i].Value).ToString(),
						Steel = (SupplyStore.MainSupplyData[2].List[diffIndex].Value - SupplyStore.MainSupplyData[2].List[i].Value).ToString(),
						Bauxite = (SupplyStore.MainSupplyData[3].List[diffIndex].Value - SupplyStore.MainSupplyData[3].List[i].Value).ToString(),
						Diamond = (SupplyStore.MainSupplyData[4].List[diffIndex].Value - SupplyStore.MainSupplyData[4].List[i].Value).ToString(),
					};
					SupplyDiffList.Add(sd);
					diffIndex = i;
				}
			}
			return;
		}
		// グラフのスケールを変更する
		private void ChartScaleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			ChangeChartScale();
		}

		void ChangeChartScale() {
			if(SupplyChart == null)
				return;
			// グラフののスケールを決定する(デフォルト値は「2週間」)
			var index = ChartScaleComboBox.SelectedIndex;
			var chartScale = ChartScale[(index != -1 ? index : 2)];
			// スケールに従い横軸を変更する
			var axisX = SupplyChart.ChartAreas[0].AxisX;
			axisX.Minimum = axisX.Maximum - chartScale;
		}

		// 資材データを再読込みする
		private void ReloadSupplyDataButton_Click(object sender, RoutedEventArgs e) {
			try {
				SupplyStore.ReadMainSupply();
				SupplyStore.ReadSubSupply();
				DrawChart();
			}
			catch (Exception) {}
		}
	}
	// SupplyDiffクラス
	public class SupplyDiff
	{
		public string Date { get; set; }
		public string Fuel { get; set; }
		public string Ammo { get; set; }
		public string Steel { get; set; }
		public string Bauxite { get; set; }
		public string Diamond { get; set; }
	}
}
