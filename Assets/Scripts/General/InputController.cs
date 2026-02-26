using UnityEngine;

public class InputController : MonoBehaviour
{
    [SerializeField] private InputReader reader;

    // 入力受付の有効／無効を管理
    private bool inputEnabled;

    // 外部へキー入力を通知するためのコールバック
    private System.Action<KeyCode> inputCallback;

    private void Awake()
    {
        // InputReaderのキー入力イベントを購読
        if (reader != null)
            reader.OnKeyPressed += HandleKey;
    }

    private void OnDestroy()
    {
        // イベント購読解除（参照残り防止）
        if (reader != null)
            reader.OnKeyPressed -= HandleKey;
    }

    // 入力時に呼び出す処理を外部から登録
    public void Bind(System.Action<KeyCode> callback)
    {
        inputCallback = callback;
    }

    // コールバック解除
    public void Unbind()
    {
        inputCallback = null;
    }

    // 入力受付の切り替え
    public void EnableInput(bool value)
    {
        inputEnabled = value;
    }

    // 実際のキー入力受信処理
    private void HandleKey(KeyCode key)
    {
        // 入力無効時は無視
        if (!inputEnabled)
            return;

        // Escapeはここでは処理しない（ポーズ用など別管理）
        if (key == KeyCode.Escape)
            return;

        // 登録されている処理があれば実行
        inputCallback?.Invoke(key);
    }
}