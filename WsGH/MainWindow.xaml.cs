using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using OpenCvSharp;
using System.ComponentModel;

namespace WsGH {
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window {
		ScreenshotProvider sp;
		Logger logger;
		// コンストラクタ
		public MainWindow() {
			InitializeComponent();
			logger = new Logger() { LoggingText = "" };
			DataContext = logger;
		}
		// メニュー操作
		private void ExitMenu_Click(object sender, RoutedEventArgs e) {
			Close();
		}
		private void GetPositionMenu_Click(object sender, RoutedEventArgs e) {
			sp = new ScreenshotProvider(new AfterAction(getPosition));
		}
		private void GetScreenshotMenu_Click(object sender, RoutedEventArgs e) {
			saveScreenshot();
		}
		private void AboutMenu_Click(object sender, RoutedEventArgs e) {
			// 自分自身のバージョン情報を取得する
			// (http://dobon.net/vb/dotnet/file/myversioninfo.html)
			// AssemblyTitle
			var asmttl = ((System.Reflection.AssemblyTitleAttribute)
				Attribute.GetCustomAttribute(
				System.Reflection.Assembly.GetExecutingAssembly(),
				typeof(System.Reflection.AssemblyTitleAttribute))).Title;
			// AssemblyCopyright
			var asmcpy = ((System.Reflection.AssemblyCopyrightAttribute)
				Attribute.GetCustomAttribute(
				System.Reflection.Assembly.GetExecutingAssembly(),
				typeof(System.Reflection.AssemblyCopyrightAttribute))).Copyright;
			// AssemblyProduct
			var asmprd = ((System.Reflection.AssemblyProductAttribute)
				Attribute.GetCustomAttribute(
				System.Reflection.Assembly.GetExecutingAssembly(),
				typeof(System.Reflection.AssemblyProductAttribute))).Product;
			var asmver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
			MessageBox.Show(asmttl + " Ver." + asmver + "\n" + asmcpy + "\n" + asmprd);
		}
		// ボタン操作
		private void ScreenShotButton_Click(object sender, RoutedEventArgs e) {
			saveScreenshot();
		}
		// Log追加
		void addLog(string str) {
			logger.LoggingText += str + "\n";
		}
		// 座標取得後の画面更新処理
		void getPosition() {
			if(sp.isGetPosition()) {
				GetScreenshotMenu.IsEnabled = ScreenShotButton.IsEnabled = true;
				addLog("get Position : Success");
				addLog("  " + sp.getPositionStr());
			} else {
				GetScreenshotMenu.IsEnabled = ScreenShotButton.IsEnabled = false;
				addLog("get Position : Failed");
			}
		}
		// 画像保存処理
		void saveScreenshot() {
			var dt = DateTime.Now;
			var fileName = dt.ToString("yyyy-MM-dd hh-mm-ss-fff") + ".png";
			try {
				sp.getScreenShot(TwitterOptionMenu.IsChecked).Save(fileName);
				addLog("save Screenshot : Success");
				addLog("  (" + fileName + ")");
			} catch(Exception) {
				addLog("save Screenshot : Failed");
			}
		}
	}
	// ログ管理用のクラス
	class Logger : INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged;
		string loggingText;
		public string LoggingText {
			get {
				return loggingText;
			}
			set {
				loggingText = value;
				NotifiyPropertyChanged("LoggingText");
			}
		}
		private void NotifiyPropertyChanged(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
