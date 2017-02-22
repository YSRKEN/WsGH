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
	public partial class SupplyWindow : Window {
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
			SupplyChart.Series.Clear();
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
				// SupplyChartに追加する
				SupplyChart.Series.Add(series);
			}
			//SupplyChart.ChartAreas[0].AxisX.Minimum = 0;
			//SupplyChart.ChartAreas[0].AxisX.Maximum = 10000;
			//SupplyChart.ChartAreas[0].AxisY.Minimum = 0;
			//SupplyChart.ChartAreas[0].AxisY.Maximum = 10000;
		}
	}
}
