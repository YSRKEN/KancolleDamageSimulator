using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BindableWinFormsControl {
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window {
		/* メンバ変数 */
		//変数
		bool autoCalcFlg = false;	//自動再計算フラグ
		Chart chart;				//グラフ
		SortedDictionary<int, int> sorted_hist;	//ヒストグラム
		//定数
		System.Random rand = new System.Random();	//乱数シード
		const int TabIndexGun     = 0;	//砲撃戦
		const int TabIndexGunAir  = 1;	//砲撃戦(空母)
		const int TabIndexTorpedo = 2;	//雷撃戦
		const int TabIndexAir     = 3;	//航空戦
		const int TabIndexAntiSub = 4;	//対潜攻撃
		const int TabIndexNight   = 5;	//夜戦
		int[] loopCount = { 1000, 10000, 100000, 1000000 };	//ループカウント

		/* メソッド */
		public MainWindow() {
			InitializeComponent();
			// チャート用の初期設定
			var windowsFormsHost1 = (WindowsFormsHost)grid_Graph.Children[0];
			chart = (Chart)windowsFormsHost1.Child;
			chart.ChartAreas.Add("ChartArea");
			// BindableNumericUpDown用の初期設定
			DataContext = new TestBindObject() {
				Critical = 133,
				AntiSubKammusu = 94,
				AntiSubWeapons = 23,
				Defense = 21,
				MaxHP = 27,
				NowHP = 27,
				StatusMessage = "",
				Torpedo = 78
			};
		}

		/// <summary>
		/// スライドバーを動かした際の処理
		/// </summary>
		private void slider_Critical_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			if(autoCalcFlg) {
				CalcHistogram();
			}
		}

		/// <summary>
		/// 「計算開始」ボタンを押した際の処理
		/// </summary>
		private void button_Run_Click(object sender, RoutedEventArgs e) {
			CalcHistogram();
		}

		/// <summary>
		/// 計算処理(分割後)
		/// </summary>
		private void CalcDataSlave(Dictionary<int, int> hist, int[] status, int type, int loops) {
			var bindData = DataContext as TestBindObject;
			// 基本攻撃力を算出する
			var baseAttackValue = 0.0;
			switch(tabControl.SelectedIndex) {
			case TabIndexTorpedo:
				baseAttackValue = bindData.Torpedo + 5;
				// 装備改修値
				if(comboBox_AntiSub.SelectedIndex == 0) {
					baseAttackValue += 1.2 * Math.Sqrt(comboBox_Torpedo_Level_0.SelectedIndex);
					baseAttackValue += 1.2 * Math.Sqrt(comboBox_Torpedo_Level_1.SelectedIndex);
					baseAttackValue += 1.2 * Math.Sqrt(comboBox_Torpedo_Level_2.SelectedIndex);
					baseAttackValue += 1.2 * Math.Sqrt(comboBox_Torpedo_Level_3.SelectedIndex);
				}
				break;
			case TabIndexAntiSub:
				// 素対潜
				baseAttackValue += Math.Sqrt(bindData.AntiSubKammusu) * 2;
				// 装備対潜
				baseAttackValue += 1.5 * bindData.AntiSubWeapons;
				// 装備改修値
				if(comboBox_AntiSub.SelectedIndex == 0) {
					baseAttackValue += Math.Sqrt(comboBox_AntiSub_Level_0.SelectedIndex);
					baseAttackValue += Math.Sqrt(comboBox_AntiSub_Level_1.SelectedIndex);
					baseAttackValue += Math.Sqrt(comboBox_AntiSub_Level_2.SelectedIndex);
					baseAttackValue += Math.Sqrt(comboBox_AntiSub_Level_3.SelectedIndex);
				}
				// 対潜定数
				baseAttackValue += (comboBox_AntiSub.SelectedIndex == 0 ? 13 : 8);
				break;
			}
			// キャップ前攻撃力を出す
			var attackValueBeforeCap = baseAttackValue;
			//対潜シナジー補正
			if(tabControl.SelectedIndex == TabIndexAntiSub) {
				if((bool)checkBox_AntiSubSynergy.IsChecked)
					attackValueBeforeCap *= 1.15;
			}
			//陣形補正
			if(tabControl.SelectedIndex == TabIndexAntiSub) {
				double[] param = { 0.6, 0.8, 1.2, 1.0, 1.3 };
				attackValueBeforeCap *= param[comboBox_Formation.SelectedIndex];
			}else {
				double[] param = { 1.0, 0.8, 0.7, 0.6, 0.6 };
				attackValueBeforeCap *= param[comboBox_Formation.SelectedIndex];
			}
			//損傷補正
			if(tabControl.SelectedIndex == TabIndexTorpedo && (bool)checkBox_FirstTorpedo.IsChecked) {
				double[] param = { 1, 0.8, 0.0 };
				attackValueBeforeCap *= param[comboBox_Damage.SelectedIndex];
			} else {
				double[] param = { 1, 0.7, 0.4 };
				attackValueBeforeCap *= param[comboBox_Damage.SelectedIndex];
			}
			//交戦形態補正
			{
				double[] param = { 1, 0.8, 1.2, 0.6 };
				attackValueBeforeCap *= param[type];
			}
			// キャップ後攻撃力を出す
			var attackValueAfterCap = 0.0;
			if(tabControl.SelectedIndex == TabIndexAntiSub) {
				attackValueAfterCap = CalcCap(attackValueBeforeCap, 100.0);
			}else {
				attackValueAfterCap = CalcCap(attackValueBeforeCap, 150.0);
			}
			// 最終攻撃力を出す
			double lastAttackValueWithoutCL = (int)attackValueAfterCap;
			var lastAttackValueWithCL_ = (int)attackValueAfterCap * 1.5;
			if(tabControl.SelectedIndex == TabIndexAntiSub) {
				if(comboBox_AntiSub.SelectedIndex == 1) {
					// 熟練度補正
					var airLevelWeight = 1.0;
					airLevelWeight += Limit(comboBox_AntiSub_Level_0.SelectedIndex, 0, 7) * 0.2 / 7;
					airLevelWeight += Limit(comboBox_AntiSub_Level_1.SelectedIndex, 0, 7) * 0.1 / 7;
					airLevelWeight += Limit(comboBox_AntiSub_Level_2.SelectedIndex, 0, 7) * 0.1 / 7;
					airLevelWeight += Limit(comboBox_AntiSub_Level_3.SelectedIndex, 0, 7) * 0.1 / 7;
					lastAttackValueWithCL_ *= airLevelWeight;
				}
			}
			double lastAttackValueWithCL = (int)lastAttackValueWithCL_;
			// ダメージを算出し、ヒストグラムを取る
			//初期設定
			var defense = bindData.Defense;
			var nowHP = bindData.NowHP;
			var maxHP = bindData.MaxHP;
			double[] ammoWeight = { 1.0, 0.8, 0.4, 0.0 };
			var ammo = ammoWeight[comboBox_AmmoPer.SelectedIndex];
			//ループ
			for(int i = 0; i < loops; ++i) {
				// ダメージ計算
				var defenseValue = defense * 0.7 + rand.Next(defense) * 0.6;
				int damage;
				if(rand.NextDouble() * 1000 < slider_Critical.Value) {
					// クリティカル
					damage = (int)((lastAttackValueWithCL - defenseValue) * ammo);
				} else {
					// 非クリティカル
					damage = (int)((lastAttackValueWithoutCL - defenseValue) * ammo);
				}
				//0以下の際の処理(カスダメ)
				if(damage <= 0) {
					damage = (int)(nowHP * 0.06 + rand.Next(nowHP) * 0.08);
				}
				//艦娘で、更に現在耐久以上だった際の処理(大破ストッパー)
				if(damage >= nowHP && (bool)checkBox_Kammusu.IsChecked) {
					//大破：最大耐久の25％「以下」
					if(nowHP * 4 > maxHP) {
						damage = (int)(nowHP * 0.5 + rand.Next(nowHP) * 0.3);
					}
				}
				// ヒストグラムカウント
				if(hist.ContainsKey(damage)) {
					++hist[damage];
				} else {
					hist[damage] = 1;
				}
				// 状態カウント
				var nowHP2 = nowHP - damage;
				if(nowHP == nowHP2) {
					++status[0];
				} else if(nowHP2 * 4 > maxHP) {
					++status[1];
				} else if(nowHP2 * 2 > maxHP) {
					++status[2];
				} else if(nowHP2 * 4 > maxHP) {
					++status[3];
				} else if(nowHP2 > 0) {
					++status[4];
				} else {
					++status[5];
				}
			}
		}

		/// <summary>
		/// 計算処理
		/// </summary>
		private void CalcData() {
			// 「彩雲有り」「彩雲無し」のため、処理を分割する
			var hist = new Dictionary<int, int>();	//ヒストグラム(元)
			int[] status = { 0, 0, 0, 0, 0, 0 };    //状態カウント
			//彩雲が絡む場合、確率毎に按分する
			//「彩雲有り」の場合、同航戦：反航戦：丁字有利：丁字不利＝45：40：15：0
			//「彩雲無し」の場合、同航戦：反航戦：丁字有利：丁字不利＝45：30：15：10
			var count = loopCount[comboBox_TryCount.SelectedIndex];
			switch(comboBox_Position.SelectedIndex) {
			case 4:
				CalcDataSlave(hist, status, 0, count * 45 / 100);
				CalcDataSlave(hist, status, 1, count * 40 / 100);
				CalcDataSlave(hist, status, 2, count * 15 / 100);
				break;
			case 5:
				CalcDataSlave(hist, status, 0, count * 45 / 100);
				CalcDataSlave(hist, status, 1, count * 30 / 100);
				CalcDataSlave(hist, status, 2, count * 15 / 100);
				CalcDataSlave(hist, status, 3, count * 10 / 100);
				break;
			default:
				CalcDataSlave(hist, status, comboBox_Position.SelectedIndex, count);
				break;
			}
			//ソート
			sorted_hist = new SortedDictionary<int, int>(hist);
			//状態messageの更新
			var bindData = DataContext as TestBindObject;
			bindData.StatusMessage = "無傷率：" + Math.Round(100.0 * status[0] / count, 1) + "%\n";
			bindData.StatusMessage += "カスダメ率：" + Math.Round(100.0 * status[1] / count, 1) + "%\n";
			bindData.StatusMessage += "小破率：" + Math.Round(100.0 * status[2] / count, 1) + "%\n";
			bindData.StatusMessage += "中破率：" + Math.Round(100.0 * status[3] / count, 1) + "%\n";
			bindData.StatusMessage += "大破率：" + Math.Round(100.0 * status[4] / count, 1) + "%\n";
			bindData.StatusMessage += "撃沈率：" + Math.Round(100.0 * status[5] / count, 1) + "%";
		}

		/// <summary>
		/// ヒストグラムの計算処理
		/// </summary>
		private void CalcHistogram() {
			try {
				// 計算処理
				CalcData();
				// ヒストグラムを描画させる
				DrawGraph();
			} catch { }
		}

		/// <summary>
		/// グラフを再描画する処理
		/// </summary>
		private void DrawGraph() {
			// スケールの最大・最小値を計算する
			double axisX_Max = double.MinValue, axisX_Min = double.MaxValue, axisY_Max = double.MinValue, axisY_Min = double.MaxValue;
			foreach(var pair in sorted_hist) {
				var valuePer = 100.0 * pair.Value / loopCount[comboBox_TryCount.SelectedIndex];
				axisX_Max = Math.Max(axisX_Max, pair.Key);
				axisX_Min = Math.Min(axisX_Min, pair.Key);
				axisY_Max = Math.Max(axisY_Max, valuePer);
				axisY_Min = Math.Min(axisY_Min, valuePer);
			}
			axisX_Min = Math.Floor(axisX_Min);
			axisX_Max = Math.Ceiling(axisX_Max);
			axisY_Min = Math.Floor(axisY_Min);
			axisY_Max = Math.Ceiling(axisY_Max);
			// チャートにグラフを追加する
			chart.Series.Clear();
			var seriesHist = new Series();
			seriesHist.ChartType = SeriesChartType.Line;
			seriesHist.MarkerStyle = MarkerStyle.Circle;
			foreach(var pair in sorted_hist) {
				// 値を取得しつつ、スケールの最大・最小値を計算する
				var valuePer = 100.0 * pair.Value / loopCount[comboBox_TryCount.SelectedIndex];
				seriesHist.Points.AddXY(pair.Key, valuePer);
			}
			chart.Series.Add(seriesHist);
			chart.ChartAreas[0].AxisX.Minimum = axisX_Min;
			chart.ChartAreas[0].AxisX.Maximum = axisX_Max;
			chart.ChartAreas[0].AxisY.Minimum = 0.0;
			chart.ChartAreas[0].AxisY.Maximum = axisY_Max;
		}

		/// <summary>
		/// 「自動再計算」チェックを付けた際の処理
		/// </summary>
		private void checkBox_AutoCalc_Checked(object sender, RoutedEventArgs e) {
			autoCalcFlg = true;
			if(autoCalcFlg) {
				CalcHistogram();
			}
		}

		/// <summary>
		/// 「自動再計算」チェックを外した際の処理
		/// </summary>
		private void checkBox_AutoCalc_Unchecked(object sender, RoutedEventArgs e) {
			autoCalcFlg = false;
		}

		/// <summary>
		/// 値を制限する
		/// </summary>
		private int Limit(int a, int min, int max) {
			if(a < min)
				return min;
			if(a > max)
				return max;
			return a;
		}

		/// <summary>
		/// キャップ計算
		/// </summary>
		private double CalcCap(double x, double cap) {
			if(x < cap)
				return x;
			else
				return cap + Math.Sqrt(x - cap);
		}


		/// <summary>
		/// ヒストグラムの文字列を作成する
		/// </summary>
		private string MakeHistText() {
			string histText = "damage,count\n";
			foreach(var pair in sorted_hist) {
				histText += "" + pair.Key + "," + pair.Value + "\n";
			}
			return histText;
		}

		private void comboBox_Position_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if(autoCalcFlg) {
				CalcHistogram();
			}
		}

		private void comboBox_Formation_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if(autoCalcFlg) {
				CalcHistogram();
			}
		}

		private void comboBox_Damage_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if(autoCalcFlg) {
				CalcHistogram();
			}
		}

		private void comboBox_AmmoPer_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if(autoCalcFlg) {
				CalcHistogram();
			}
		}

		private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if(autoCalcFlg) {
				CalcHistogram();
			}
		}

		private void textBox_AntiSub_Kammusu_TextChanged(object sender, TextChangedEventArgs e) {
			if(autoCalcFlg) {
				CalcHistogram();
			}
		}

		private void textBox_AntiSub_Weapons_TextChanged(object sender, TextChangedEventArgs e) {
			if(autoCalcFlg) {
				CalcHistogram();
			}
		}

		private void comboBox_AntiSub_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if(autoCalcFlg) {
				CalcHistogram();
			}
		}

		private void checkBox_AntiSubSynergy_Checked(object sender, RoutedEventArgs e) {
			if(autoCalcFlg) {
				CalcHistogram();
			}
		}

		private void checkBox_AntiSubSynergy_Unchecked(object sender, RoutedEventArgs e) {
			if(autoCalcFlg) {
				CalcHistogram();
			}
		}

		private void comboBox_AntiSub_Level_0_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if(autoCalcFlg) {
				CalcHistogram();
			}
		}

		private void comboBox_AntiSub_Level_1_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if(autoCalcFlg) {
				CalcHistogram();
			}
		}

		private void comboBox_AntiSub_Level_2_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if(autoCalcFlg) {
				CalcHistogram();
			}
		}

		private void comboBox_AntiSub_Level_3_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if(autoCalcFlg) {
				CalcHistogram();
			}
		}

		private void textBox_Defense_TextChanged(object sender, TextChangedEventArgs e) {
			if(autoCalcFlg) {
				CalcHistogram();
			}
		}

		private void textBox_MaxHP_TextChanged(object sender, TextChangedEventArgs e) {
			if(autoCalcFlg) {
				CalcHistogram();
			}
		}

		private void textBox_NowHP_TextChanged(object sender, TextChangedEventArgs e) {
			if(autoCalcFlg) {
				CalcHistogram();
			}
		}

		private void checkBox_Kammusu_Checked(object sender, RoutedEventArgs e) {
			if(autoCalcFlg) {
				CalcHistogram();
			}
		}

		private void checkBox_Kammusu_Unchecked(object sender, RoutedEventArgs e) {
			if(autoCalcFlg) {
				CalcHistogram();
			}
		}

		private void comboBox_TryCount_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if(autoCalcFlg) {
				CalcHistogram();
			}
		}

		private void CopyHistText_Click(object sender, RoutedEventArgs e) {
			var histText = MakeHistText();
			System.Windows.Clipboard.SetText(histText);
		}

		private void CopyHistPic_Click(object sender, RoutedEventArgs e) {
			var stream = new System.IO.MemoryStream();
			chart.SaveImage(stream, System.Drawing.Imaging.ImageFormat.Bmp);
			var bmp = new System.Drawing.Bitmap(stream);
			System.Windows.Clipboard.SetDataObject(bmp);
		}

		private void SaveHistText_Click(object sender, RoutedEventArgs e) {
			var histText = MakeHistText();
			var sfd = new SaveFileDialog();
			sfd.FileName = "hist.csv";
			sfd.Filter = "CSVファイル(*.csv)|*.csv|すべてのファイル(*.*)|*.*";
			sfd.ShowDialog();
			if(sfd.FileName != "") {
				var stream = sfd.OpenFile();
				if(stream != null) {
					//ファイルに書き込む
					var sw = new System.IO.StreamWriter(stream);
					sw.Write(histText);
					//閉じる
					sw.Close();
					stream.Close();
				}
			}
		}

		private void SaveHistPic_Click(object sender, RoutedEventArgs e) {
			var sfd = new SaveFileDialog();
			sfd.FileName = "hist.png";
			sfd.Filter = "PNGファイル(*.png)|*.png|すべてのファイル(*.*)|*.*";
			sfd.ShowDialog();
			if(sfd.FileName != "") {
				chart.SaveImage(sfd.FileName, System.Drawing.Imaging.ImageFormat.Png);
			}
		}

		private void NUD_AntiSubKammusu_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {

		}

		private void NUD_AntiSubKammusu_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
			if(autoCalcFlg) {
				CalcHistogram();
			}
		}

		private void NUD_AntiSubWeapons_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
			if(autoCalcFlg) {
				CalcHistogram();
			}
		}

		private void NUD_Defense_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
			if(autoCalcFlg) {
				CalcHistogram();
			}
		}

		private void NUD_MaxHP_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
			if(autoCalcFlg) {
				CalcHistogram();
			}
		}

		private void NUD_NowHP_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
			if(autoCalcFlg) {
				CalcHistogram();
			}
		}
	}
}
