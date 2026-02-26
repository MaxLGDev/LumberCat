using System;
using UnityEngine;

// 各ラウンドの入力ロジックを定義するインターフェース
public interface IRoundMechanic
{
    // 正解／不正解入力時の通知イベント
    event System.Action OnValidInput;
    event System.Action OnInvalidInput;

    // ラウンド開始時に呼ばれる（使用可能なキーを受け取る）
    void StartRound(KeyCode[] allowedKeys);

    // キー入力を受け取り、判定を行う
    void HandleKey(KeyCode key);

    // 現在有効なキーが変更されたときの通知
    event Action<KeyCode[]> OnCurrentKeyChanged;

    // 現在判定対象となっているキー
    KeyCode CurrentKey { get; }
}