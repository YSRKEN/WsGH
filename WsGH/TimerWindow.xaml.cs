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
		}
		// ウィンドウを閉じる際の処理
		private void Window_Closed(object sender, EventArgs e) {
			Properties.Settings.Default.ShowTimerWindowFlg = false;
			Close();
		}
	}
	// コンバータ
	public class TimerConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			var time = (ulong)value;
			if(time == 0) {
				return "--:--:--";
			}else {
				return "00:00:00";
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
			set { expTimer1 = value; NotifyPropertyChanged("ExpTimer1"); }
		}
		ulong expTimer2;
		public ulong ExpTimer2 {
			get { return expTimer2; }
			set { expTimer2 = value; NotifyPropertyChanged("ExpTimer2"); }
		}
		ulong expTimer3;
		public ulong ExpTimer3 {
			get { return expTimer3; }
			set { expTimer3 = value; NotifyPropertyChanged("ExpTimer3"); }
		}
		ulong expTimer4;
		public ulong ExpTimer4 {
			get { return expTimer4; }
			set { expTimer4 = value; NotifyPropertyChanged("ExpTimer4"); }
		}
		public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };
		private void NotifyPropertyChanged(string parameter) {
			PropertyChanged(this, new PropertyChangedEventArgs(parameter));
		}
	}
}
