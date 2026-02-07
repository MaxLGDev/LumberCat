using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChangeIconSpriteHelper : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Image muteIcon;

    [SerializeField] private Sprite muted;
    [SerializeField] private Sprite unmuted;

    private void Start()
    {
        UpdateIcon();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        SoundManager.Instance.ToggleMute();
        UpdateIcon();
    }

    private void UpdateIcon()
    {
        muteIcon.sprite = SoundManager.Instance.IsMuted() ? muted : unmuted;
    }
    
}
