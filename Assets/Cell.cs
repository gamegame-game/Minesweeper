using System;
using UnityEngine;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    // セル1辺の大きさ
    public static int Size = 30;

    // セルの状態
    public CellState State { get; set; }

    // このセルに爆弾があるかどうか
    public bool IsBomb { get; set; }

    // 隣接する爆弾の数です
    public int NeighbourBombCount { get; set; }

    // 隣接するセルを展開するメソッドです
    public Action OpenNeighbourCell { get; private set; }

    // セルを初期化します
    public void Initialize(bool isBomb, Action openNeighbourCell)
    {
        this.State = CellState.Closing;
        this.IsBomb = isBomb;
        this.OpenNeighbourCell = openNeighbourCell;
    }

    // セルを展開します
    public bool Open()
    {
        // すでに展開済の場合は何もしません
        // フラグが立ててあるセルも展開しないようにします
        if (this.State == CellState.Opened || this.State == CellState.Flag)
            return true;

        // Textコンポーネントを取得しておく
        var text = this.GetComponentInChildren<Text>();
        if (this.IsBomb)
        {
            // 爆弾セルを開いたのでゲームオーバー
            text.text = "Ｘ";
            return false;
        }
        else
        {
            // セルを展開済の状態に更新し、白色にします
            this.State = CellState.Opened;
            this.GetComponent<Image>().color = Color.white;
            if (this.NeighbourBombCount == 0)
            {
                // 爆弾０なので空文字表示にします
                text.text = string.Empty;
                // 隣接する爆弾０のセルなので周りのセルを展開します
                this.OpenNeighbourCell();
            }
            else
            {
                // 隣接する爆弾の数を表示します
                text.text = this.NeighbourBombCount.ToString();
            }
            return true;
        }
    }

    // セルにマークを付けます
    public void ChangeMark()
    {
        var text = this.GetComponentInChildren<Text>();
        switch (this.State)
        {
            case CellState.Closing:
                this.State = CellState.Flag;
                // フラグは適当な文字で代替してます
                text.text = "●"; 
                break;
            case CellState.Flag:
                this.State = CellState.Question;
                text.text = "？";
                break;
            case CellState.Question:
                this.State = CellState.Closing;
                text.text = string.Empty;
                break;
            default:
                break;
        }
    }
}

// フィールド上のセルがどのような状態にあるかを表します
public enum CellState
{
    Closing = 0,    // セルが閉じられた状態
    Opened = 1,     // セルが展開された状態
    Flag = 2,       // セルにフラグが立てられた状態
    Question = 3    // セルに？が付けられた状態
}
