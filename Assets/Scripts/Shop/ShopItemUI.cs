using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.Shop
{
    /// <summary>
    /// UI component for displaying a single shop item.
    /// </summary>
    public class ShopItemUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI typeText;
        [SerializeField] private Button buyButton;
        [SerializeField] private GameObject soldOverlay;

        private ShopItem item;
        private System.Action<ShopItem> onPurchase;

        public void Setup(ShopItem shopItem, System.Action<ShopItem> purchaseCallback)
        {
            item = shopItem;
            onPurchase = purchaseCallback;

            if (nameText) nameText.text = item.DisplayName;
            if (descriptionText) descriptionText.text = item.Description;
            if (priceText) priceText.text = $"{item.Price}g";
            if (typeText) typeText.text = item.Type.ToString().ToUpper();

            // Set icon if available
            if (iconImage)
            {
                if (item.Type == ShopItem.ItemType.Card && item.Card?.icon != null)
                {
                    iconImage.sprite = item.Card.icon;
                    iconImage.enabled = true;
                }
                else if (item.Type == ShopItem.ItemType.Equipment && item.Equipment?.icon != null)
                {
                    iconImage.sprite = item.Equipment.icon;
                    iconImage.enabled = true;
                }
                else
                {
                    iconImage.enabled = false;
                }
            }

            if (buyButton)
            {
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(OnBuyClicked);
            }

            RefreshSoldState();
        }

        private void OnBuyClicked()
        {
            if (item == null || item.IsSold) return;
            onPurchase?.Invoke(item);
            RefreshSoldState();
        }

        public void RefreshSoldState()
        {
            if (item == null) return;

            bool canAfford = MapSession.I != null && MapSession.I.Gold >= item.Price;

            if (soldOverlay)
            {
                soldOverlay.SetActive(item.IsSold);
            }

            if (buyButton)
            {
                buyButton.interactable = !item.IsSold && canAfford;
            }

            if (backgroundImage)
            {
                var c = backgroundImage.color;
                c.a = item.IsSold ? 0.3f : 1f;
                backgroundImage.color = c;
            }

            if (priceText)
            {
                priceText.color = item.IsSold ? Color.gray : (canAfford ? new Color(1f, 0.85f, 0.1f) : Color.red);
            }
        }
    }
}
