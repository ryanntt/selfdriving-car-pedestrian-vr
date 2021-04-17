using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectorScript : MonoBehaviour {

    CarEngine carEngineScript = null;

    // Use this for initialization
    void Start () {
        GameObject tempObj = GameObject.Find("CarTorus");
        carEngineScript = tempObj.GetComponent<CarEngine>();
    }
	
	// Update is called once per frame
	void Update () {

        var texture1 = Resources.Load<Texture2D>("Images/slideshow_1"); // Don't use extention *.png as Unity can't detect that
        var texture2 = Resources.Load<Texture2D>("Images/slideshow_2");

        var moving_texture1 = Resources.Load<Texture2D>("Images/pattern-default-01"); // Don't use extention *.png as Unity can't detect that
        var moving_texture2 = Resources.Load<Texture2D>("Images/pattern-default-02");
        var moving_texture3 = Resources.Load<Texture2D>("Images/pattern-default-03");
        var moving_texture4 = Resources.Load<Texture2D>("Images/pattern-default-04");

        var currentCarStatus = carEngineScript.currentCarStatus;
        var prevCarStatus = carEngineScript.prevCarStatus;

        if (currentCarStatus == "stopped") {
            if (prevCarStatus == "moving") {
            //print(gameObject.GetComponent<ProjectorSim>().images.Count);
                gameObject.GetComponent<ProjectorSim>().images.Clear(); // Clear all elements of the images array
                gameObject.GetComponent<ProjectorSim>().images.Add(texture1);
                gameObject.GetComponent<ProjectorSim>().images.Add(texture2);
                gameObject.GetComponent<ProjectorSim>().images.Add(texture1);
                gameObject.GetComponent<ProjectorSim>().images.Add(texture2);
                gameObject.GetComponent<ProjectorSim>().Awake();
                gameObject.GetComponent<ProjectorSim>().OnEnable();

            }
        }

        if (currentCarStatus == "moving") {
            if (prevCarStatus == "stopped") {
                gameObject.GetComponent<ProjectorSim>().images.Clear(); // Clear all elements of the images array
                gameObject.GetComponent<ProjectorSim>().images.Add(moving_texture1);
                gameObject.GetComponent<ProjectorSim>().images.Add(moving_texture2);
                gameObject.GetComponent<ProjectorSim>().images.Add(moving_texture3);
                gameObject.GetComponent<ProjectorSim>().images.Add(moving_texture4);
                gameObject.GetComponent<ProjectorSim>().Awake();
                gameObject.GetComponent<ProjectorSim>().OnEnable();

            }
        }
    }
}
