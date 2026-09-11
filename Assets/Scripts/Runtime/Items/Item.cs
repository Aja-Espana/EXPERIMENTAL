using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public abstract class Item : MonoBehaviour
{
    [Header("General")]
    public new string name;
    public int id;
    public int quantity;
    public Sprite icon;

    public abstract void Use();
}
