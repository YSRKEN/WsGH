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
		public MainWindow() {
			InitializeComponent();
		}

		private void button_Click(object sender, RoutedEventArgs e) {
			using(var img = new IplImage(@"D:\lena.png")) {
				Cv.SetImageROI(img, new CvRect(200, 200, 180, 200));
				Cv.Not(img, img);
				Cv.ResetImageROI(img);
				using(new CvWindow(img)) {
					Cv.WaitKey();
				}
			}
		}
	}
}
