using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Attach to a Canvas panel in the Overworld scene.
// Subscribes to ShopManager events and builds the item list dynamically.
public class ShopUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private UIFadeController panelFade;
    [SerializeField] private TextMeshProUGUI shopNameText;
    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private Transform itemListParent;
    [SerializeField] private GameObject shopItemPrefab;
    [SerializeField] private Button closeButton;

    private ShopItemUI[] itemRows = System.Array.Empty<ShopItemUI>();

    void Start()
    {
        ShopManager.Instance.onShopOpen  += OnShopOpen;
        ShopManager.Instance.onShopClose += OnShopClose;

        closeButton.onClick.AddListener(() => ShopManager.Instance.CloseShop());

        if (panelFade != null)
        {
            panelFade.SetVisibleImmediate(false);
        }
        else
        {
            panel.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (ShopManager.Instance == null) return;
        ShopManager.Instance.onShopOpen  -= OnShopOpen;
        ShopManager.Instance.onShopClose -= OnShopClose;
    }

    void Update()
    {
        if (!ShopManager.Instance.IsOpen) return;

        // Refresh affordability every frame so buttons stay accurate.
        int currency = InventoryManager.Instance?.Currency ?? 0;
        currencyText.text = $"Credits: {currency}";

        foreach (var row in itemRows)
            row.RefreshAffordability(currency, maxStack: 99);
    }

    private void OnShopOpen(ShopDefinition shop)
    {
        if (panelFade != null)
        {
            panelFade.Show();
        }
        else
        {
            panel.SetActive(true);
        }
        shopNameText.text = shop.shopName;

        BuildItemList(shop);
    }

    private void OnShopClose()
    {
        if (panelFade != null)
        {
            panelFade.Hide();
        }
        else
        {
            panel.SetActive(false);
        }
        ClearItemList();
    }

    private void BuildItemList(ShopDefinition shop)
    {
        ClearItemList();

        var rows = new System.Collections.Generic.List<ShopItemUI>();

        foreach (string id in shop.itemIDs)
        {
            var def = InventoryManager.Instance?.GetDefinition(id);
            if (def == null)
            {
                Debug.LogWarning($"[ShopUI] Unknown itemID '{id}' in shop '{shop.shopName}'.");
                continue;
            }

            var go  = Instantiate(shopItemPrefab, itemListParent);
            var row = go.GetComponent<ShopItemUI>();
            if (row != null)
            {
                row.Populate(def);
                rows.Add(row);
            }
        }

        itemRows = rows.ToArray();
    }

    private void ClearItemList()
    {
        foreach (Transform child in itemListParent)
            Destroy(child.gameObject);

        itemRows = System.Array.Empty<ShopItemUI>();
    }
}
