/* 多言語対応リソースファイルを任意で切り替えられるようにする
 * http://grabacr.net/archives/1647
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WsGH.Properties;

namespace WsGH {
	public class ResourceService : INotifyPropertyChanged {
		#region singleton members

		private static readonly ResourceService _current = new ResourceService();
		public static ResourceService Current {
			get { return _current; }
		}

		#endregion

		private readonly Resources _resources = new Resources();

		/// <summary>
		/// 多言語化されたリソースを取得します。
		/// </summary>
		public Resources Resources {
			get { return _resources; }
		}

		#region INotifyPropertyChanged members

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

		/// <summary>
		/// 指定されたカルチャ名を使用して、リソースのカルチャを変更します。
		/// </summary>
		/// <param name="name">カルチャの名前。</param>
		public void ChangeCulture(string name) {
			Resources.Culture = CultureInfo.GetCultureInfo(name);
			RaisePropertyChanged("Resources");
		}
	}
}
