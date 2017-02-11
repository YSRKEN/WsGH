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
		ScreenshotProvider sp = null;	//スクショ用の情報を記憶する
		TimerWindow tw = null;			//TimerWindowのインスタンス
		int timerWindowSecond;			//毎秒行う処理のために秒数を記憶
		// コンストラクタ
		public MainWindow() {
			InitializeComponent();
			SceneRecognition.InitialSceneRecognition();
			MouseLeftButtonDown += (o, e) => DragMove();
			// フォルダの有無をチェック
			if(!System.IO.Directory.Exists(@"pic\")) {
				System.IO.Directory.CreateDirectory(@"pic\");
			}
			// 画面表示を初期化
			DataContext = new MainWindowDC() {
				LoggingText = "",
				MenuHeaderBackgroundOther = "",
			};
			// アプリの設定を初期化
			TwitterOptionMenu.IsChecked = Properties.Settings.Default.ScreenshotForTwitterFlg;
			SetBackgroundCheck(Properties.Settings.Default.BackgroundColorType);
			// タイマーを作成する
			DispatcherTimer m_Timer = new DispatcherTimer(DispatcherPriority.Normal, Dispatcher);
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
			sp = new ScreenshotProvider(new AfterAction(getPosition), GetBackgroundColor());
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
			addLog($"{Properties.Resources.LoggingTextForTwitter} : {(TwitterOptionMenu.IsChecked ? "True" : "False")}");
			// チェックの状態を反映させる
			Properties.Settings.Default.ScreenshotForTwitterFlg = TwitterOptionMenu.IsChecked;
			Properties.Settings.Default.Save();
		}
		private void BackgroundOptionMenuBS_Click(object sender, RoutedEventArgs e) {
			SetBackgroundCheck(0);
			// 画面にも反映させる
			addLog($"{Properties.Resources.LoggingTextrBackground} : #000000");
		}
		private void BackgroundOptionMenuNox_Click(object sender, RoutedEventArgs e) {
			SetBackgroundCheck(1);
			// 画面にも反映させる
			addLog($"{Properties.Resources.LoggingTextrBackground} : #1C1B20");
		}
		private void BackgroundOptionMenuOther_Click(object sender, RoutedEventArgs e) {
			SetBackgroundCheck(2);
			// ゲーム背景色を変更するため、色変更ダイアログを表示する
			var cd = new ColorDialog();
			if(cd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
				// 色変更を行う
				Properties.Settings.Default.BackgroundColor = cd.Color;
				Properties.Settings.Default.Save();
				// 画面にも反映させる
				addLog($"{Properties.Resources.LoggingTextrBackground} : {MainWindowDC.ColorToString(Properties.Settings.Default.BackgroundColor)}");
				var bindData = DataContext as MainWindowDC;
				bindData.MenuHeaderBackgroundOther = "";
			}
		}
		private void SelectLanguageJapanese_Click(object sender, RoutedEventArgs e) {
			// 日本語に切り替え
			ChangeLanguage("ja-JP");
		}
		private void SelectLanguageEnglish_Click(object sender, RoutedEventArgs e) {
			// 英語に切り替え
			ChangeLanguage("");
		}
		// ボタン操作
		private void ScreenShotButton_Click(object sender, RoutedEventArgs e) {
			saveScreenshot();
		}
		// ログに内容を追加
		private void addLog(string str) {
			var dt = DateTime.Now;
			var bindData = DataContext as MainWindowDC;
			bindData.LoggingText += dt.ToString("hh:mm:ss ") + str + "\n";
		}
		// 背景色のチェックを切り替え
		void SetBackgroundCheck(int colorType) {
			Properties.Settings.Default.BackgroundColorType = colorType;
			Properties.Settings.Default.Save();
			switch(colorType) {
			case 0:
				// BlueStacks
				BackgroundOptionMenuBS.IsChecked = true;
				BackgroundOptionMenuNox.IsChecked = false;
				BackgroundOptionMenuOther.IsChecked = false;
				break;
			case 1:
				// Nox
				BackgroundOptionMenuBS.IsChecked = false;
				BackgroundOptionMenuNox.IsChecked = true;
				BackgroundOptionMenuOther.IsChecked = false;
				break;
			case 2:
				// Other
				BackgroundOptionMenuBS.IsChecked = false;
				BackgroundOptionMenuNox.IsChecked = false;
				BackgroundOptionMenuOther.IsChecked = true;
				break;
			default:
				break;
			}
		}
		// 背景色を取得
		System.Drawing.Color GetBackgroundColor() {
			switch(Properties.Settings.Default.BackgroundColorType) {
			case 0:
				// BlueStacks
				return System.Drawing.Color.FromArgb(0, 0, 0);
			case 1:
				// Nox
				return System.Drawing.Color.FromArgb(28, 27, 32);
			case 2:
				// Other
				return Properties.Settings.Default.BackgroundColor;
			default:
				return System.Drawing.Color.FromArgb(0, 0, 0);
			}
		}
		// 座標取得後の画面更新処理
		private void getPosition() {
			// 成功か失敗かを読み取る
			var isGetPosition = sp.isGetPosition();
			// 結果を記録
			GetScreenshotMenu.IsEnabled = ScreenShotButton.IsEnabled = isGetPosition;
			addLog($"{Properties.Resources.LoggingTextGetPosition} : {(isGetPosition ? "Success" : "Failed")}");
			// 成功時は座標を表示する
			if(isGetPosition)
				addLog("  " + sp.getPositionStr());
		}
		// 画像保存処理
		private void saveScreenshot() {
			// 現在時間からファイル名を生成する
			var dt = DateTime.Now;
			var fileName = dt.ToString("yyyy-MM-dd hh-mm-ss-fff") + (TwitterOptionMenu.IsChecked ? "_twi" : "") + ".png";
			// 画像を保存する
			try {
				sp.getScreenShot(TwitterOptionMenu.IsChecked).Save(@"pic\" + fileName);
				addLog($"{Properties.Resources.LoggingTextGetScreenshot} : Success");
				addLog("  (" + fileName + ")");
			} catch(Exception) {
				addLog($"{Properties.Resources.LoggingTextGetScreenshot} : Failed");
			}
		}
		// 言語切替
		void ChangeLanguage(string culture) {
			ResourceService.Current.ChangeCulture(culture);
			var bindData = DataContext as MainWindowDC;
			bindData.MenuHeaderBackgroundOther = "";
		}
		// タイマー動作
		private void DispatcherTimer_Tick(object sender, EventArgs e) {
			// 可能ならスクショを取得する
			Bitmap captureFrame = sp?.getScreenShot(TwitterOptionMenu.IsChecked);
			#region 毎フレーム毎の処理
			// スクショが取得できていた場合
			if(captureFrame != null) {
				// シーンを判定する
				var scene = SceneRecognition.JudgeScene(captureFrame);
				// 現在認識しているシーンを表示する
				var culture = (Properties.Resources.Culture == null ? System.Globalization.CultureInfo.CurrentCulture : Properties.Resources.Culture).ToString();
				SceneTextBlock.Text = $"{Properties.Resources.LoggingTextScene} : {(culture == "ja-JP" ? SceneRecognition.SceneStringJapanese[scene] : SceneRecognition.SceneString[scene])}";
				// シーンごとに振り分ける
				var bindData = tw.DataContext as TimerValue;
				switch(scene) {
				case SceneRecognition.SceneType.Expedition:
					#region 遠征中なら、遠征時間を読み取る
					var expEndTime = SceneRecognition.getExpeditionTimer(captureFrame);
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
					#endregion
				case SceneRecognition.SceneType.Build:
					#region 建造中なら、建造時間を読み取る
					var buildEndTime = SceneRecognition.getBuildTimer(captureFrame);
					foreach(var pair in buildEndTime) {
						switch(pair.Key) {
						case 0:
							bindData.BuildTimer1 = pair.Value;
							break;
						case 1:
							bindData.BuildTimer2 = pair.Value;
							break;
						case 2:
							bindData.BuildTimer3 = pair.Value;
							break;
						case 3:
							bindData.BuildTimer4 = pair.Value;
							break;
						default:
							break;
						}
					}
					break;
				#endregion
				case SceneRecognition.SceneType.Develop:
					#region 開発中なら、開発時間を読み取る
					var devEndTime = SceneRecognition.getDevTimer(captureFrame);
					foreach(var pair in devEndTime) {
						switch(pair.Key) {
						case 0:
							bindData.DevTimer1 = pair.Value;
							break;
						case 1:
							bindData.DevTimer2 = pair.Value;
							break;
						case 2:
							bindData.DevTimer3 = pair.Value;
							break;
						case 3:
							bindData.DevTimer4 = pair.Value;
							break;
						default:
							break;
						}
					}
					break;
				#endregion
				case SceneRecognition.SceneType.Dock:
					#region 入渠中なら、入渠時間を読み取る
					var dockEndTime = SceneRecognition.getDockTimer(captureFrame);
					foreach(var pair in dockEndTime) {
						switch(pair.Key) {
						case 0:
							bindData.DockTimer1 = pair.Value;
							break;
						case 1:
							bindData.DockTimer2 = pair.Value;
							break;
						case 2:
							bindData.DockTimer3 = pair.Value;
							break;
						case 3:
							bindData.DockTimer4 = pair.Value;
							break;
						default:
							break;
						}
					}
					break;
				#endregion
				default:
					break;
				}
			}
			#endregion
			#region 1秒ごとの処理
			var timerWindowSecondNow = DateTime.Now.Second;
			if(timerWindowSecond != timerWindowSecondNow) {
				timerWindowSecond = timerWindowSecondNow;
				// スクショが取得できていた場合
				if(captureFrame != null) {
					// ズレチェック
					if(sp.IsPositionShifting()) {
						addLog(Properties.Resources.LoggingTextFoundPS);
						addLog(Properties.Resources.LoggingTextTryFixPS);
						// ズレ修復の結果を代入
						var tryFixPositionShifting = sp.TryPositionShifting();
						GetScreenshotMenu.IsEnabled = ScreenShotButton.IsEnabled = tryFixPositionShifting;
						addLog($"{Properties.Resources.LoggingTextFixPS} : {(tryFixPositionShifting ? "Success" : "Failed")}");
						if(tryFixPositionShifting)
							addLog("  " + sp.getPositionStr());
					}
				}
				// タイマーの表示を更新する
				var bindData = tw.DataContext as TimerValue;
				bindData.RedrawTimerWindow();
			}
			#endregion
			captureFrame?.Dispose();
		}
	}
	class MainWindowDC : INotifyPropertyChanged {
		// ログテキスト
		string loggingText;
		public string LoggingText {
			get { return loggingText; }
			set {
				loggingText = value;
				NotifyPropertyChanged("LoggingText");
			}
		}
		// 背景オプション
		public string MenuHeaderBackgroundOther {
			get {
				return $"{Properties.Resources.MenuHeaderBackgroundOther} : {ColorToString(Properties.Settings.Default.BackgroundColor)} ...";
			}
			set {
				NotifyPropertyChanged("MenuHeaderBackgroundOther");
			}
		}
		// 色情報を文字列に変換
		public static string ColorToString(System.Drawing.Color clr) {
			return "#" + clr.R.ToString("X2") + clr.G.ToString("X2") + clr.B.ToString("X2");
		}
		//
		public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };
		public void NotifyPropertyChanged(string parameter) {
			PropertyChanged(this, new PropertyChangedEventArgs(parameter));
		}
	}
}
