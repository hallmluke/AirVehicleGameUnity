using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Speedometer : MonoBehaviour
{
    public Text Speed;

    void OnEnable() {
        this.AddObserver(SetSpeed, "VehicleSpeed");
    }

    void OnDisable() {
        this.RemoveObserver(SetSpeed, "VehicleSpeed");
    }

    void SetSpeed(object sender, object args) {
        float SpeedVal = (float) args;
        Speed.text = SpeedVal.ToString("F1") + " MPH";
    }
}
