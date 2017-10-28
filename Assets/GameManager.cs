using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        Play();
    }

    // ゲームを開始します
    private void Play()
    {
        var field = GameObject.Find("Field").GetComponent<Field>();

        // 行数、列数、爆弾の個数を指定してフィールドを初期化します。
        field.Initialize(9, 9, 10);
    }
}
