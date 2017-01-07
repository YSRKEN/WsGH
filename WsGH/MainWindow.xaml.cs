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

		public MainWindow() {
			InitializeComponent();
		}

		private void ExitMenu_Click(object sender, RoutedEventArgs e) {
			Close();
		}

		private void GetPositionMenu_Click(object sender, RoutedEventArgs e) {
			sp = new ScreenshotProvider(new AfterAction(GetPosition));
		}

		private void GetScreenshotMenu_Click(object sender, RoutedEventArgs e) {
			saveScreenshot();
		}

		private void ScreenShotButton_Click(object sender, RoutedEventArgs e) {
			saveScreenshot();
		}

		void GetPosition() {
			if(sp.isGetPosition()) {
				GetScreenshotMenu.IsEnabled = ScreenShotButton.IsEnabled = true;
			} else {
				GetScreenshotMenu.IsEnabled = ScreenShotButton.IsEnabled = false;
			}
		}

		void saveScreenshot() {
			sp.getScreenShot().Save("test.png");
		}
	}
}
