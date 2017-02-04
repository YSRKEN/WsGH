﻿using System;
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
using System.Windows.Forms;
using System.Windows.Threading;

namespace WsGH {
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window {
		ScreenshotProvider sp = null;
		Logger logger;
		DispatcherTimer m_Timer = null;
		// コンストラクタ
		public MainWindow() {
			InitializeComponent();
			MouseLeftButtonDown += (o, e) => DragMove();
			// フォルダの有無をチェック
			if(!System.IO.Directory.Exists(@"pic\")) {
				System.IO.Directory.CreateDirectory(@"pic\");
			}
			// ログ表示を初期化
			DataContext = logger = new Logger() { LoggingText = "" };
			// アプリの設定を初期化
			TwitterOptionMenu.IsChecked = Properties.Settings.Default.ScreenshotForTwitterFlg;
			BackgroundOptionMenu.Header = "Background : " + ColorToString(Properties.Settings.Default.BackgroundColor) + " ...";
			// タイマーを作成する
			m_Timer = new DispatcherTimer(DispatcherPriority.Normal, this.Dispatcher);
			m_Timer.Interval = TimeSpan.FromMilliseconds(1000.0);
			m_Timer.Tick += new EventHandler(DispatcherTimer_Tick);
			// タイマーの実行開始
			m_Timer.Start();
			// タイマー画面を作成・表示
			TimerWindow tw = new TimerWindow();
			if(Properties.Settings.Default.ShowTimerWindowFlg) {
				tw.Show();
			}
		}
		// メニュー操作
		private void ExitMenu_Click(object sender, RoutedEventArgs e) {
			Close();
		}
		private void GetPositionMenu_Click(object sender, RoutedEventArgs e) {
			sp = new ScreenshotProvider(new AfterAction(getPosition), Properties.Settings.Default.BackgroundColor);
		}
		private void GetScreenshotMenu_Click(object sender, RoutedEventArgs e) {
			saveScreenshot();
		}
		private void ShowTimerWindow_Click(object sender, RoutedEventArgs e) {
			if(!Properties.Settings.Default.ShowTimerWindowFlg) {
				TimerWindow tw = new TimerWindow();
				Properties.Settings.Default.ShowTimerWindowFlg = true;
				tw.Show();
			}
		}
		private void ShowPicFolderMenu_Click(object sender, RoutedEventArgs e) {
			System.Diagnostics.Process.Start(@"pic\");
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
			System.Windows.MessageBox.Show(asmttl + " Ver." + asmver + "\n" + asmcpy + "\n" + asmprd);
		}
		private void TwitterOption_Changed(object sender, RoutedEventArgs e) {
			if(TwitterOptionMenu.IsChecked) {
				addLog("for Twitter : True");
			} else {
				addLog("for Twitter : False");
			}
			Properties.Settings.Default.ScreenshotForTwitterFlg = TwitterOptionMenu.IsChecked;
			Properties.Settings.Default.Save();
		}
		private void BackgroundOptionMenu_Click(object sender, RoutedEventArgs e) {
			var cd = new ColorDialog();
			if(cd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
				Properties.Settings.Default.BackgroundColor = cd.Color;
				BackgroundOptionMenu.Header = "Background : " + ColorToString(Properties.Settings.Default.BackgroundColor) + " ...";
				addLog("Background : " + ColorToString(Properties.Settings.Default.BackgroundColor));
				Properties.Settings.Default.Save();
			}
		}
		// ボタン操作
		private void ScreenShotButton_Click(object sender, RoutedEventArgs e) {
			saveScreenshot();
		}
		// ログに内容を追加
		private void addLog(string str) {
			var dt = DateTime.Now;
			logger.LoggingText += dt.ToString("hh:mm:ss ") + str + "\n";
		}
		// 座標取得後の画面更新処理
		private void getPosition() {
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
		private void saveScreenshot() {
			var dt = DateTime.Now;
			var fileName = dt.ToString("yyyy-MM-dd hh-mm-ss-fff") + ".png";
			try {
				sp.getScreenShot(TwitterOptionMenu.IsChecked).Save(@"pic\" + fileName);
				addLog("save Screenshot : Success");
				addLog("  (" + fileName + ")");
			} catch(Exception) {
				addLog("save Screenshot : Failed");
			}
		}
		// タイマー動作
		private void DispatcherTimer_Tick(object sender, EventArgs e) {
			// 座標取得できていた場合の処理
			if(sp != null && sp.isGetPosition()) {
				var screenShot = sp.getScreenShot(TwitterOptionMenu.IsChecked);
				// 遠征中なら、遠征時間読み取りにチャレンジしてみる
				if(SceneRecognition.isExpeditionScene(screenShot)) {
					addLog("遠征");
				}
			}
		}
		// 色情報を文字列に変換
		public static string ColorToString(System.Drawing.Color clr) {
			return "#" + clr.R.ToString("X2") + clr.G.ToString("X2") + clr.B.ToString("X2");
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
