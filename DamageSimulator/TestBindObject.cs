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
		private int antiSubKammusuString;
		private int antiSubWeaponsString;
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
				NotifyPropertyChanged("Critical");
				NotifyPropertyChanged("CriticalLabelString");
			}
		}

		public string CriticalLabelString {
			get { return (1.0 * critical / 10).ToString("0.0") + "%"; }
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

		public int DefenseString {
			get { return defense; }
			set {
				defense = value;
				NotifyPropertyChanged("DefenseString");
			}
		}

		public int MaxHPString {
			get { return maxHP; }
			set {
				maxHP = value;
				NotifyPropertyChanged("MaxHPString");
			}
		}

		public int NowHPString {
			get { return nowHP; }
			set {
				nowHP = value;
				NotifyPropertyChanged("NowHPString");
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
