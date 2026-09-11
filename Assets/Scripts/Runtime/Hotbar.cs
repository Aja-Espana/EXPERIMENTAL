using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[ExecuteAlways]
public class Hotbar : MonoBehaviour
{
    public Player owner;
    public Image hotbarBackground;
    public Image selectionIcon;
    public List<Image> iconImages = new List<Image>();

    [Header("General")]
    public Vector2 iconSize = new Vector2(50, 50);
    public float iconSpacing = 0f;

    private Inventory inventory;

    void Update()
    {
        if (owner != null)
        {
            inventory = owner.inventory;
        }
        else
        {
            inventory = null;
            return;
        }

        if (inventory?.hotbar == null) return;

        GetData();
        UpdateIconImages();
        UpdateSelectionIcon();
    }

    void GetData()
    {
        int count = inventory.hotbar.Count;
        float totalWidth = (iconSize.x * count) + (iconSpacing * Mathf.Max(0, count - 1));

        Vector2 hotbarSize = new Vector2(totalWidth, iconSize.y);

        if (hotbarBackground != null)
        {
            hotbarBackground.rectTransform.sizeDelta = hotbarSize;
        }
    }

    // UI
    void UpdateIconImages()
    {
        // Update Icon Objects
        int targetCount = inventory.hotbar.Count;

        while (iconImages.Count < targetCount)
        {
            GameObject obj = new GameObject($"Slot_{iconImages.Count}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            obj.transform.SetParent(transform, false);

            Image img = obj.GetComponent<Image>();
            iconImages.Add(img);
        }

        while (iconImages.Count > targetCount)
        {
            int lastIndex = iconImages.Count - 1;
            Image icon = iconImages[lastIndex];
            iconImages.RemoveAt(lastIndex);

            if (icon != null)
            {
                if (Application.isPlaying)
                    Destroy(icon.gameObject);
                else
                    DestroyImmediate(icon.gameObject);
            }
        }

        float stepOffset = iconSize.x + iconSpacing;

        float totalWidth = (iconSize.x * targetCount) + (iconSpacing * Mathf.Max(0, targetCount - 1));
        float startX = -totalWidth / 2f + iconSize.x / 2f;

        // Update Data
        for (int i = 0; i < targetCount; i++)
        {
            RectTransform rect = iconImages[i].rectTransform;
            
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = iconSize;

            float currentX = startX + (i * stepOffset);
            rect.anchoredPosition = new Vector2(currentX, 0f);

            if (inventory.hotbar[i] != null && inventory.hotbar[i].icon != null)
            {
                iconImages[i].sprite = inventory.hotbar[i].icon;
                iconImages[i].enabled = true;
                iconImages[i].preserveAspect = true;
            }
            else
            {
                iconImages[i].sprite = null;
                iconImages[i].enabled = false;
            }
        }
    }

    void UpdateSelectionIcon() 
    {
        if (selectionIcon == null) return;

        if (iconImages == null || iconImages.Count == 0 || inventory.equippedItem == null) 
        {
            selectionIcon.enabled = false;
            return;
        }

        selectionIcon.enabled = true;

        RectTransform rect = selectionIcon.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        RectTransform selection = iconImages[inventory.currentItemIndex].GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(selection.anchoredPosition.x, rect.anchoredPosition.y);
    }
}