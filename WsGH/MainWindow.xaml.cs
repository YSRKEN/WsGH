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

namespace WsGH {
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window {
		ScreenshotProvider sp;
		// コンストラクタ
		public MainWindow() {
			InitializeComponent();
		}
		// メニュー操作
		private void ExitMenu_Click(object sender, RoutedEventArgs e) {
			Close();
		}
		private void GetPositionMenu_Click(object sender, RoutedEventArgs e) {
			sp = new ScreenshotProvider(new AfterAction(GetPosition));
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
		// 座標取得後の画面更新処理
		void GetPosition() {
			if(sp.isGetPosition()) {
				GetScreenshotMenu.IsEnabled = ScreenShotButton.IsEnabled = true;
			} else {
				GetScreenshotMenu.IsEnabled = ScreenShotButton.IsEnabled = false;
			}
		}
		// 画像保存処理
		void saveScreenshot() {
			sp.getScreenShot().Save("test.png");
		}
	}
}
