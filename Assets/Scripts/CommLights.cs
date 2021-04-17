using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommLights : MonoBehaviour {
	public Material CommLight_Moving;
	public Material CommLight_Slowing;
	public Material CommLight_Stopped;
	public Material CommLight_DoNotCross;
	CarEngine carEngineScript;
	// Use this for initialization
	void Start () {
		GameObject tempObj = GameObject.Find("CarTorus");
        carEngineScript = tempObj.GetComponent<CarEngine>();
	}
	
	// Update is called once per frame
	void Update () {
		var currentCarStatus = carEngineScript.currentCarStatus;
		var hasHuman = carEngineScript.hasHuman;


		if (currentCarStatus == "slowing" && hasHuman)
        {
			gameObject.GetComponent<Renderer>().material = CommLight_Slowing;
        }

        if (currentCarStatus == "stopped" && hasHuman)
        {
           gameObject.GetComponent<Renderer>().material = CommLight_Stopped;
        }

        if (currentCarStatus == "donotcross")
        {
			gameObject.GetComponent<Renderer>().material = CommLight_DoNotCross;
        }

        if (currentCarStatus == "moving" || currentCarStatus == "accelerating" )
        {
			gameObject.GetComponent<Renderer>().material = CommLight_Moving;
        }
		
	}
}
