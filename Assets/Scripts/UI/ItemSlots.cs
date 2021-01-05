using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlots : MonoBehaviour
{

    public RectTransform Slot1;
    HeldItem Slot1Item;

    void OnEnable() {
        this.AddObserver(AddHeldItem, "HeldItemAcquired");
        this.AddObserver(RemoveHeldItem, "HeldItemUsed");
    }

    void OnDisable() {
        this.RemoveObserver(AddHeldItem, "HeldItemAcquired");
        this.RemoveObserver(RemoveHeldItem, "HeldItemUsed");
    }

    void AddHeldItem(object sender, object args) {
        HeldItem acquiredItem = args as HeldItem;
        // TODO Maybe eventually implement pooling for icons?
        Slot1Item = Instantiate(acquiredItem);
        Slot1Item.transform.SetParent(Slot1);
        Slot1Item.transform.localPosition = Vector3.zero;
    }

    void RemoveHeldItem(object sender, object args) {
        Destroy(Slot1Item.gameObject);
    }

}
