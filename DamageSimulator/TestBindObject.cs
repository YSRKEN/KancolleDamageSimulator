/* WPFΕNumericUpDownπg€ _ tocsworld
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
		/* U€έθ(Cν) */
		private int attackGun;
		public int AttackGun {
			get { return attackGun; }
			set {
				attackGun = value;
				NotifyPropertyChanged("AttackGun");
			}
		}

		/* U€έθ(Cν(σκn)) */
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

		/* U€έθ() */
		private int torpedo;
		public int Torpedo {
			get { return torpedo; }
			set {
				torpedo = value;
				NotifyPropertyChanged("Torpedo");
			}
		}

		/* U€έθ(qσν) */
		private int powerAir;
		public int PowerAir {
			get { return powerAir; }
			set {
				powerAir = value;
				NotifyPropertyChanged("PowerAir");
			}
		}
		private int slotsAir;
		public int SlotsAir {
			get { return slotsAir; }
			set {
				slotsAir = value;
				NotifyPropertyChanged("SlotsAir");
			}
		}

		/* U€έθ(ΞφU) */
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

		/* U€έθ(ιν) */
		private int attackNight;
		public int AttackNight {
			get { return attackNight; }
			set {
				attackNight = value;
				NotifyPropertyChanged("AttackNight");
			}
		}
		private int torpedoNight;
		public int TorpedoNight {
			get { return torpedoNight; }
			set {
				torpedoNight = value;
				NotifyPropertyChanged("TorpedoNight");
			}
		}

		/* hδpέθ */
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

		/* vZbgpR{{bNX */
		private List<string> shipTypeList;
		public List<string> ShipTypeList
		{
			get { return shipTypeList; }
			set { shipTypeList = value; }
		}
		private List<string> shipClassList;
		public List<string> ShipClassList {
			get {
				return shipClassList;
			}
			set {
				shipClassList = value;
				NotifyPropertyChanged("ShipClassList");
			}
		}
		private List<string> shipNameList;
		public List<string> ShipNameList {
			get { return shipNameList; }
			set {
				shipNameList = value;
				NotifyPropertyChanged("ShipNameList");
			}
		}
		private string hunterName;
		public string HunterName {
			get { return hunterName; }
			set {
				hunterName = value;
				NotifyPropertyChanged("HunterName");
			}
		}
		private string targetName;
		public string TargetName {
			get { return targetName; }
			set {
				targetName = value;
				NotifyPropertyChanged("TargetName");
			}
		}

		/* »ΜΌ */
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
		private int ammoParam;
		public int AmmoParam {
			get { return ammoParam; }
			set {
				ammoParam = value;
				NotifyPropertyChanged("AmmoParam");
			}
		}

		private void NotifyPropertyChanged(string parameter) {
			PropertyChanged(this, new PropertyChangedEventArgs(parameter));
		}
		public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };
	}
}
