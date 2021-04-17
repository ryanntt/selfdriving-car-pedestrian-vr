using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarWheel : MonoBehaviour {
    public WheelCollider targetWheel;
    private Vector3 wheelPosition = new Vector3();
    private Quaternion wheelRotation = new Quaternion();
	
	// Update is called once per frame
	void Update () {
        targetWheel.GetWorldPose(out wheelPosition, out wheelRotation); // GetWorldPose get the position and rotation and store those in the variables
        transform.position = wheelPosition;
        transform.rotation = wheelRotation;
	}
}
