using UnityEngine;
using Mandible.Core;
using Mandible.Systems;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class Inventory
{
    Player owner;
    public Transform inventory;

    [Header("General")]
    public ObservedList<Item> items = new ObservedList<Item>();
    public ObservedList<Item> hotbar = new ObservedList<Item>();
    public Item equippedItem;

    [HideInInspector] public int currentItemIndex;
    Item prevItem;
    PlayerInputActions input;

    public Inventory(Player owner = default)
    {
        this.owner = owner;
    }

    public void Start()
    {
        input = new PlayerInputActions();
        input?.Enable();

        SetInputListeners();

        hotbar.Changed += OnHotbarSlotChanged;
        hotbar.Updated += OnHotbarUpdated;
    }

    public void Update()
    {
        
    }

    public void SetOwner(Player owner)
    {
        this.owner = owner;
    }

    public void OnDestroy()
    {
        if (hotbar != null)
        {
            hotbar.Changed -= OnHotbarSlotChanged;
            hotbar.Updated -= OnHotbarUpdated;
        }

        if (input != null)
        {
            input.Disable();
        }
    }

    // Hotbar / Active Items
    public void SwitchItem(int index)
    {
        if (equippedItem != null) {
            equippedItem.gameObject.SetActive(false);
            equippedItem.transform.SetParent(inventory);
        }
        
        prevItem = equippedItem;
        currentItemIndex = (index % hotbar.Count + hotbar.Count) % hotbar.Count;
        equippedItem = hotbar[currentItemIndex];

        if (equippedItem == null) return;

        equippedItem.transform.SetParent(owner.hand);
        equippedItem.transform.localPosition = new Vector3(0, 0, 0);
        equippedItem.transform.localRotation = Quaternion.Euler(new Vector3(0, 45f, 0));
        equippedItem.gameObject.SetActive(true);
    }

    // Input
    void HandleScroll(float input)
    {
        if (input == 0) return;

        int direction = input > 0 ? 1 : -1;
        SwitchItem(currentItemIndex + direction);
    }

    // Save / Load
    public bool LoadInventory() 
    {
        return true;
    }

    // Events
    void SetInputListeners()
    {
        input.Player.Scroll.performed += ctx => HandleScroll(ctx.ReadValue<float>());
    }

    private void OnHotbarSlotChanged(int index, Item oldValue, Item newValue)
    {
        Debug.Log("On hotbar slot changed");
        SwitchItem(currentItemIndex);
    }

    private void OnHotbarUpdated()
    {
        Debug.Log("On hotbar updated");
        SwitchItem(currentItemIndex);
    }
}
