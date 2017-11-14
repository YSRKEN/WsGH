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

namespace AzLH {
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
			if (SupplyChart == null)
				return;
			bool IsDrawTypeMain = (ChartShowTypeComboBox.SelectedIndex == 0);
			// データが空なら描画しない
			if (IsDrawTypeMain) {
				if (SupplyStore.MainSupplyListCount == 0)
					return;
			}else if (SupplyStore.MainSupplyListCount == 0
				|| SupplyStore.SubSupplyListCount[0] == 0
				|| SupplyStore.SubSupplyListCount[1] == 0
				|| SupplyStore.SubSupplyListCount[2] == 0
				|| SupplyStore.SubSupplyListCount[3] == 0)
				return;
			// グラフの要素を消去する
			SupplyChart.Series.Clear();
			SupplyChart.Legends.Clear();
			// グラフの軸ラベルおよび罫線の色を設定する
			var chartArea = SupplyChart.ChartAreas[0];
			if (IsDrawTypeMain) {
				chartArea.AxisY.Title = Properties.Resources.SupplyChartYTitle;
				chartArea.AxisY2.Title = Properties.Resources.SupplyChartY2Title;
			}
			else {
				chartArea.AxisY.Title = Properties.Resources.SupplyChartY3Title;
				chartArea.AxisY2.Title = Properties.Resources.SupplyChartY4Title;
			}
			chartArea.AxisX.MajorGrid.LineColor = Color.LightGray;
			chartArea.AxisY.MajorGrid.LineColor = Color.LightGray;
			chartArea.AxisX2.MajorGrid.LineColor = Color.LightGray;
			chartArea.AxisY2.MajorGrid.LineColor = Color.LightGray;
			// グラフの凡例を設定する
			var SupplyChartLegends = (IsDrawTypeMain
				? new Dictionary<string, string> {
				{"Fuel", Properties.Resources.SupplyTypeFuel },
				{"Money", Properties.Resources.SupplyTypeMoney },
			} : new Dictionary<string, string> {
				{"Diamond", Properties.Resources.SupplyTypeDiamond },
				{"Cube", Properties.Resources.SupplyTypeCube },
				{"Drill", Properties.Resources.SupplyTypeDrill },
				{"Medal", Properties.Resources.SupplyTypeMedal },
				{"FurnitureCoin", Properties.Resources.SupplyTypeFurnitureCoin },
			});
			// グラフを追加する
			int index = 0;
			if (IsDrawTypeMain) {
				foreach (var data in SupplyStore.MainSupplyData) {
					if (data.Type == "Diamond")
						continue;
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
					if (data.Type == "Money") {
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
			}
			else {
				foreach (var data in SupplyStore.MainSupplyData) {
					if (data.Type != "Diamond")
						continue;
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
					if (data.Type == "FurnitureCoin") {
						series.YAxisType = AxisType.Secondary;
					}
					else {
						series.YAxisType = AxisType.Primary;
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
			}
			// グラフのスケールを設定する
			if (IsDrawTypeMain) {
				LeastChartTime = SupplyStore.MainSupplyData.Where(p => p.Type != "Diamond").Max(p => p.List.Max(d => d.Key));
			}
			else {
				var time1 = SupplyStore.MainSupplyData.Where(p => p.Type == "Diamond").Max(p => p.List.Max(d => d.Key));
				var time2 = SupplyStore.SubSupplyData.Max(p => p.List.Max(d => d.Key));
				LeastChartTime = (time1 > time2 ? time1 : time2);
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
						Money = (SupplyStore.MainSupplyData[1].List[diffIndex].Value - SupplyStore.MainSupplyData[1].List[i].Value).ToString(),
						Diamond = (SupplyStore.MainSupplyData[2].List[diffIndex].Value - SupplyStore.MainSupplyData[2].List[i].Value).ToString(),
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
			// スケールに従い縦軸を変更する
			bool IsDrawTypeMain = (ChartShowTypeComboBox.SelectedIndex == 0);
			if (IsDrawTypeMain) {
				int maximumY1 = 0;
				int maximumY2 = 0;
				foreach (var data in SupplyStore.MainSupplyData) {
					if (data.Type == "Diamond")
						continue;
					if (data.Type == "Money") {
						maximumY2 = Math.Max(maximumY2, data.List.Where(p => (LeastChartTime - p.Key).Days < chartScale).Max(p => p.Value));
					}
					else {
						maximumY1 = Math.Max(maximumY1, data.List.Where(p => (LeastChartTime - p.Key).Days < chartScale).Max(p => p.Value));
					}
				}
				if(maximumY1 != 0)
					SupplyChart.ChartAreas[0].AxisY.Maximum = SpecialCeiling(maximumY1);
				if(maximumY2 != 0)
					SupplyChart.ChartAreas[0].AxisY2.Maximum = SpecialCeiling(maximumY2);
			}
			else {
				int maximumY1 = 0;
				int maximumY2 = 0;
				foreach (var data in SupplyStore.MainSupplyData) {
					if (data.Type != "Diamond")
						continue;
					maximumY2 = Math.Max(maximumY2, data.List.Where(p => (LeastChartTime - p.Key).Days < chartScale).Max(p => p.Value));
				}
				foreach (var data in SupplyStore.SubSupplyData) {
					if (data.List.Count(p => (LeastChartTime - p.Key).Days < chartScale) == 0)
						continue;
					if (data.Type == "FurnitureCoin") {
						maximumY2 = Math.Max(maximumY2, data.List.Where(p => (LeastChartTime - p.Key).Days < chartScale).Max(p => p.Value));
					}
					else {
						maximumY1 = Math.Max(maximumY1, data.List.Where(p => (LeastChartTime - p.Key).Days < chartScale).Max(p => p.Value));
					}
				}
				if (maximumY1 != 0)
					SupplyChart.ChartAreas[0].AxisY.Maximum = SpecialCeiling(maximumY1);
				if (maximumY2 != 0)
					SupplyChart.ChartAreas[0].AxisY2.Maximum = SpecialCeiling(maximumY2);
			}
		}
		// グラフ表示に適した適当な数値に切り上げる
		private double SpecialCeiling(int x) {
			// とりあえず10で割っていく
			double x_ = x;
			double temp = 1.0;
			while(x_ >= 10.0) {
				x_ /= 10.0;
				temp *= 10.0;
			}
			// 切り上げ処理
			x_ = Math.Ceiling(x_);
			return x_ * temp;
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

		private void ChartShowTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			DrawChart();
		}
	}
	// SupplyDiffクラス
	public class SupplyDiff
	{
		public string Date { get; set; }
		public string Fuel { get; set; }
		public string Money { get; set; }
		public string Diamond { get; set; }
	}
}
