using UnityEngine;
using System;

public class InputReader : MonoBehaviour
{
    // キーが押された瞬間に通知するイベント
    public Action<KeyCode> OnKeyPressed;

    void Update()
    {
        // すべてのKeyCodeを走査し、押されたキーを検出
        foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
        {
            // そのフレームで押されたキーを検出
            if (Input.GetKeyDown(key))
            {
                // 押されたキーを外部へ通知
                OnKeyPressed?.Invoke(key);

                // 同一フレーム内では最初に検出したキーのみ処理
                break;
            }
        }
    }
}