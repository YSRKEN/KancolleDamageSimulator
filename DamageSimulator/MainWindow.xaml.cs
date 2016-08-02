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
		enum StatusIndex { NoDamage, TooSmall, Small, Middle, Large, Destroyed, NormalMin, NormalMax, CriticalMin, CriticalMax };

		/* コンストラクタ */
		public MainWindow() {
			InitializeComponent();
			// チャート用の初期設定
			var windowsFormsHost1 = (WindowsFormsHost)grid_Graph.Children[0];
			chart = (Chart)windowsFormsHost1.Child;
			chart.ChartAreas.Add("ChartArea");
			// BindableNumericUpDown用の初期設定
			DataContext = new TestBindObject() {
				AttackGun = 50,
				Torpedo = 78,
				AntiSubKammusu = 94,
				AntiSubWeapons = 23,
				Defense = 21,
				MaxHP = 27,
				NowHP = 27,
				Critical = 133,
				StatusMessage = "",
			};
		}

		/* ヒストグラム関係 */
		/// <summary>
		/// ヒストグラムの計算処理
		/// </summary>
		private void DrawHistogram() {
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
		/// ヒストグラムの文字列を作成する
		/// </summary>
		private string MakeHistText() {
			string histText = "damage,count\n";
			foreach(var pair in sorted_hist) {
				histText += "" + pair.Key + "," + pair.Value + "\n";
			}
			return histText;
		}

		/* クリック時の動作 */
		/// <summary>
		/// スライドバーを動かした際の処理
		/// </summary>
		private void slider_Critical_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			AutoDrawHistogram();
		}
		/// <summary>
		/// 「計算開始」ボタンを押した際の処理
		/// </summary>
		private void button_Run_Click(object sender, RoutedEventArgs e) {
			DrawHistogram();
		}
		/// <summary>
		/// 「自動再計算」チェックを付けた際の処理
		/// </summary>
		private void checkBox_AutoCalc_Checked(object sender, RoutedEventArgs e) {
			autoCalcFlg = true;
			AutoDrawHistogram();
		}
		/// <summary>
		/// 「自動再計算」チェックを外した際の処理
		/// </summary>
		private void checkBox_AutoCalc_Unchecked(object sender, RoutedEventArgs e) {
			autoCalcFlg = false;
		}

		/* 右クリック時の操作 */
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

		/* 自動再計算用に同一の内容が書かれている */
		private void AutoDrawHistogram() {
			if(autoCalcFlg)
				DrawHistogram();
		}
		//防御用・シミュレーション用設定
		private void NUD_Defense_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
			AutoDrawHistogram();
		}
		private void NUD_MaxHP_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            var bindData = DataContext as TestBindObject;
            if (bindData.NowHP > bindData.MaxHP) bindData.NowHP = bindData.MaxHP;
			AutoDrawHistogram();
		}
		private void NUD_NowHP_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            var bindData = DataContext as TestBindObject;
            if (bindData.NowHP > bindData.MaxHP) bindData.MaxHP = bindData.NowHP;
			AutoDrawHistogram();
		}
		private void checkBox_Kammusu_Checked(object sender, RoutedEventArgs e) {
			AutoDrawHistogram();
		}
		private void checkBox_Kammusu_Unchecked(object sender, RoutedEventArgs e) {
			AutoDrawHistogram();
		}
		private void comboBox_TryCount_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			AutoDrawHistogram();
		}
		//攻撃用設定(共通)
		private void comboBox_Position_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			AutoDrawHistogram();
		}
		private void comboBox_Formation_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			AutoDrawHistogram();
		}
		private void comboBox_Damage_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			AutoDrawHistogram();
		}
		private void comboBox_AmmoPer_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			AutoDrawHistogram();
		}
		private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			AutoDrawHistogram();
		}
		//攻撃用設定(砲撃戦)
		private void NUD_Attack_Gun_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
			AutoDrawHistogram();
		}
		private void comboBox_Watch_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			AutoDrawHistogram();
		}
		private void comboBox_Shell_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			AutoDrawHistogram();
		}
		private void checkBox_Sanshiki_Checked(object sender, RoutedEventArgs e) {
			AutoDrawHistogram();
		}
		private void checkBox_Sanshiki_Unchecked(object sender, RoutedEventArgs e) {
			AutoDrawHistogram();
		}
		private void comboBox_WG42_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			AutoDrawHistogram();
		}
		private void comboBox_Attack_Gun_Type_0_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			AutoDrawHistogram();
		}
		private void comboBox_Attack_Gun_Type_1_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			AutoDrawHistogram();
		}
		private void comboBox_Attack_Gun_Type_2_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			AutoDrawHistogram();
		}
		private void comboBox_Attack_Gun_Type_3_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			AutoDrawHistogram();
		}
		private void comboBox_Attack_Gun_Level_0_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			AutoDrawHistogram();
		}
		private void comboBox_Attack_Gun_Level_1_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			AutoDrawHistogram();
		}
		private void comboBox_Attack_Gun_Level_2_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			AutoDrawHistogram();
		}
		private void comboBox_Attack_Gun_Level_3_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			AutoDrawHistogram();
		}
		//攻撃用設定(雷撃)
		private void NUD_Torpedo_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
			AutoDrawHistogram();
		}
		private void checkBox_FirstTorpedo_Checked(object sender, RoutedEventArgs e) {
			AutoDrawHistogram();
		}
		private void checkBox_FirstTorpedo_Unchecked(object sender, RoutedEventArgs e) {
			AutoDrawHistogram();
		}
		private void comboBox_Torpedo_Level_0_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			AutoDrawHistogram();
		}
		private void comboBox_Torpedo_Level_1_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			AutoDrawHistogram();
		}
		private void comboBox_Torpedo_Level_2_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			AutoDrawHistogram();
		}
		private void comboBox_Torpedo_Level_3_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			AutoDrawHistogram();
		}
		//攻撃用設定(対潜)
		private void textBox_AntiSub_Kammusu_TextChanged(object sender, TextChangedEventArgs e) {
			AutoDrawHistogram();
		}
		private void textBox_AntiSub_Weapons_TextChanged(object sender, TextChangedEventArgs e) {
			AutoDrawHistogram();
		}
		private void comboBox_AntiSub_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			AutoDrawHistogram();
		}
		private void checkBox_AntiSubSynergy_Checked(object sender, RoutedEventArgs e) {
			AutoDrawHistogram();
		}
		private void checkBox_AntiSubSynergy_Unchecked(object sender, RoutedEventArgs e) {
			AutoDrawHistogram();
		}
		private void comboBox_AntiSub_Level_0_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			AutoDrawHistogram();
		}
		private void comboBox_AntiSub_Level_1_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			AutoDrawHistogram();
		}
		private void comboBox_AntiSub_Level_2_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			AutoDrawHistogram();
		}
		private void comboBox_AntiSub_Level_3_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			AutoDrawHistogram();
		}
		private void NUD_AntiSubKammusu_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
			AutoDrawHistogram();
		}
		private void NUD_AntiSubKammusu_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
			AutoDrawHistogram();
		}
		private void NUD_AntiSubWeapons_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
			AutoDrawHistogram();
		}

		/* 計算用各種関数 */
		/// <summary>
		/// 計算処理
		/// </summary>
		private void CalcData() {
			// 「彩雲有り」「彩雲無し」のため、処理を分割する
			var hist = new Dictionary<int, int>();  //ヒストグラム(元)
			var count = loopCount[comboBox_TryCount.SelectedIndex];
			int[] status = {					//状態カウント
				0, 0, 0, 0, 0, 0,				//無傷～撃沈回数
				count + 1, -1, count + 1, -1 };	//通常攻撃の下限と上限、クリティカルの下限と上限
				//彩雲が絡む場合、確率毎に按分する
				//「彩雲有り」の場合、同航戦：反航戦：丁字有利：丁字不利＝45：40：15：0
				//「彩雲無し」の場合、同航戦：反航戦：丁字有利：丁字不利＝45：30：15：10
			switch(comboBox_Position.SelectedIndex) {
			case 4:
				CalcDataSlave(0, hist, status, count * 45 / 100);
				CalcDataSlave(1, hist, status, count * 40 / 100);
				CalcDataSlave(2, hist, status, count * 15 / 100);
				break;
			case 5:
				CalcDataSlave(0, hist, status, count * 45 / 100);
				CalcDataSlave(1, hist, status, count * 30 / 100);
				CalcDataSlave(2, hist, status, count * 15 / 100);
				CalcDataSlave(3, hist, status, count * 10 / 100);
				break;
			default:
				CalcDataSlave(comboBox_Position.SelectedIndex, hist, status, count);
				break;
			}
			//ソート
			sorted_hist = new SortedDictionary<int, int>(hist);
			//状態messageの更新
			var bindData = DataContext as TestBindObject;
			bindData.StatusMessage = "通常ダメージ：" + status[(int)StatusIndex.NormalMin] + "～" + status[(int)StatusIndex.NormalMax] + "\n";
			bindData.StatusMessage += "クリティカル：" + status[(int)StatusIndex.CriticalMin] + "～" + status[(int)StatusIndex.CriticalMax] + "\n";
			bindData.StatusMessage += "無傷率：" + Math.Round(100.0 * status[(int)StatusIndex.NoDamage] / count, 1) + "%\n";
			bindData.StatusMessage += "カスダメ率：" + Math.Round(100.0 * status[(int)StatusIndex.TooSmall] / count, 1) + "%\n";
			bindData.StatusMessage += "小破率：" + Math.Round(100.0 * status[(int)StatusIndex.Small] / count, 1) + "%\n";
			bindData.StatusMessage += "中破率：" + Math.Round(100.0 * status[(int)StatusIndex.Middle] / count, 1) + "%\n";
			bindData.StatusMessage += "大破率：" + Math.Round(100.0 * status[(int)StatusIndex.Large] / count, 1) + "%\n";
			bindData.StatusMessage += "撃沈率：" + Math.Round(100.0 * status[(int)StatusIndex.Destroyed] / count, 1) + "%";
		}
		/// <summary>
		/// 計算処理(分割後)
		/// </summary>
		private void CalcDataSlave(int type, Dictionary<int, int> hist, int[] status, int loops) {
			// 基本攻撃力を算出する
			var baseAttackValue = CalcBaseAttack();
			// キャップ前攻撃力を出す
			var attackValueBeforeCap = CalcAttackBeforeCap(baseAttackValue, type);
			// キャップ後攻撃力を出す
			var attackValueAfterCap = CalcAttackAfterCap(attackValueBeforeCap);
			// 最終攻撃力を出す
			var lastAttackValue = CalcLastAttack(attackValueAfterCap);
			// ダメージを算出し、ヒストグラムを取る
			CalcHistogram(lastAttackValue, hist, status, loops);
		}
		/// <summary>
		/// 基本攻撃力を出す
		/// </summary>
		private double CalcBaseAttack() {
			var bindData = DataContext as TestBindObject;
			var baseAttackValue = 0.0;
			switch(tabControl.SelectedIndex) {
			case TabIndexGun:
				baseAttackValue = bindData.AttackGun + 5;
				// 装備改修値
				double[] param = { 1.0,  1.5, 0.75};
				baseAttackValue += param[comboBox_Attack_Gun_Type_0.SelectedIndex] * Math.Sqrt(comboBox_Attack_Gun_Level_0.SelectedIndex);
				baseAttackValue += param[comboBox_Attack_Gun_Type_1.SelectedIndex] * Math.Sqrt(comboBox_Attack_Gun_Level_1.SelectedIndex);
				baseAttackValue += param[comboBox_Attack_Gun_Type_2.SelectedIndex] * Math.Sqrt(comboBox_Attack_Gun_Level_2.SelectedIndex);
				baseAttackValue += param[comboBox_Attack_Gun_Type_3.SelectedIndex] * Math.Sqrt(comboBox_Attack_Gun_Level_3.SelectedIndex);
				break;
			case TabIndexGunAir:
				break;
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
			case TabIndexAir:
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
			case TabIndexNight:
				break;
			}
			return baseAttackValue;
		}
		/// <summary>
		/// キャップ前攻撃力を出す
		/// </summary>
		private double CalcAttackBeforeCap(double baseAttackValue, int type) {
			var bindData = DataContext as TestBindObject;
			var attackValueBeforeCap = baseAttackValue;
			// 三式弾特効
			if((bool)checkBox_Sanshiki.IsChecked)
				attackValueBeforeCap *= 2.5;
			// WG42特効
			{
				double[] param = { 0, 75, 110, 140, 160 };
				attackValueBeforeCap += param[comboBox_WG42.SelectedIndex];
			}
			// キャップ前補正
			//交戦形態補正
			{
				double[] param = { 1, 0.8, 1.2, 0.6 };
				attackValueBeforeCap *= param[type];
			}
			//陣形補正
			if(tabControl.SelectedIndex == TabIndexAntiSub) {
				double[] param = { 0.6, 0.8, 1.2, 1.0, 1.3 };
				attackValueBeforeCap *= param[comboBox_Formation.SelectedIndex];
			} else {
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
			//対潜シナジー補正
			if(tabControl.SelectedIndex == TabIndexAntiSub) {
				if((bool)checkBox_AntiSubSynergy.IsChecked)
					attackValueBeforeCap *= 1.15;
			}
			//夜戦特殊攻撃補正
			if(tabControl.SelectedIndex == TabIndexNight) {
				
			}
			return attackValueBeforeCap;
		}
		/// <summary>
		/// キャップ後攻撃力を出す
		/// </summary>
		private double CalcAttackAfterCap(double attackValueBeforeCap) {
			var attackValueAfterCap = 0.0;
			if(tabControl.SelectedIndex == TabIndexAntiSub) {
				attackValueAfterCap = CalcCap(attackValueBeforeCap, 100.0);
			} else if(tabControl.SelectedIndex == TabIndexNight) {
				attackValueAfterCap = CalcCap(attackValueBeforeCap, 300.0);
			} else {
				attackValueAfterCap = CalcCap(attackValueBeforeCap, 150.0);
			}
			return attackValueAfterCap;
		}
		/// <summary>
		/// 最終攻撃力を出す
		/// </summary>
		private double[] CalcLastAttack(double attackValueAfterCap) {
			double lastAttackValue = (int)attackValueAfterCap;
			// 徹甲弾特効
			{
				double[] param = { 1.0, 1.08, 1.1, 1.15, 1.15};
				lastAttackValue = (int)(lastAttackValue * param[comboBox_Shell.SelectedIndex]);
			}
			// クリティカル補正
			double lastAttackValueWithoutCL = (int)lastAttackValue;
			var lastAttackValueWithCL = (int)lastAttackValue * 1.5;
			// 熟練度補正
			switch(tabControl.SelectedIndex) {
			case TabIndexGunAir:
				break;
			case TabIndexAir:
				break;
			case TabIndexAntiSub:
				if(comboBox_AntiSub.SelectedIndex == 1) {
					// 熟練度補正
					var airLevelWeight = 1.0;
					airLevelWeight += Limit(comboBox_AntiSub_Level_0.SelectedIndex, 0, 7) * 0.2 / 7;
					airLevelWeight += Limit(comboBox_AntiSub_Level_1.SelectedIndex, 0, 7) * 0.1 / 7;
					airLevelWeight += Limit(comboBox_AntiSub_Level_2.SelectedIndex, 0, 7) * 0.1 / 7;
					airLevelWeight += Limit(comboBox_AntiSub_Level_3.SelectedIndex, 0, 7) * 0.1 / 7;
					lastAttackValueWithoutCL = (int)(lastAttackValueWithoutCL * airLevelWeight);
					lastAttackValueWithCL = (int)(lastAttackValueWithCL * airLevelWeight);
				}
				break;
			}
			// 触接・弾着補正
			switch(tabControl.SelectedIndex) {
			case TabIndexGun:
				{
					double[] param = { 1.0, 1.5, 1.3, 1.2, 1.1, 1.2 };
					lastAttackValueWithoutCL = lastAttackValueWithoutCL * param[comboBox_Watch.SelectedIndex];
					lastAttackValueWithCL = lastAttackValueWithCL * param[comboBox_Watch.SelectedIndex];
				}
				break;
			case TabIndexAir:
				break;
			}
			return new double[] { lastAttackValueWithoutCL , lastAttackValueWithCL };
		}
		/// <summary>
		/// ヒストグラムを出す
		/// </summary>
		private void CalcHistogram(double[] lastAttackValue, Dictionary<int, int> hist, int[] status, int loops) {
			var bindData = DataContext as TestBindObject;
			var lastAttackValueWithoutCL = lastAttackValue[0];
			var lastAttackValueWithCL = lastAttackValue[1];
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
				bool criticalFlg = false;
				if(rand.NextDouble() * 1000 < slider_Critical.Value) {
					// クリティカル
					criticalFlg = true;
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
					++status[(int)StatusIndex.NoDamage];
				} else if(nowHP2 * 4 > maxHP) {
					++status[(int)StatusIndex.TooSmall];
				} else if(nowHP2 * 2 > maxHP) {
					++status[(int)StatusIndex.Small];
				} else if(nowHP2 * 4 > maxHP) {
					++status[(int)StatusIndex.Middle];
				} else if(nowHP2 > 0) {
					++status[(int)StatusIndex.Large];
				} else {
					++status[(int)StatusIndex.Destroyed];
				}
				// 上下限カウント
				if(criticalFlg) {
					status[(int)StatusIndex.CriticalMin] = Math.Min(status[(int)StatusIndex.CriticalMin], damage);
					status[(int)StatusIndex.CriticalMax] = Math.Max(status[(int)StatusIndex.CriticalMax], damage);
				} else {
					status[(int)StatusIndex.NormalMin] = Math.Min(status[(int)StatusIndex.NormalMin], damage);
					status[(int)StatusIndex.NormalMax] = Math.Max(status[(int)StatusIndex.NormalMax], damage);
				}
			}
			// 特殊処理
			if(slider_Critical.Value == 0) {
				status[(int)StatusIndex.CriticalMin] = status[(int)StatusIndex.CriticalMax] = - 1;
			}
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
	}
}
