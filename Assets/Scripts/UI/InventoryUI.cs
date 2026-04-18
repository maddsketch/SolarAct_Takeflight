using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

// Attach to a Canvas panel in the Overworld scene.
// Toggle with the "Inventory" input action (add to the Player action map).
public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    public bool IsOpen { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private UIFadeController panelFade;
    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private Transform itemListParent;
    [SerializeField] private GameObject inventoryItemPrefab;
    [SerializeField] private Button closeButton;

    private PlayerInput playerInput;
    private InputAction inventoryAction;
    private readonly List<InventoryItemUI> rows = new();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        TryResolveInventoryAction();

        closeButton.onClick.AddListener(Close);

        InventoryManager.Instance.OnInventoryChanged += Refresh;

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
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= Refresh;
    }

    void Update()
    {
        if (playerInput == null || !playerInput.isActiveAndEnabled)
        {
            TryResolveInventoryAction();
            if (playerInput == null || !playerInput.isActiveAndEnabled)
                return;
        }
        if (inventoryAction == null)
            TryResolveInventoryAction();

        if (inventoryAction != null && inventoryAction.WasPerformedThisFrame())
            Toggle();
    }

    private void TryResolveInventoryAction()
    {
        if (playerInput == null || !playerInput)
            playerInput = FindAnyObjectByType<PlayerInput>();
        if (playerInput == null)
        {
            inventoryAction = null;
            return;
        }

        var actions = playerInput.actions;
        inventoryAction = actions != null ? actions["Inventory"] : null;
    }

    // --- Open / Close ---

    public void Toggle() { if (IsOpen) Close(); else Open(); }

    public void Open()
    {
        if (ShopManager.Instance != null && ShopManager.Instance.IsOpen) return;
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen) return;

        IsOpen = true;
        if (panelFade != null)
        {
            panelFade.Show();
        }
        else
        {
            panel.SetActive(true);
        }
        Refresh();
    }

    public void Close()
    {
        IsOpen = false;
        if (panelFade != null)
        {
            panelFade.Hide();
        }
        else
        {
            panel.SetActive(false);
        }
    }

    // --- Build list ---

    private void Refresh()
    {
        if (!IsOpen) return;

        // Update currency display.
        int currency = InventoryManager.Instance?.Currency ?? 0;
        if (currencyText != null)
            currencyText.text = $"Credits: {currency}";

        // Rebuild the item list.
        foreach (Transform child in itemListParent)
            Destroy(child.gameObject);
        rows.Clear();

        var inventory = GameStateManager.Instance.Current.inventory;
        foreach (var entry in inventory)
        {
            var def = InventoryManager.Instance?.GetDefinition(entry.itemID);
            if (def == null) continue;

            var go  = Instantiate(inventoryItemPrefab, itemListParent);
            var row = go.GetComponent<InventoryItemUI>();
            if (row == null) continue;

            row.Populate(def, entry.quantity);
            rows.Add(row);
        }
    }
}
