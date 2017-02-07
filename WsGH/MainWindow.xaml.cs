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
using System.Windows.Forms;
using System.Windows.Threading;
using System.Drawing;

namespace WsGH {
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window {
		ScreenshotProvider sp = null;
		Logger logger;
		TimerWindow tw;
		int timerWindowSecond;
		// コンストラクタ
		public MainWindow() {
			InitializeComponent();
			SceneRecognition.InitialSceneRecognition();
			MouseLeftButtonDown += (o, e) => DragMove();
			// フォルダの有無をチェック
			if(!System.IO.Directory.Exists(@"pic\")) {
				System.IO.Directory.CreateDirectory(@"pic\");
			}
			// ログ表示を初期化
			DataContext = logger = new Logger() { LoggingText = "" };
			// アプリの設定を初期化
			TwitterOptionMenu.IsChecked = Properties.Settings.Default.ScreenshotForTwitterFlg;
			BackgroundOptionMenu.Header = $"Background : {ColorToString(Properties.Settings.Default.BackgroundColor)} ...";
			// タイマーを作成する
			DispatcherTimer m_Timer = new DispatcherTimer(DispatcherPriority.Normal, this.Dispatcher);
			m_Timer.Interval = TimeSpan.FromMilliseconds(200.0);
			m_Timer.Tick += new EventHandler(DispatcherTimer_Tick);
			// タイマーの実行開始
			m_Timer.Start();
			timerWindowSecond = DateTime.Now.Second;
			// タイマー画面を作成・表示
			tw = new TimerWindow();
			if(Properties.Settings.Default.ShowTimerWindowFlg) {
				tw.Show();
			}
		}
		// ウィンドウ位置復元
		protected override void OnSourceInitialized(EventArgs e) {
			base.OnSourceInitialized(e);
			try {
				NativeMethods.SetWindowPlacementHelper(this, Properties.Settings.Default.MainWindowPlacement);
			}
			catch { }
		}
		// ウィンドウ位置保存
		protected override void OnClosing(CancelEventArgs e) {
			base.OnClosing(e);
			Properties.Settings.Default.MainWindowPlacement = NativeMethods.GetWindowPlacementHelper(this);
			Properties.Settings.Default.Save();
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
		private void ShowTimerWindow_Click(object sender, RoutedEventArgs e) {
			// 2枚以上同じウィンドウを生成しないようにする
			if(Properties.Settings.Default.ShowTimerWindowFlg)
				return;
			// ウィンドウを生成
			tw = new TimerWindow();
			Properties.Settings.Default.ShowTimerWindowFlg = true;
			Properties.Settings.Default.Save();
			tw.Show();
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
			// チェックの状態をログに記録する
			if(TwitterOptionMenu.IsChecked) {
				addLog("for Twitter : True");
			} else {
				addLog("for Twitter : False");
			}
			// チェックの状態を反映させる
			Properties.Settings.Default.ScreenshotForTwitterFlg = TwitterOptionMenu.IsChecked;
			Properties.Settings.Default.Save();
		}
		private void BackgroundOptionMenu_Click(object sender, RoutedEventArgs e) {
			// ゲーム背景色を変更するため、色変更ダイアログを表示する
			var cd = new ColorDialog();
			if(cd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
				// 色変更を行い、画面にも反映させる
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
			// 成功した場合と失敗した場合で処理を分ける
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
			// 現在時間からファイル名を生成する
			var dt = DateTime.Now;
			var fileName = dt.ToString("yyyy-MM-dd hh-mm-ss-fff") + (TwitterOptionMenu.IsChecked ? "_twi" : "") + ".png";
			// 画像を保存する
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
			// 可能ならスクショを取得する
			Bitmap screenShot = sp?.getScreenShot(TwitterOptionMenu.IsChecked);
			// 毎フレーム毎の処理
			{
				// スクショが取得できていた場合
				if(screenShot != null) {
					// シーンを判定する
					var scene = SceneRecognition.JudgeScene(screenShot);
					// シーンごとに振り分ける
					switch(scene) {
					case SceneRecognition.SceneType.Expedition:
						// 遠征中なら、遠征時間読み取りにチャレンジしてみる
						var expEndTime = SceneRecognition.getExpeditionTimer(screenShot);
						var bindData = tw.DataContext as TimerValue;
						foreach(var pair in expEndTime) {
							switch(pair.Key) {
							case 0:
								bindData.ExpTimer1 = pair.Value;
								break;
							case 1:
								bindData.ExpTimer2 = pair.Value;
								break;
							case 2:
								bindData.ExpTimer3 = pair.Value;
								break;
							case 3:
								bindData.ExpTimer4 = pair.Value;
								break;
							default:
								break;
							}
						}
						break;
					case SceneRecognition.SceneType.Build:
						// 建造中
						break;
					case SceneRecognition.SceneType.Develop:
						// 開発中
						break;
					case SceneRecognition.SceneType.Dock:
						// 入渠中
						break;
					default:
						break;
					}
				}
			}
			// 1秒ごとの処理
			var timerWindowSecondNow = DateTime.Now.Second;
			if(timerWindowSecond != timerWindowSecondNow) {
				timerWindowSecond = timerWindowSecondNow;
				// スクショが取得できていた場合
				if(screenShot != null) {
					// ズレチェック
					if(sp.IsPositionShifting()) {
						addLog("Found PositionShifting!");
						addLog("try fix PositionShifting...");
						if(sp.TryPositionShifting()) {
							GetScreenshotMenu.IsEnabled = ScreenShotButton.IsEnabled = true;
							addLog("fix PositionShifting : Success");
							addLog("  " + sp.getPositionStr());
						} else {
							GetScreenshotMenu.IsEnabled = ScreenShotButton.IsEnabled = false;
							addLog("fix PositionShifting : Failed");
						}
					}
				}
				// タイマーの表示を更新する
				var bindData = tw.DataContext as TimerValue;
				bindData.RedrawTimerWindow();
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
			get { return loggingText; }
			set { loggingText = value; NotifiyPropertyChanged("LoggingText"); }
		}
		private void NotifiyPropertyChanged(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
