using UnityEngine;
using Mandible.PlayerController;

public class Player : Mandible.PlayerController.Player
{
    [Header("General")]
    [SerializeField] public int currentHealth;
    [SerializeField] public int maxHealth;
    [SerializeField] public int sanity;
    [SerializeField] public Inventory inventory = new Inventory();

    [Header("Body")]
    [SerializeField] public Transform hand;

    void Start()
    {
        inventory.SetOwner(this);
        inventory.Start();
    }

    protected override void Update()
    {
        inventory.Update();
    }

    void OnValidate()
    {
        inventory.SetOwner(this);
    }

    void OnDestroy()
    {
        inventory?.OnDestroy();
    }

    // Save / Load
    public void Save() 
    {

    }

    public void Load() 
    {

    }
}

