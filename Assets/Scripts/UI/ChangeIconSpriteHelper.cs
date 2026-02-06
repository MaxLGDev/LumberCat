//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;

//public class ChangeIconSpriteHelper : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
//{
//    private Color normalColor;
//    [SerializeField] private Color hoverColor;

//    private Image iconImage;

//    private void Awake()
//    {
//        iconImage = GetComponent<Image>();
//        normalColor = iconImage.color;
//    }

//    public void OnPointerEnter(PointerEventData eventData)
//    {
//        iconImage.color = hoverColor;
//    }

//    public void OnPointerExit(PointerEventData eventData)
//    {
//        iconImage.color = normalColor;
//    }
//}
