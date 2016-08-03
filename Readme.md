# KancolleDamageSimulator

## 概要
　[艦これ](http://www.dmm.com/netgame/feature/kancolle.html)用のダメージ計算機です。[ばいなり](https://twitter.com/b_inary)さんの[ダメージ計算機](http://kancollecalc.web.fc2.com/damage.html)からヒントを得て作られました。  
　戦闘時の状況を入力すると、与えられるダメージ量が[モンテカルロシミュレーション](https://ja.wikipedia.org/wiki/%E3%83%A2%E3%83%B3%E3%83%86%E3%82%AB%E3%83%AB%E3%83%AD%E6%B3%95)で計算され、グラフとして表示されます。  
　また、耐久値を入力することにより、大破率や撃沈率なども表示することができます。  
　更に、ヒストグラムの数値やグラフの画像をクリップボードにコピーしたり、保存したりすることができます。  
　なお、このソフトウェアは[WPF](https://ja.wikipedia.org/wiki/Windows_Presentation_Foundation)を用いて作られました。

## 使い方
- 本家の[ダメージ計算機](http://kancollecalc.web.fc2.com/damage.html)と操作性はほぼ一緒です
- 『攻撃側設定』グループで交戦形態などの設定を行います。右上のタブで攻撃種別が分かれています
- 『防御用設定』グループで敵装甲値などの設定を行います。艦娘相手に攻撃する際は『艦娘フラグ』にチェック！
- 『計算条件』グループで試行回数などの設定を行います。計算は軽いので、試行回数は常時100万回でいいかと
- 『計算開始』ボタンを押せば計算しますし、『自動で再計算』にチェックを入れればより便利に扱えます
- 計算し終えると、ダメージ幅や、大破率・撃沈率などが[ツールチップ](https://ja.wikipedia.org/wiki/%E3%83%84%E3%83%BC%E3%83%AB%E3%83%81%E3%83%83%E3%83%97)で表示されます
- 『計算開始』ボタンの上で右クリックすると、ヒストグラムのデータをテキストか画像でコピー or 保存できます
- ヒストグラムのテキストデータはCSV形式で、左列からダメージ値・カウント数・カウント割合・標本標準偏差・95％信頼区間(最小値―最大値)を表します


## 注意点
- **このソフトによる計算結果には一切責任を持ちません**
- 航空戦で交戦形態・攻撃陣形・損傷状態が、夜戦で交戦形態・攻撃陣形が無視されるのは**仕様です**
- 本家の[ダメージ計算機](http://kancollecalc.web.fc2.com/damage.html)と同じく、以下の項目には対応していません
 - 6-4などの、[特殊な敵地上施設](http://ja.kancolle.wikia.com/wiki/%E7%89%B9%E5%8A%B9%E8%A3%85%E5%82%99#.E9.99.B8.E4.B8.8A.E7.89.B9.E5.8A.B9)におけるダメージ処理
 - [軽巡軽量砲補正](http://ja.kancolle.wikia.com/wiki/%E3%83%80%E3%83%A1%E3%83%BC%E3%82%B8%E5%BC%8F#.E8.BB.BD.E5.B7.A1.E8.BB.BD.E9.87.8F.E7.A0.B2.E8.A3.9C.E6.AD.A3)
 - 支援艦隊
 - [PT小鬼群特効](http://ja.kancolle.wikia.com/wiki/%E7%89%B9%E5%8A%B9%E8%A3%85%E5%82%99#PT.E5.B0.8F.E9.AC.BC.E7.BE.A4.E7.89.B9.E5.8A.B9)
- また、本家の[ダメージ計算機](http://kancollecalc.web.fc2.com/damage.html)とは以下の点が異なります
 - **連合艦隊用の艦隊・陣形には対応していない**
 - [艦載機熟練度補正](http://ja.kancolle.wikia.com/wiki/%E8%89%A6%E8%BC%89%E6%A9%9F%E7%86%9F%E7%B7%B4%E5%BA%A6)において、最大値(<<)ではない場合も**攻撃力増加量が表記熟練度に比例する**と仮定している
 - [致死ダメージ置換](http://ja.kancolle.wikia.com/wiki/%E3%83%80%E3%83%A1%E3%83%BC%E3%82%B8%E7%BD%AE%E6%8F%9B)(いわゆる大破/撃沈ストッパー)に対応しているので、**艦娘に向けた攻撃でも対応可能**

## ライセンス
[MIT License](https://ja.wikipedia.org/wiki/MIT_License)


## 更新履歴
|Version|Information|
|-------|-----------|
|1.0.0|最初のリリース|
