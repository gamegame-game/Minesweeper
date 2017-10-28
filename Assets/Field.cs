using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Field : MonoBehaviour
{
    // 生成するセルのプレファブです。
    [SerializeField]
    private GameObject CellPrefab;

    // フィールドに存在するすべてのセルです。
    public Cell[,] Cells { get; private set; }

    // フィールドの初期化(爆弾位置)を行います。
    public void Initialize(int row, int col, int bombCount)
    {
        this.Cells = new Cell[row, col];

        // ランダム生成された爆弾位置の配列
        var randomBombFlags = Enumerable.Concat(
            Enumerable.Repeat(true, bombCount),
            Enumerable.Repeat(false, row * col - bombCount)
            ).OrderBy(_ => Guid.NewGuid()).ToArray();

        // 生成されたランダム位置に爆弾フラグ設定
        int i = 0;
        for (int r = 0; r < this.Cells.GetLength(0); r++)
        {
            for (int c = 0; c < this.Cells.GetLength(1); c++)
            {
                this.Cells[r, c] = GenerateCell(r, c, randomBombFlags[i++]);
            }
        }

        // 隣接する爆弾を数えておく
        for (int r = 0; r < this.Cells.GetLength(0); r++)
        {
            for (int c = 0; c < this.Cells.GetLength(1); c++)
            {
                this.Cells[r, c].NeighbourBombCount = 
                    GetNeighbourCells(r, c)
                    .Count(cell => cell.IsBomb);
            }
        }

        // フィールドサイズの調整
        this.GetComponent<RectTransform>().sizeDelta = new Vector2(col * Cell.Size, row * Cell.Size);
    }

    // セルを盤面に初期化・生成します。
    private Cell GenerateCell(int r, int c, bool isBomb)
    {
        // ゲームオブジェクトを画面に配置します。
        var go = Instantiate(CellPrefab, this.GetComponent<RectTransform>());
        go.GetComponent<RectTransform>().anchoredPosition = new Vector2(Cell.Size / 2 + Cell.Size * c, Cell.Size / 2 + Cell.Size * r);

        // セルクラスを初期化します。
        var cell = go.GetComponent<Cell>();
        cell.Initialize(isBomb, () => OpenNeighbourCell(r, c));

        return cell;
    }

    // 隣接するセルをすべて取得します。
    private Cell[] GetNeighbourCells(int r, int c)
    {
        var cells = new List<Cell>();

        var isTop = r == 0;
        var isButtom = r == this.Cells.GetLength(0) - 1;
        var isLeft = c == 0;
        var isRight = c == this.Cells.GetLength(1) - 1;

        // 左上
        if (!isTop && !isLeft) cells.Add(this.Cells[r - 1, c - 1]);
        // 上
        if (!isTop) cells.Add(this.Cells[r - 1, c]);
        // 右上
        if (!isTop && !isRight) cells.Add(this.Cells[r - 1, c + 1]);
        // 右
        if (!isRight) cells.Add(this.Cells[r, c + 1]);
        // 右下
        if (!isButtom && !isRight) cells.Add(this.Cells[r + 1, c + 1]);
        // 下
        if (!isButtom) cells.Add(this.Cells[r + 1, c]);
        // 左下
        if (!isButtom && !isLeft) cells.Add(this.Cells[r + 1, c - 1]);
        // 左の判定
        if (!isLeft) cells.Add(this.Cells[r, c - 1]);

        return cells.ToArray();
    }

    // あるセルの隣接するセルすべてを展開します。
    private void OpenNeighbourCell(int r, int c)
    {
        foreach (var cell in GetNeighbourCells(r, c))
        {
            // 展開済のセルには何もしません
            if (cell.State != CellState.Opened) cell.Open();
        }
    }

    // クリックを監視してマークします
    private void Update()
    {
        RightClickCell();
        ClickCell();
    }

    // セルのクリックを判定します。
    private void ClickCell()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var clickPoint = Input.mousePosition;
            var collider = Physics2D.OverlapPoint(clickPoint);
            if (collider)
            {
                var go = collider.transform.gameObject;

                // クリックされたらセルを展開します。
                var result = go.GetComponent<Cell>()?.Open();

                // 爆弾だった場合ゲームオーバー
                if (result == false)
                {
                    GameOver();
                }
                else
                {
                    // 展開に成功した場合、爆弾以外のすべてのセルが展開済の場合クリア
                    if (this.Cells.Cast<Cell>().Where(cell => !cell.IsBomb)
                        .All(cell => cell.State == CellState.Opened))
                    {
                        GameClear();
                    }
                }
            }
        }
    }

    // セルの右クリックを判定します。
    private void RightClickCell()
    {
        if (Input.GetMouseButtonDown(1))
        {
            var clickPoint = Input.mousePosition;
            var collider = Physics2D.OverlapPoint(clickPoint);
            if (collider)
            {
                var go = collider.transform.gameObject;
                go.GetComponent<Cell>()?.ChangeMark();
            }
        }
    }

    // ゲームオーバー時の処理です。
    private void GameOver()
    {
        Debug.Log("ゲームオーバー");
    }

    // ゲームオーバー時の処理です。
    private void GameClear()
    {
        Debug.Log("ゲームクリア");
    }
}
