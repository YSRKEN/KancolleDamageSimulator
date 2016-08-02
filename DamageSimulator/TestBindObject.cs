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
		/* UŒ‚‘¤Ý’è(–CŒ‚í) */
		private int attackGun;
		public int AttackGun {
			get { return attackGun; }
			set {
				attackGun = value;
				NotifyPropertyChanged("AttackGun");
			}
		}

		/* UŒ‚‘¤Ý’è(–CŒ‚í(‹ó•êŒn)) */
		private int attackGunAir;
		public int AttackGunAir {
			get { return attackGunAir; }
			set {
				attackGunAir = value;
				NotifyPropertyChanged("AttackGunAir");
			}
		}
		private int bombGunAir;
		public int BombGunAir {
			get { return bombGunAir; }
			set {
				bombGunAir = value;
				NotifyPropertyChanged("BombGunAir");
			}
		}
		private int torpedoGunAir;
		public int TorpedoGunAir {
			get { return torpedoGunAir; }
			set {
				torpedoGunAir = value;
				NotifyPropertyChanged("TorpedoGunAir");
			}
		}

		/* UŒ‚‘¤Ý’è(—‹Œ‚) */
		private int torpedo;
		public int Torpedo {
			get { return torpedo; }
			set {
				torpedo = value;
				NotifyPropertyChanged("Torpedo");
			}
		}

		/* UŒ‚‘¤Ý’è(‘ÎöUŒ‚) */
		private int antiSubKammusu;
		public int AntiSubKammusu {
			get { return antiSubKammusu; }
			set {
				antiSubKammusu = value;
				NotifyPropertyChanged("AntiSubKammusu");
			}
		}
		private int antiSubWeapons;
		public int AntiSubWeapons {
			get { return antiSubWeapons; }
			set {
				antiSubWeapons = value;
				NotifyPropertyChanged("AntiSubWeapons");
			}
		}

		/* –hŒä—pÝ’è */
		private int defense;
		public int Defense {
			get { return defense; }
			set {
				defense = value;
				NotifyPropertyChanged("Defense");
			}
		}
		private int maxHP;
		public int MaxHP {
			get { return maxHP; }
			set {
				maxHP = value;
				NotifyPropertyChanged("MaxHP");
			}
		}
		private int nowHP;
		public int NowHP {
			get { return nowHP; }
			set {
				nowHP = value;
				NotifyPropertyChanged("NowHP");
			}
		}

		/* ‚»‚Ì‘¼ */
		private int critical;
		public int Critical {
			get { return critical; }
			set {
				critical = value;
				NotifyPropertyChanged("CriticalLabel");
			}
		}
		public string CriticalLabel {
			get { return (1.0 * critical / 10).ToString("0.0") + "%"; }
		}
		private string statusMessage;
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
