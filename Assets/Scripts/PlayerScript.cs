using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (gameObject.GetComponent<BoxCollider>().enabled == true) {
                gameObject.GetComponent<BoxCollider>().enabled = false;
                print("Sensors Disabled");
            } else {
                gameObject.GetComponent<BoxCollider>().enabled = true;
                print("Sensors Enabled");
            }
        }
		
	} 
}
