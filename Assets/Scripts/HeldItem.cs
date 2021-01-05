using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeldItem : MonoBehaviour
{
    public Sprite ItemIcon;
    public SpawnableItem spawnableItem;
    public Vehicle owner;

    public void UseItem() {
        Instantiate(spawnableItem, owner.transform.position + owner.transform.forward * 4, owner.transform.rotation);
        this.PostNotification("HeldItemUsed");
    }

}
