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
	}
}
