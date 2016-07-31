using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
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
		Chart chart;
		SortedDictionary<int, int> sorted_hist;
		System.Random rand = new System.Random();
		bool autoCalcFlg = false;
		const int TabIndexAntiSub = 1;
		int[] loopCount = { 1000, 10000, 100000, 1000000 };

		/* メソッド */
		public MainWindow() {
			InitializeComponent();
			// チャート用の初期設定
			var windowsFormsHost1 = (WindowsFormsHost)grid_Graph.Children[0];
			chart = (Chart)windowsFormsHost1.Child;
			chart.ChartAreas.Add("ChartArea");
			// BindableNumericUpDown用の初期設定
			DataContext = new TestBindObject() { CriticalLabelString = "13.3%", AntiSubKammusuString = 94, AntiSubWeaponsString = 23 };
		}

		/// <summary>
		/// スライドバーを動かした際の処理
		/// </summary>
		private void slider_Critical_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			var bindingData = DataContext as TestBindObject;
			//bindingData.CriticalLabelString = "" + (slider_Critical.Value / 10).ToString("0.0") + "%";
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
		/// ヒストグラムの計算処理
		/// </summary>
		private void CalcHistogram() {
			try {
				var bindingData = DataContext as TestBindObject;
				// 基本攻撃力を算出する
				var baseAttackValue = 0.0;
				if(tabControl.SelectedIndex == TabIndexAntiSub) {
					// 素対潜
					baseAttackValue += Math.Sqrt(bindingData.AntiSubKammusuString) * 2;
					// 装備対潜
					baseAttackValue += 1.5 * bindingData.AntiSubWeaponsString;
					// 装備改修値
					if(comboBox_AntiSub.SelectedIndex == 0) {
						baseAttackValue += Math.Sqrt(comboBox_AntiSub_Level_0.SelectedIndex);
						baseAttackValue += Math.Sqrt(comboBox_AntiSub_Level_1.SelectedIndex);
						baseAttackValue += Math.Sqrt(comboBox_AntiSub_Level_2.SelectedIndex);
						baseAttackValue += Math.Sqrt(comboBox_AntiSub_Level_3.SelectedIndex);
					}
					// 対潜定数
					baseAttackValue += (comboBox_AntiSub.SelectedIndex == 0 ? 13 : 8);
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
				}
				//損傷補正
				if(tabControl.SelectedIndex == TabIndexAntiSub) {
					double[] param = { 1, 0.7, 0.4 };
					attackValueBeforeCap *= param[comboBox_Damage.SelectedIndex];
				}
				//交戦形態補正
				{
					// 「彩雲有り」の場合、同航戦：反航戦：丁字有利：丁字不利＝45：40：15：0
					// 「彩雲無し」の場合、同航戦：反航戦：丁字有利：丁字不利＝45：30：15：10
					// したがって、
					double[] param = { 1, 0.8, 1.2, 0.6 };
					attackValueBeforeCap *= param[comboBox_Position.SelectedIndex];
				}
				// キャップ後攻撃力を出す
				var attackValueAfterCap = attackValueBeforeCap;
				if(tabControl.SelectedIndex == TabIndexAntiSub) {
					if(attackValueAfterCap > 100.0)
						attackValueAfterCap = 100.0 + Math.Sqrt(attackValueAfterCap - 100.0);
				}
				// 最終攻撃力を出す
				double lastAttackValueWithoutCL = (int)attackValueAfterCap;
				var lastAttackValueWithCL_ = (int)attackValueAfterCap * 1.5;
				if(comboBox_AntiSub.SelectedIndex == 1) {
					// 熟練度補正
					var airLevelWeight = 1.0;
					airLevelWeight += Limit(comboBox_AntiSub_Level_0.SelectedIndex, 0, 7) * 0.2 / 7;
					airLevelWeight += Limit(comboBox_AntiSub_Level_1.SelectedIndex, 0, 7) * 0.1 / 7;
					airLevelWeight += Limit(comboBox_AntiSub_Level_2.SelectedIndex, 0, 7) * 0.1 / 7;
					airLevelWeight += Limit(comboBox_AntiSub_Level_3.SelectedIndex, 0, 7) * 0.1 / 7;
					lastAttackValueWithCL_ *= airLevelWeight;
				}
				double lastAttackValueWithCL = (int)lastAttackValueWithCL_;
				// ダメージを算出し、ヒストグラムを取る
				//初期設定
				var hist = new Dictionary<int, int>();
				var defense = int.Parse(textBox_Defense.Text);
				var nowHP = int.Parse(textBox_NowHP.Text);
				var maxHP = int.Parse(textBox_MaxHP.Text);
				double[] ammoWeight = { 1.0, 0.8, 0.4, 0.0 };
				var ammo = ammoWeight[comboBox_AmmoPer.SelectedIndex];
				//ループ
				for(int i = 0; i < loopCount[comboBox_TryCount.SelectedIndex]; ++i) {
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
						damage = (int)(nowHP * 0.5 + rand.Next(nowHP) * 0.3);
					}
					// ヒストグラムカウント
					if(hist.ContainsKey(damage)) {
						++hist[damage];
					} else {
						hist[damage] = 1;
					}
				}
				//ソート
				sorted_hist = new SortedDictionary<int, int>(hist);
				// ヒストグラムを描画させる
				DrawGraph();
			} catch { }
		}

		/// <summary>
		/// グラフを再描画する処理(中身はサンプル)
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
	}
}
