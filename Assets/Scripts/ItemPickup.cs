using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public string ItemId;
    // Start is called before the first frame update
    void Start()
    {
        ItemId = System.Guid.NewGuid().ToString();
        this.PostNotification("SpawnedItem", gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other) {
        Debug.Log("Item entered");
        if(other.GetComponent<Vehicle>() != null) {
            this.PostNotification("DespawnedItem", gameObject);
            Destroy(gameObject);
        }
    }
}
