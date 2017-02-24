using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WsGH {
	/// <summary>
	/// SupplyWindow.xaml の相互作用ロジック
	/// </summary>
	using dColor = System.Drawing.Color;
	public partial class SupplyWindow : Window {
		static int[] ChartScale = {1, 7, 14, 30, 60, 90, 180, 365};
		static DateTime LeastChartTime;
		// コンストラクタ
		public SupplyWindow() {
			InitializeComponent();
			MouseLeftButtonDown += (o, e) => DragMove();
			DrawChart(SupplyStore.MakeChartData());
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
		public void DrawChart(Dictionary<string, List<KeyValuePair<DateTime, int>>> ChartData) {
			if(ChartData.First().Value.Count() == 0)
				return;
			// グラフの要素を消去する
			SupplyChart.Series.Clear();
			SupplyChart.Legends.Clear();
			// グラフの軸ラベルおよび罫線の色を設定する
			SupplyChart.ChartAreas[0].AxisY.Title = Properties.Resources.SupplyChartYTitle;
			SupplyChart.ChartAreas[0].AxisY2.Title = Properties.Resources.SupplyChartY2Title;
			SupplyChart.ChartAreas[0].AxisX.MajorGrid.LineColor = dColor.LightGray;
			SupplyChart.ChartAreas[0].AxisY.MajorGrid.LineColor = dColor.LightGray;
			SupplyChart.ChartAreas[0].AxisX2.MajorGrid.LineColor = dColor.LightGray;
			SupplyChart.ChartAreas[0].AxisY2.MajorGrid.LineColor = dColor.LightGray;
			// グラフを描画する際の色を設定する
			var SupplyChartColor = new Dictionary<string, dColor> {
				{"Fuel", dColor.Green},
				{"Ammo", dColor.Chocolate},
				{"Steel", dColor.DarkGray},
				{"Bauxite", dColor.OrangeRed},
				{"Diamond", dColor.SkyBlue},
			};
			// グラフを追加する
			foreach(var data in ChartData) {
				var series = new Series();
				// 名前を設定する
				series.Name = data.Key;
				// 折れ線グラフに設定する
				series.ChartType = SeriesChartType.Line;
				// 横軸を「時間」とする
				series.XValueType = ChartValueType.DateTime;
				// 表示用データを追加する
				foreach(var Column in data.Value) {
					series.Points.AddXY(Column.Key.ToOADate(), Column.Value);
				}
				// 表示位置を調整
				if(data.Key == "Diamond") {
					series.YAxisType = AxisType.Secondary;
				}
				// 表示色を選択
				series.Color = SupplyChartColor[data.Key];
				series.BorderWidth = 2;
				// SupplyChartに追加する
				SupplyChart.Series.Add(series);
				// 凡例の設定
				var legend = new Legend();
				legend.DockedToChartArea = "ChartArea";
				legend.Alignment = System.Drawing.StringAlignment.Far;
				SupplyChart.Legends.Add(legend);
			}
			// グラフのスケールを設定する
			LeastChartTime = ChartData.First().Value.Max(d => d.Key);
			ChangeChartScale();
		}
		// グラフのスケールを変更する
		private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			ChangeChartScale();
		}
		void ChangeChartScale() {
			var chartScale = ChartScale[(ChartScaleComboBox.SelectedIndex != -1 ? ChartScaleComboBox.SelectedIndex : 2)];
			SupplyChart.ChartAreas[0].AxisX.Maximum = LeastChartTime.ToOADate();
			SupplyChart.ChartAreas[0].AxisX.Minimum = LeastChartTime.ToOADate() - chartScale;
		}
	}
}
