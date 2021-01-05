using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public string ItemId;
    public HeldItem heldItem;
    // Start is called before the first frame update
    void Start()
    {
        ItemId = System.Guid.NewGuid().ToString();
        this.PostNotification("SpawnedItemPickup", gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other) {
        Debug.Log("Item entered");
        Vehicle collidedVehicle = other.GetComponent<Vehicle>();
        if(collidedVehicle != null) {
            collidedVehicle.Item1 = heldItem;
            heldItem.owner = collidedVehicle;
            this.PostNotification("HeldItemAcquired", heldItem);
            this.PostNotification("DespawnedItemPickup", gameObject);
            Destroy(gameObject);
        }
    }
}
