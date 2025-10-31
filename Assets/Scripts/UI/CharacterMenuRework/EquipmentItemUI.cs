// Assets/Scripts/UI/CharacterMenuRework/EquipmentItemUI.cs
using UnityEngine;
using UnityEngine.UI;
using Game.Equipment;

namespace Game.UI.Inventory
{
    public class EquipmentItemUI : MonoBehaviour
    {
        [SerializeField] private Image icon;
        public EquipmentInstance Instance { get; private set; }

        void Awake()
        {
            if (icon)
            {
                var rt = icon.rectTransform;
                rt.anchorMin   = Vector2.zero;
                rt.anchorMax   = Vector2.one;
                rt.offsetMin   = Vector2.zero;
                rt.offsetMax   = Vector2.zero;
                rt.pivot       = new Vector2(0.5f, 0.5f);
                rt.localScale  = Vector3.one;     // ensure visible
                rt.localPosition = Vector3.zero;
            }
        }

        public void Bind(EquipmentInstance inst)
        {
            Instance = inst;
            var sprite = (inst != null && inst.def != null) ? inst.def.icon : null;
            if (icon)
            {
                icon.sprite = sprite;
                icon.enabled = sprite != null;   // <- critical so a stale white quad isn’t shown/hidden wrong
                var c = icon.color; c.a = (sprite != null) ? 1f : 0f; icon.color = c;
            }
            gameObject.SetActive(inst != null);
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (icon) icon.rectTransform.localScale = Vector3.one;
        }
#endif
    }
}
