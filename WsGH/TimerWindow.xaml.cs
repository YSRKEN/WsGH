using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
	/// TimerWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class TimerWindow : Window {
		// コンストラクタ
		public TimerWindow() {
			InitializeComponent();
			MouseLeftButtonDown += (o, e) => DragMove();
			DataContext = new TimerValue() {
				ExpTimer1 = Properties.Settings.Default.ExpTimer1,
				ExpTimer2 = Properties.Settings.Default.ExpTimer2,
				ExpTimer3 = Properties.Settings.Default.ExpTimer3,
				ExpTimer4 = Properties.Settings.Default.ExpTimer4,
				BuildTimer1 = Properties.Settings.Default.BuildTimer1,
				BuildTimer2 = Properties.Settings.Default.BuildTimer2,
				BuildTimer3 = Properties.Settings.Default.BuildTimer3,
				BuildTimer4 = Properties.Settings.Default.BuildTimer4,
				DevTimer1 = Properties.Settings.Default.DevTimer1,
				DevTimer2 = Properties.Settings.Default.DevTimer2,
				DevTimer3 = Properties.Settings.Default.DevTimer3,
				DevTimer4 = Properties.Settings.Default.DevTimer4,
				DockTimer1 = Properties.Settings.Default.DockTimer1,
				DockTimer2 = Properties.Settings.Default.DockTimer2,
				DockTimer3 = Properties.Settings.Default.DockTimer3,
				DockTimer4 = Properties.Settings.Default.DockTimer4,
			};
		}
		// ウィンドウ位置復元
		protected override void OnSourceInitialized(EventArgs e) {
			base.OnSourceInitialized(e);
			try {
				NativeMethods.SetWindowPlacementHelper(this, Properties.Settings.Default.TimerWindowPlacement);
			} catch { }
		}
		// ウィンドウ位置保存
		protected override void OnClosing(CancelEventArgs e) {
			base.OnClosing(e);
			// メインウィンドウが閉じられたことによって閉じる場合でなければ、
			// ShowTimerWindowFlgをfalseにする
			if(Application.Current.MainWindow != null) {
				Properties.Settings.Default.ShowTimerWindowFlg = false;
			}
			// TimerWindowの位置を保存する
			Properties.Settings.Default.TimerWindowPlacement = NativeMethods.GetWindowPlacementHelper(this);
			Properties.Settings.Default.Save();
		}

		private void ExpTimer1TextBlock_MouseDown(object sender, MouseButtonEventArgs e) {
			var bindData = DataContext as TimerValue;
			bindData.ExpTimer1 = 0;
		}
		private void ExpTimer2TextBlock_MouseDown(object sender, MouseButtonEventArgs e) {
			var bindData = DataContext as TimerValue;
			bindData.ExpTimer2 = 0;
		}
		private void ExpTimer3TextBlock_MouseDown(object sender, MouseButtonEventArgs e) {
			var bindData = DataContext as TimerValue;
			bindData.ExpTimer3 = 0;
		}
		private void ExpTimer4TextBlock_MouseDown(object sender, MouseButtonEventArgs e) {
			var bindData = DataContext as TimerValue;
			bindData.ExpTimer4 = 0;
		}
		private void BuildTimer1TextBlock_MouseDown(object sender, MouseButtonEventArgs e) {
			var bindData = DataContext as TimerValue;
			bindData.BuildTimer1 = 0;
		}
		private void BuildTimer2TextBlock_MouseDown(object sender, MouseButtonEventArgs e) {
			var bindData = DataContext as TimerValue;
			bindData.BuildTimer2 = 0;
		}
		private void BuildTimer3TextBlock_MouseDown(object sender, MouseButtonEventArgs e) {
			var bindData = DataContext as TimerValue;
			bindData.BuildTimer3 = 0;
		}
		private void BuildTimer4TextBlock_MouseDown(object sender, MouseButtonEventArgs e) {
			var bindData = DataContext as TimerValue;
			bindData.BuildTimer4 = 0;
		}
		private void DevTimer1TextBlock_MouseDown(object sender, MouseButtonEventArgs e) {
			var bindData = DataContext as TimerValue;
			bindData.DevTimer1 = 0;
		}
		private void DevTimer2TextBlock_MouseDown(object sender, MouseButtonEventArgs e) {
			var bindData = DataContext as TimerValue;
			bindData.DevTimer2 = 0;
		}
		private void DevTimer3TextBlock_MouseDown(object sender, MouseButtonEventArgs e) {
			var bindData = DataContext as TimerValue;
			bindData.DevTimer3 = 0;
		}
		private void DevTimer4TextBlock_MouseDown(object sender, MouseButtonEventArgs e) {
			var bindData = DataContext as TimerValue;
			bindData.DevTimer4 = 0;
		}
		private void DockTimer1TextBlock_MouseDown(object sender, MouseButtonEventArgs e) {
			var bindData = DataContext as TimerValue;
			bindData.DockTimer1 = 0;
		}
		private void DockTimer2TextBlock_MouseDown(object sender, MouseButtonEventArgs e) {
			var bindData = DataContext as TimerValue;
			bindData.DockTimer2 = 0;
		}
		private void DockTimer3TextBlock_MouseDown(object sender, MouseButtonEventArgs e) {
			var bindData = DataContext as TimerValue;
			bindData.DockTimer3 = 0;
		}
		private void DockTimer4TextBlock_MouseDown(object sender, MouseButtonEventArgs e) {
			var bindData = DataContext as TimerValue;
			bindData.DockTimer4 = 0;
		}
	}
	// コンバータ
	public class TimerConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			// 完了時刻を読み取る
			var time = (ulong)value;
			// 現在時刻を読み取る
			var now_time = SceneRecognition.GetUnixTime(DateTime.Now);
			if(time <= now_time) {
				// 現在時刻≧完了時刻だと--:--:--になる
				return "--:--:--";
			} else {
				// 時間差を計算して画面に表示する
				var leastSecond = time - now_time;
				var hour = leastSecond / (60 * 60);
				leastSecond -= hour * 60 * 60;
				var minute = leastSecond / 60;
				leastSecond -= minute * 60;
				var second = leastSecond;
				return hour.ToString("D2") + ":" + minute.ToString("D2") + ":" + second.ToString("D2");
			}
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
	// 値を保存するクラス
	public class TimerValue : INotifyPropertyChanged
	{
		#region 遠征タイマー
		ulong expTimer1;
		public ulong ExpTimer1 {
			get { return expTimer1; }
			set {
				expTimer1 = value;
				Properties.Settings.Default.ExpTimer1 = value;
				Properties.Settings.Default.Save();
				NotifyPropertyChanged("ExpTimer1");
			}
		}
		ulong expTimer2;
		public ulong ExpTimer2 {
			get { return expTimer2; }
			set {
				expTimer2 = value;
				Properties.Settings.Default.ExpTimer2 = value;
				Properties.Settings.Default.Save();
				NotifyPropertyChanged("ExpTimer2");
			}
		}
		ulong expTimer3;
		public ulong ExpTimer3 {
			get { return expTimer3; }
			set {
				expTimer3 = value;
				Properties.Settings.Default.ExpTimer3 = value;
				Properties.Settings.Default.Save();
				NotifyPropertyChanged("ExpTimer3");
			}
		}
		ulong expTimer4;
		public ulong ExpTimer4 {
			get { return expTimer4; }
			set {
				expTimer4 = value;
				Properties.Settings.Default.ExpTimer4 = value;
				Properties.Settings.Default.Save();
				NotifyPropertyChanged("ExpTimer4");
			}
		}
		#endregion
		#region 建造タイマー
		ulong buildTimer1;
		public ulong BuildTimer1 {
			get { return buildTimer1; }
			set {
				buildTimer1 = value;
				Properties.Settings.Default.BuildTimer1 = value;
				Properties.Settings.Default.Save();
				NotifyPropertyChanged("BuildTimer1");
			}
		}
		ulong buildTimer2;
		public ulong BuildTimer2 {
			get { return buildTimer2; }
			set {
				buildTimer2 = value;
				Properties.Settings.Default.BuildTimer2 = value;
				Properties.Settings.Default.Save();
				NotifyPropertyChanged("BuildTimer2");
			}
		}
		ulong buildTimer3;
		public ulong BuildTimer3 {
			get { return buildTimer3; }
			set {
				buildTimer3 = value;
				Properties.Settings.Default.BuildTimer3 = value;
				Properties.Settings.Default.Save();
				NotifyPropertyChanged("BuildTimer3");
			}
		}
		ulong buildTimer4;
		public ulong BuildTimer4 {
			get { return buildTimer4; }
			set {
				buildTimer4 = value;
				Properties.Settings.Default.BuildTimer4 = value;
				Properties.Settings.Default.Save();
				NotifyPropertyChanged("BuildTimer4");
			}
		}
		#endregion
		#region 開発タイマー
		ulong devTimer1;
		public ulong DevTimer1 {
			get { return devTimer1; }
			set {
				devTimer1 = value;
				Properties.Settings.Default.DevTimer1 = value;
				Properties.Settings.Default.Save();
				NotifyPropertyChanged("DevTimer1");
			}
		}
		ulong devTimer2;
		public ulong DevTimer2 {
			get { return devTimer2; }
			set {
				devTimer2 = value;
				Properties.Settings.Default.DevTimer2 = value;
				Properties.Settings.Default.Save();
				NotifyPropertyChanged("DevTimer2");
			}
		}
		ulong devTimer3;
		public ulong DevTimer3 {
			get { return devTimer3; }
			set {
				devTimer3 = value;
				Properties.Settings.Default.DevTimer3 = value;
				Properties.Settings.Default.Save();
				NotifyPropertyChanged("DevTimer3");
			}
		}
		ulong devTimer4;
		public ulong DevTimer4 {
			get { return devTimer4; }
			set {
				devTimer4 = value;
				Properties.Settings.Default.DevTimer4 = value;
				Properties.Settings.Default.Save();
				NotifyPropertyChanged("DevTimer4");
			}
		}
		#endregion
		#region 入渠タイマー
		ulong dockTimer1;
		public ulong DockTimer1 {
			get { return dockTimer1; }
			set {
				dockTimer1 = value;
				Properties.Settings.Default.DockTimer1 = value;
				Properties.Settings.Default.Save();
				NotifyPropertyChanged("DockTimer1");
			}
		}
		ulong dockTimer2;
		public ulong DockTimer2 {
			get { return dockTimer2; }
			set {
				dockTimer2 = value;
				Properties.Settings.Default.DockTimer2 = value;
				Properties.Settings.Default.Save();
				NotifyPropertyChanged("DockTimer2");
			}
		}
		ulong dockTimer3;
		public ulong DockTimer3 {
			get { return dockTimer3; }
			set {
				dockTimer3 = value;
				Properties.Settings.Default.DockTimer3 = value;
				Properties.Settings.Default.Save();
				NotifyPropertyChanged("DockTimer3");
			}
		}
		ulong dockTimer4;
		public ulong DockTimer4 {
			get { return dockTimer4; }
			set {
				dockTimer4 = value;
				Properties.Settings.Default.DockTimer4 = value;
				Properties.Settings.Default.Save();
				NotifyPropertyChanged("DockTimer4");
			}
		}
		#endregion
		public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };
		public void RedrawTimerWindow() {
			// タイマー全体を更新する
			NotifyPropertyChanged("ExpTimer1");
			NotifyPropertyChanged("ExpTimer2");
			NotifyPropertyChanged("ExpTimer3");
			NotifyPropertyChanged("ExpTimer4");
			NotifyPropertyChanged("BuildTimer1");
			NotifyPropertyChanged("BuildTimer2");
			NotifyPropertyChanged("BuildTimer3");
			NotifyPropertyChanged("BuildTimer4");
			NotifyPropertyChanged("DevTimer1");
			NotifyPropertyChanged("DevTimer2");
			NotifyPropertyChanged("DevTimer3");
			NotifyPropertyChanged("DevTimer4");
			NotifyPropertyChanged("DockTimer1");
			NotifyPropertyChanged("DockTimer2");
			NotifyPropertyChanged("DockTimer3");
			NotifyPropertyChanged("DockTimer4");
		}
		private void NotifyPropertyChanged(string parameter) {
			PropertyChanged(this, new PropertyChangedEventArgs(parameter));
		}
	}
}
