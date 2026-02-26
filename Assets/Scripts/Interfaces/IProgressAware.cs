using UnityEngine;

// 進行状況（例: 入力回数や達成度）の変化を受け取るためのインターフェース
public interface IProgressAware
{
    // current: 現在の進行値
    // required: 達成に必要な値
    void OnProgressChanged(int current, int required);
}