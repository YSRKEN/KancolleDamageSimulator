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
		private int antiSubKammusu;
		private int antiSubWeapons;
		private int critical;
		private int defense;
		private int maxHP;
		private int nowHP;
		private string statusMessage;

		public int Critical
		{
			get { return critical; }
			set
			{
				critical = value;
				NotifyPropertyChanged("CriticalLabel");
			}
		}

		public string CriticalLabel {
			get { return (1.0 * critical / 10).ToString("0.0") + "%"; }
		}

		public int AntiSubKammusu {
			get { return antiSubKammusu; }
			set {
				antiSubKammusu = value;
				NotifyPropertyChanged("AntiSubKammusu");
			}
		}

		public int AntiSubWeapons {
			get { return antiSubWeapons; }
			set {
				antiSubWeapons = value;
				NotifyPropertyChanged("AntiSubWeapons");
			}
		}

		public int Defense {
			get { return defense; }
			set {
				defense = value;
				NotifyPropertyChanged("Defense");
			}
		}

		public int MaxHP {
			get { return maxHP; }
			set {
				maxHP = value;
				NotifyPropertyChanged("MaxHP");
			}
		}

		public int NowHP {
			get { return nowHP; }
			set {
				nowHP = value;
				NotifyPropertyChanged("NowHP");
			}
		}

		public string StatusMessage {
			get { return statusMessage; }
			set {
				statusMessage = value;
				NotifyPropertyChanged("StatusMessage");
			}
		}

		private void NotifyPropertyChanged(string parameter) {
			PropertyChanged(this, new PropertyChangedEventArgs(parameter));
		}
		public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };
	}
}
