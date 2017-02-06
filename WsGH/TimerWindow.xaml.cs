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
		// 遠征タイマー
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
		public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };
		public void RedrawTimerWindow() {
			// タイマー全体を更新する
			NotifyPropertyChanged("ExpTimer1");
			NotifyPropertyChanged("ExpTimer2");
			NotifyPropertyChanged("ExpTimer3");
			NotifyPropertyChanged("ExpTimer4");
		}
		private void NotifyPropertyChanged(string parameter) {
			PropertyChanged(this, new PropertyChangedEventArgs(parameter));
		}
	}
}
