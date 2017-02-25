using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace WsGH {
	// MonitorFromWindowが返したディスプレイ番号の種類
	public enum MonitorDefaultTo { MONITOR_DEFAULTTONULL, MONITOR_DEFAULTTOPRIMARY, MONITOR_DEFAULTTONEAREST }
	// GetDpiForMonitorが返したDPIの種類
	public enum MonitorDpiType { MDT_EFFECTIVE_DPI, MDT_ANGULAR_DPI, MDT_RAW_DPI, MDT_DEFAULT = MDT_EFFECTIVE_DPI }
	public enum CommandShowSwitch { SW_SHOWNORMAL = 1, SW_SHOWMINIMIZED = 2 };
	class NativeMethods { 
		// ウィンドウハンドルから、そのウィンドウが乗っているディスプレイ番号を取得
		[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern IntPtr MonitorFromWindow(IntPtr hwnd, MonitorDefaultTo dwFlags);
		// ディスプレイ番号からDPIを取得
		[DllImport("SHCore.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
		public static extern void GetDpiForMonitor(IntPtr hmonitor, MonitorDpiType dpiType, ref uint dpiX, ref uint dpiY);
		// オブジェクトを破棄する
		[DllImport("gdi32.dll")]
		public static extern bool DeleteObject(IntPtr hObject);
		// ウィンドウ位置をセットする
		[DllImport("user32.dll")]
		static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);
		// ウィンドウ位置を取得する
		[DllImport("user32.dll")]
		static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);
		// ヘルパーメソッド
		public static void SetWindowPlacementHelper(Window window, WINDOWPLACEMENT wp) {
			// wpについて色々と設定する
			wp.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
			wp.flags = 0;
			if(wp.showCmd == (int)CommandShowSwitch.SW_SHOWMINIMIZED)
				wp.showCmd = (int)CommandShowSwitch.SW_SHOWNORMAL;
			// ウィンドウサイズは変更しないように小細工する
			wp.normalPosition.Right = wp.normalPosition.Left + (int)window.Width;
			wp.normalPosition.Bottom = wp.normalPosition.Top + (int)window.Height;
			// GetWindowPlacement関数を実行
			var hwnd = new WindowInteropHelper(window).Handle;
			SetWindowPlacement(hwnd, ref wp);
		}
		public static WINDOWPLACEMENT GetWindowPlacementHelper(Window window) {
			var wp = new WINDOWPLACEMENT();
			var hwnd = new WindowInteropHelper(window).Handle;
			GetWindowPlacement(hwnd, out wp);
			return wp;
		}

	}
}
