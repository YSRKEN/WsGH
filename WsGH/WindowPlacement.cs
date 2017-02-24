using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace WsGH {
	// WINDOWPLACEMENT構造体で使用するためのRECT構造体
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct RECT {
		public int Left;
		public int Top;
		public int Right;
		public int Bottom;
		public RECT(int left, int top, int right, int bottom) {
			Left = left;
			Top = top;
			Right = right;
			Bottom = bottom;
		}
	}
	// WINDOWPLACEMENT構造体で使用するためのPOINT構造体
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct POINT {
		public int X;
		public int Y;
		public POINT(int x, int y) {
			X = x;
			Y = y;
		}
	}
	// WINDOWPLACEMENT構造体
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct WINDOWPLACEMENT {
		public int length;
		public int flags;
		public int showCmd;
		public POINT minPosition;
		public POINT maxPosition;
		public RECT normalPosition;
	}
	// WINDOWPLACEMENT構造体でウィンドウ位置・サイズを変更するためのAPI
	static class NativeMethods {
		// API宣言
		[DllImport("user32.dll")]
		static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);
		[DllImport("user32.dll")]
		static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);
		// 定数宣言
		const int SW_SHOWNORMAL = 1;
		const int SW_SHOWMINIMIZED = 2;
		// ヘルパーメソッド
		public static void SetWindowPlacementHelper(Window window, WINDOWPLACEMENT wp) {
			// wpについて色々と設定する
			wp.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
			wp.flags = 0;
			wp.showCmd = (wp.showCmd == SW_SHOWMINIMIZED ? SW_SHOWNORMAL : wp.showCmd);
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

namespace WsGH.Properties {
	partial class Settings {
		[global::System.Configuration.UserScopedSettingAttribute()]
		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		public WINDOWPLACEMENT MainWindowPlacement {
			get {
				return ((WINDOWPLACEMENT)(this["MainWindowPlacement"]));
			}
			set {
				this["MainWindowPlacement"] = value;
			}
		}
		[global::System.Configuration.UserScopedSettingAttribute()]
		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		public WINDOWPLACEMENT TimerWindowPlacement {
			get {
				return ((WINDOWPLACEMENT)(this["TimerWindowPlacement"]));
			}
			set {
				this["TimerWindowPlacement"] = value;
			}
		}
		[global::System.Configuration.UserScopedSettingAttribute()]
		[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
		public WINDOWPLACEMENT SupplyWindowPlacement {
			get {
				return ((WINDOWPLACEMENT)(this["SupplyWindowPlacement"]));
			}
			set {
				this["SupplyWindowPlacement"] = value;
			}
		}
	}
}
