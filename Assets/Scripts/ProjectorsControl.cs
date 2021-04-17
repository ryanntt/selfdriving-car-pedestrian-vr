using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectorsControl : MonoBehaviour {

    CarEngine carEngineScript = null;

    // Use this for initialization
    void Start()
    {
        GameObject tempObj = GameObject.Find("CarTorus");
        carEngineScript = tempObj.GetComponent<CarEngine>();
    }

    // Update is called once per frame
    void Update()
    {
        var currentCarStatus = carEngineScript.currentCarStatus;
        var hasHuman = carEngineScript.hasHuman;

        var Stopped_Projector = transform.Find("Stopped_Projector");
        var Moving_Projector = transform.Find("Moving_Projector");
        var Slowing_Projector = transform.Find("Slowing_Projector");
        var DoNotCross_Projector = transform.Find("DoNotCross_Projector");

        if (currentCarStatus == "slowing" && hasHuman)
        {

            Moving_Projector.GetComponent<ProjectorSim>().enabled = false;
            //print(Moving_Projector.GetComponent<ProjectorSim>().enabled);
            Stopped_Projector.GetComponent<ProjectorSim>().enabled = false;
            DoNotCross_Projector.GetComponent<ProjectorSim>().enabled = false;
            Slowing_Projector.GetComponent<ProjectorSim>().enabled = true; // Need to disable all other projector before enabling the chosen one.
        }

        if (currentCarStatus == "stopped" && hasHuman)
        {
           
            Moving_Projector.GetComponent<ProjectorSim>().enabled = false;
            //print(Moving_Projector.GetComponent<ProjectorSim>().enabled);
            Slowing_Projector.GetComponent<ProjectorSim>().enabled = false;
            DoNotCross_Projector.GetComponent<ProjectorSim>().enabled = false;
            Stopped_Projector.GetComponent<ProjectorSim>().enabled = true;
        }

        if (currentCarStatus == "donotcross")
        {

            Moving_Projector.GetComponent<ProjectorSim>().enabled = false;
            //print(Moving_Projector.GetComponent<ProjectorSim>().enabled);
            Stopped_Projector.GetComponent<ProjectorSim>().enabled = false;
            Slowing_Projector.GetComponent<ProjectorSim>().enabled = false;
            DoNotCross_Projector.GetComponent<ProjectorSim>().enabled = true;
        }

        if (currentCarStatus == "moving" || currentCarStatus == "accelerating")
        {
            Stopped_Projector.GetComponent<ProjectorSim>().enabled = false;
            Slowing_Projector.GetComponent<ProjectorSim>().enabled = false;
            DoNotCross_Projector.GetComponent<ProjectorSim>().enabled = false;
            Moving_Projector.GetComponent<ProjectorSim>().enabled = true;
        }
    }
}

