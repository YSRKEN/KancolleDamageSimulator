/* WPF‚ÅNumericUpDown‚ðŽg‚¤ _ tocsworld
 * https://tocsworld.wordpress.com/2014/05/10/wpf%E3%81%A7numericupdown%E3%82%92%E4%BD%BF%E3%81%86/
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BindableWinFormsControl {
	using System.ComponentModel;
	internal class TestBindObject : INotifyPropertyChanged {
		private string criticalLabelString;
		private int antiSubKammusuString;
		private int antiSubWeaponsString;

		public string CriticalLabelString {
			get { return criticalLabelString; }
			set {
				criticalLabelString = value;
				NotifyPropertyChanged("CriticalLabelString");
			}
		}

		public int AntiSubKammusuString {
			get { return antiSubKammusuString; }
			set {
				antiSubKammusuString = value;
				NotifyPropertyChanged("AntiSubKammusuString");
			}
		}

		public int AntiSubWeaponsString {
			get { return antiSubWeaponsString; }
			set {
				antiSubWeaponsString = value;
				NotifyPropertyChanged("AntiSubWeaponsString");
			}
		}

		private void NotifyPropertyChanged(string parameter) {
			PropertyChanged(this, new PropertyChangedEventArgs(parameter));
		}
		public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };
	}
}
