using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// ミュート状態に応じてアイコン画像を切り替えるクラス
public class ChangeIconSpriteHelper : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Image muteIcon; // 表示するアイコン

    [SerializeField] private Sprite muted;    // ミュート時スプライト
    [SerializeField] private Sprite unmuted;  // 通常時スプライト

    private void Start()
    {
        // 初期状態を反映
        UpdateIcon();
    }

    // クリック時処理
    public void OnPointerDown(PointerEventData eventData)
    {
        // ミュート切替
        SoundManager.Instance.ToggleMute();

        // アイコン更新
        UpdateIcon();
    }

    // 現在のミュート状態に応じてスプライト変更
    private void UpdateIcon()
    {
        muteIcon.sprite =
            SoundManager.Instance.IsMuted() ? muted : unmuted;
    }
}