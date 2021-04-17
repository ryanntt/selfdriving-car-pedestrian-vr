using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour {

    public Transform VRCamera;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            Valve.VR.SteamVR_Fade.View(Color.black, 0);
            var i = SceneManager.GetActiveScene().buildIndex;
            if (i == 0) {
                i++;
            }
            SceneManager.LoadScene(i);
            print("Scene changed");
            Valve.VR.SteamVR_Fade.View(Color.clear, 1);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Valve.VR.SteamVR_Fade.View(Color.black, 0);
            var i = SceneManager.GetActiveScene().buildIndex;
            if (i == 1)
            {
                i--;
            }
            SceneManager.LoadScene(i);
            print("Scene changed");
            Valve.VR.SteamVR_Fade.View(Color.clear, 1);
        }

    }
}
