using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Minesweeper
{
    public partial class Form1 : Form
    {
        #region private fields
        /// <summary>
        /// 行の数
        /// </summary>
        private const int mineFieldRows = 9;
        /// <summary>
        /// 列の数
        /// </summary>
        const int mineFieldColumns = 9;
        /// <summary>
        /// マインの総数
        /// </summary>
        const int totalMines = 10;
        /// <summary>
        /// セル総数
        /// </summary>
        const int totalCells = 81;

        /// <summary>
        /// 対応するセルにマインが存在することを示します
        /// </summary>
        private bool[,] mines = new bool[mineFieldRows, mineFieldColumns];

        /// <summary>
        /// 対応するセルが訪問されたかどうかを示します
        /// </summary>
        private bool[,] visited = new bool[mineFieldRows, mineFieldColumns];

        /// <summary>
        /// これまでに訪問したセルの数
        /// </summary>
        private int visitedCount = 0;

        /// <summary>
        /// 一番最初のクリックの時だけ本当です
        /// </summary>
        private bool isFirstClick = true;

        /// <summary>
        /// これまでにかかった秒の数
        /// </summary>
        private int _ticks = 0;

        #endregion

        #region public コンストラクタ

        public Form1()
        {
            InitializeComponent();
        }

        #endregion

        #region コントロール　イベント

        private void Form1_Load(object sender, EventArgs e)
        {
            // どのボタンをクリックされても同じメソッドに扱います
            setAllbuttonClicktoButton1Click();

            //グリードをリセットの処理 
            resetGrid();
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            //グリードをリセットの処理 
            resetGrid();
        }

        /// <summary>
        /// ボタンがどれをクリックしてもこのメソッドで扱う
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            //クリックされたボタンを取得
            Button clickedButton = (Button)sender;

            //ボタンの位置を取得
            TableLayoutPanelCellPosition pos = this.tableLayoutPanel1.GetPositionFromControl(clickedButton);
            int posX = pos.Row;
            int posY = pos.Column;
            if (mines[posX, posY] == true) //この位置にマインがある場合
            {
                if (isFirstClick)
                {//一番最初のクリックがマインの所だとマインを取り替える
                    mines[posX, posY] = false;
                    // マインを取り替える
                    putMineOnNextEmptyCell(posX, posY);
                    //今回クリックしたセルを空白として扱う
                    processEmptyField(posX, posY);
                }
                else
                {
                    // ゲーム負けたことの処理
                    gameOver();
                }
            }
            else
            {
                //今回クリックした空白セルの処理
                processEmptyField(posX, posY);
            }

            if (isFirstClick == true)
            {
                //今回のクリックは一番最初のクリックだったら
                isFirstClick = false;
                //タイマーを開始する
                timer1.Start();
            }

        }


        /// <summary>
        /// タイマーのtickイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            //秒数を1つ増やします
            _ticks++;

            //対応する時間を計算して表示する
            int hh = _ticks / 60;
            int mm = _ticks % 60;
            this.labelTimer.Text = string.Format("{0:00}:{1:00}", hh, mm);
        }

        #endregion

        #region ヘルパーメソッド

        /// <summary>
        /// どのボタンをクリックされても同じメソッドに扱います
        /// </summary>
        private void setAllbuttonClicktoButton1Click()
        {
            foreach (Control item in this.tableLayoutPanel1.Controls)
            {
                if (item.Name.Substring(0, 6).Equals("button")) //ボタン１から８１までの全部の名前"button"で戦略
                {
                    item.Click += new System.EventHandler(this.button1_Click);
                }
            }

        }

        /// <summary>
        /// グリードをリセットの処理 
        /// </summary>
        private void resetGrid()
        {
            //マインフィルドを元の状態に戻す
            clearMineField();

            // マインをランダムに配置する
            placeMines();

            //秒の数を0にする
            _ticks = 0;

            //タイマーのテキストのリセット
            labelTimer.Text = "00:00";

            //また一番最初からクリック
            isFirstClick = true;
        }

        /// <summary>
        /// マインフィルドを元の状態に戻す
        /// </summary>
        private void clearMineField()
        {
            //フォーカスをリセットボタンにする
            btnReset.Select();
            for (int i = 0; i < mineFieldRows; i++) // this.tableLayoutPanel1.RowCountと同じ
            {
                for (int j = 0; j < mineFieldColumns; j++) // this.tableLayoutPanel1.ColumnCountと同じ
                {
                    //セルを空白と色がグレーにする
                    Button currentButton = (Button)this.tableLayoutPanel1.GetControlFromPosition(j, i);
                    if (currentButton != null)
                    {
                        currentButton.Text = string.Empty;
                        currentButton.BackColor = Color.LightGray;
                    }
                    mines[i, j] = false;　//セルを空白にする
                    visited[i, j] = false; //訪れていない状態にする
                }
            }
            //これまでに訪問したセルの数をクリアする
            visitedCount = 0;
        }


        /// <summary>
        /// マインをランダムに配置する 
        /// </summary>
        private void placeMines()
        {
            int mineCount = 0;
            Random r = new Random();

            while (mineCount < totalMines)
            {
                int xPos = r.Next(0, mineFieldRows);
                int yPos = r.Next(0, mineFieldColumns);
                if (mines[xPos, yPos] == false) //このセルでマインが前から入ってないと入れること
                {
                    mines[xPos, yPos] = true;
                    mineCount++;
                }
            }

        }
        /// <summary>
        /// マインのセル全部見せる
        /// </summary>
        private void showMines()
        {
            for (int i = 0; i < mineFieldRows; i++) // this.tableLayoutPanel1.RowCount
            {
                for (int j = 0; j < mineFieldColumns; j++) // this.tableLayoutPanel1.ColumnCount
                {
                    if (mines[i, j] == true)
                    {
                        Button curButton = (Button)this.tableLayoutPanel1.GetControlFromPosition(j, i);
                        if (curButton != null)
                        {
                            curButton.Text = "*";
                            curButton.BackColor = Color.Red;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ゲーム負けたことの処理
        /// </summary>
        private void gameOver()
        {
            //マインのセル全部見せる
            showMines();
            //タイマーを止める
            timer1.Stop();
            //負けたメッセージの表示
            MessageBox.Show("      Game Over!");
        }

        /// <summary>
        /// マインを取り替える
        /// 左上から最初の空のセルを見つける
        /// </summary>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        private void putMineOnNextEmptyCell(int posX, int posY)
        {
            for (int i = 0; i < mineFieldRows; i++)
            {
                for (int j = 0; j <= mineFieldColumns; j++)
                {
                    if (i == posX && j == posY) continue;

                    if (mines[posX, posY] == false)
                    {
                        mines[posX, posY] = true;
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 空白セルの処理
        /// </summary>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        private void processEmptyField(int posX, int posY)
        {
            //再帰呼び出しの基本ケース
            if (visited[posX, posY] == true)
            {
                //前に訪問されている場合処理しない
                return;
            }

            //前に訪問されていない場合にのみ処理する
            visited[posX, posY] = true;
            visitedCount++;

            //マインを含む隣接セルの数
            int adjacentMines = 0;
            for (int i = posX - 1; i <= posX + 1; i++)
            {
                for (int j = posY - 1; j <= posY + 1; j++)
                {
                    if (fieldValid(i, j)) // セルがグリッド内にあるかどうかを確認
                    {
                        if (mines[i, j] == true) //セル[i,j]にマインを入ってる
                        {
                            adjacentMines++;
                        }
                    }
                }
            }


            if (adjacentMines == 0)
            {
                //空白のセルを緑色のbackcolorで表示する
                Button currentButton = (Button)this.tableLayoutPanel1.GetControlFromPosition(posY, posX);
                if (currentButton != null)
                {
                    currentButton.Text = "";
                    currentButton.BackColor = Color.LawnGreen;
                }

                //隣のセルにはmmmがないため、隣のセルを処理します
                for (int i = posX - 1; i <= posX + 1; i++)
                {
                    for (int j = posY - 1; j <= posY + 1; j++)
                    {
                        //また自分を呼ばない
                        if (i == posX && j == posY) continue;
                        //再帰呼び出し
                        if (fieldValid(i, j))
                        {
                            // 空白セルの処理
                            processEmptyField(i, j);
                        }
                    }
                }
            }
            else
            {
                //隣のセルでマインがある場合
                //BackColorを緑色にして、隣のセルのマイン数を表示
                Button currentButton = (Button)this.tableLayoutPanel1.GetControlFromPosition(posY, posX);
                if (currentButton != null)
                {
                    currentButton.Text = adjacentMines.ToString();
                    currentButton.BackColor = Color.LawnGreen;
                }
            }

            if (visitedCount == totalCells - totalMines)
            {
                //マイン無のセルを全部訪問した場合
                showWin();
            }
        }

        /// <summary>
        /// ゲームに勝ったおめでとうメッセージを表示する
        /// </summary>
        private void showWin()
        {
            //マインのセル全部見せる
            showMines();
            //タイマーを止める
            timer1.Stop();
            //ゲームに勝ったおめでとうメッセージと掛かった時間を表示する
            MessageBox.Show("Congratulations! You have Completed the game in " + labelTimer.Text);
        }
        /// <summary>
        /// セルがグリッド内にあるかどうかを確認
        /// </summary>
        /// <param name="i">行番号</param>
        /// <param name="j">列番号</param>
        /// <returns></returns>
        private bool fieldValid(int i, int j)
        {
            if (i < 0 || i >= mineFieldRows) return false;
            if (j < 0 || j >= mineFieldColumns) return false;
            return true;
        }

        #endregion

    }
}