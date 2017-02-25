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
