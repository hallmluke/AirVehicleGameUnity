using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Minimap : MonoBehaviour
{
    // TODO: Dynamically find world size and map size? Look into bounding boxes, https://forum.unity.com/threads/getting-the-bounds-of-the-group-of-objects.70979/
    public Vector3 WorldSize;
    public Vector3 MapSize;

    public Dictionary<string, Image> Markers = new Dictionary<string, Image>();
    public Transform vehicle; // Testing
    public Image vehicleIcon; // Testing

    // Start is called before the first frame update
    void Start()
    {
        Markers.Add("Player", vehicleIcon);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMarkerPosition("Player", vehicle.position);
    }

    void UpdateMarkerPosition(string MarkerId, Vector3 WorldPosition) {
        if(!Markers.ContainsKey(MarkerId)) {
            Debug.LogError("Attempted to update marker position in minimap that is not registered.");
            return;
        }

        Image Marker = Markers[MarkerId];
        Marker.rectTransform.anchoredPosition = WorldPositionToMapPosition(WorldPosition);
    } 

    Vector3 WorldPositionToMapPosition(Vector3 WorldPosition) {

        Vector3 Result = new Vector3();
        Result.x = (-WorldPosition.x / WorldSize.x) * MapSize.x;
        Result.y = (-WorldPosition.z / WorldSize.z) * MapSize.y;

        return Result;
    }
}
