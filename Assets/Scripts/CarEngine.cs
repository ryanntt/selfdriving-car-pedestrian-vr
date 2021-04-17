using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarEngine : MonoBehaviour {
	public WheelCollider wheelLB;
	public WheelCollider wheelRB;
	public WheelCollider wheelLF;
	public WheelCollider wheelRF;
	public Transform wheelLBTransform;
	public Transform wheelRBTransform;
	public Transform wheelLFTransform;
	public Transform wheelRFTransform;

	public float maxSpeed = 400.0f;
    public float turnSpeed = 5f;
    public float maxMotorTorque = 150f;
    public float maxBrakeTorque = 100f;
    public float currentSpeed;
    public float newSteer = 0;
    public bool isBraking = false;
    public bool hasHuman = false;
    public float distanceToHuman = 0f;

    public Transform path;
    private List<Transform> nodes;

    private int currentNode = 0; // In later part, we will update currentNode as the car moves

    public float maxSteerAngle = 50f;

    [Header("Sensors")] //Create a header for the variables in the UI Inspector
    public float sensorLength = 12f;
    public float frontSensorStartPos = 1.1f;
    public float sideSensorStartPos = 0.375f;
    public float frontSensorAngle = 20;
    public float timeStart = 0;

    public string currentCarStatus = "moving"; // 5 statuses: moving, slowing, stopped, donotcross, accelerating
    public string prevCarStatus = "stopped";

    private float targetSteerAngle = 0;
    private readonly float waitingTime = 8;

    [Header("Test Control")]
    public bool paused;

    void Start() {
        paused = false;
        Transform[] pathTransforms = path.GetComponentsInChildren<Transform>();
        nodes = new List<Transform>();

        for (int i = 0; i < pathTransforms.Length; i++)
        {
            if (pathTransforms[i] != path.transform) //check if the new path is similar to our path - if the current is going in the same/or new direction
            {
                nodes.Add(pathTransforms[i]);
            }
        }
    }

    private void FixedUpdate() // This function is called every fixed framerate frame
    {
        TestControl();
        Sensors();
        ApplySteer();
        Drive();
        CheckWaypointDistance();
        Braking();
        LerpToSteerAngle();
        UpdateCarStatus();
    }

    private void Update()
    {
        TestControl();
    }

    private void Sensors() {
        RaycastHit hit;
        Vector3 sensorStartPos = transform.position;
        sensorStartPos.x += frontSensorStartPos;
        sensorStartPos.y += 0.15f;

        // Front centre sensor
        if (Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLength)) {
            if (hit.collider.CompareTag("Human")) {
                //Debug.DrawLine(sensorStartPos, hit.point, Color.red); // Turn on when debug the sensors
                if (hasHuman == false) {
                    hasHuman = true;
                    timeStart = Time.time;
                }
                distanceToHuman = Vector3.Distance(sensorStartPos, hit.point);
                return; // If any of the sensor detects human, it changes variable hasHuman and stop the function.
            } 
        }

        sensorStartPos.z += sideSensorStartPos;

        // Front left sensor
        if (Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLength))
        {
            if (hit.collider.CompareTag("Human"))
            {
                Debug.DrawLine(sensorStartPos, hit.point, Color.red); //Draw line from starting point to the hit point, only when raycast hit something
                if (hasHuman == false)
                {
                    hasHuman = true;
                }
                distanceToHuman = Vector3.Distance(sensorStartPos, hit.point);
                return;
            }
        } 

        // Front left angle sensor
        if (Physics.Raycast(sensorStartPos, Quaternion.AngleAxis(-frontSensorAngle, transform.up) * transform.forward, out hit, sensorLength))
        {
            if (hit.collider.CompareTag("Human")) {
                Debug.DrawLine(sensorStartPos, hit.point, Color.red); //Draw line from starting point to the hit point, only when raycast hit something
                if (hasHuman == false)
                {
                    hasHuman = true;
                }
                distanceToHuman = Vector3.Distance(sensorStartPos, hit.point);
                return;
            }
        }

        sensorStartPos.z -= 2 * sideSensorStartPos;

        // Front right sensor
        if (Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLength))
        {
            if (hit.collider.CompareTag("Human"))
            {
                Debug.DrawLine(sensorStartPos, hit.point, Color.red); //Draw line from starting point to the hit point, only when raycast hit something
                if (hasHuman == false)
                {
                    hasHuman = true;
                }
                distanceToHuman = Vector3.Distance(sensorStartPos, hit.point);
                return;
            }
        } 

        // Front right angle sensor
        if (Physics.Raycast(sensorStartPos, Quaternion.AngleAxis(frontSensorAngle, transform.up) * transform.forward, out hit, sensorLength))
        {
            if (hit.collider.CompareTag("Human")) {
                Debug.DrawLine(sensorStartPos, hit.point, Color.red); //Draw line from starting point to the hit point, only when raycast hit something
                if (hasHuman == false)
                {
                    hasHuman = true;
                }
                distanceToHuman = Vector3.Distance(sensorStartPos, hit.point);
                return;
            }
        }

        if (hasHuman == true) {
            timeStart = Time.time;
            hasHuman = false;
        }
    }

    private void ApplySteer() {
        Vector3 relativeVector = transform.InverseTransformPoint(nodes[currentNode].position);
        newSteer =  (relativeVector.x / relativeVector.magnitude) * maxSteerAngle;

        targetSteerAngle = newSteer;
    }

    private void Drive() {

        currentSpeed = 2 * Mathf.PI * wheelLF.radius * wheelLF.rpm;

        if (!paused) {

            if (!hasHuman && ((Time.time - timeStart) > waitingTime || Time.time <= waitingTime)) { // Wait for 5 seconds of seeing no one around
                if (currentSpeed < maxSpeed) // When the speed is less than max speed 
                {
                    float distanceToNextSteer = Vector3.Distance(transform.position, nodes[currentNode].position);

                    if (distanceToNextSteer < 12.0f)
                    { // Slow down when the car is close to the next steering and the angle is large
                        if ( currentSpeed > 250f) {
                            wheelLB.motorTorque = 0;
                            wheelRB.motorTorque = 0;
                            wheelLB.brakeTorque = 0.1f*maxBrakeTorque;
                            wheelRB.brakeTorque = 0.1f*maxBrakeTorque;
                            if (distanceToNextSteer < 6.0f)
                            {
                                isBraking = true;
                            }
                        } else {
                            isBraking = false;
                            wheelLB.motorTorque = maxMotorTorque;
                            wheelRB.motorTorque = maxMotorTorque;
                        }
                    }
                    else
                    {
                        isBraking = false;
                        wheelLB.motorTorque = maxMotorTorque;
                        wheelRB.motorTorque = maxMotorTorque;
                    }

                }
                else
                {
                    isBraking = false;
                    wheelLB.motorTorque = 0;
                    wheelRB.motorTorque = 0;
                }
            } else {
                if (!hasHuman && ((Time.time - timeStart) < waitingTime) && (currentCarStatus == "moving" || currentCarStatus == "accelerating" || currentCarStatus =="slowing")) { // If car detect human but when it hasn't stopped and saw no one, it will continue its journey.
                    isBraking = false;
                    wheelLB.motorTorque = maxMotorTorque;
                    wheelRB.motorTorque = maxMotorTorque;
                } else if (currentSpeed > 0) {
                    wheelLB.motorTorque = 0;
                    wheelRB.motorTorque = 0;
                    isBraking = distanceToHuman < 3.5f ? true : false;
                }
            }
        } else {
            wheelLB.motorTorque = 0;
            wheelRB.motorTorque = 0;
            isBraking = true;
        }

    }

    private void CheckWaypointDistance() {
        if (Vector3.Distance(transform.position, nodes[currentNode].position) < 1.7f) { // Start steer when the distance is less than x metre
            if (currentNode == nodes.Count -1) {
                currentNode = 0;
            } else {
                currentNode++;
            }
        }
    }

    private void Braking() {
        if (isBraking) {
            wheelLB.brakeTorque = maxBrakeTorque;
            wheelRB.brakeTorque = maxBrakeTorque;
        } else {
            wheelLB.brakeTorque = 0;
            wheelRB.brakeTorque = 0;
        }
    }

    private void LerpToSteerAngle() {
        wheelLF.steerAngle = Mathf.Lerp(wheelLF.steerAngle, targetSteerAngle, Time.deltaTime * turnSpeed);
        wheelRF.steerAngle = Mathf.Lerp(wheelRF.steerAngle, targetSteerAngle, Time.deltaTime * turnSpeed);
    }

    private void UpdateCarStatus() {
        //Update car status according to its physical status
        var temp = currentCarStatus;

        if (Mathf.Abs(currentSpeed) > 0.5f && wheelLB.motorTorque < 0.1f)
        {
            currentCarStatus = "slowing";

            prevCarStatus = temp;
            if (currentCarStatus != prevCarStatus)
            {
                print(prevCarStatus + "->" + currentCarStatus);
            }
            return;
        }


        if (Mathf.Abs(currentSpeed) < 0.5f && wheelLB.motorTorque < 0.1f && wheelLB.brakeTorque > 0.1f) {
            if (hasHuman == true) {
                currentCarStatus = "stopped";
            } else if (hasHuman == false && (Time.time - timeStart) < waitingTime && Time.time >= waitingTime) {
                currentCarStatus = "donotcross";
            }

            prevCarStatus = temp;
            if (currentCarStatus != prevCarStatus)
            {
                print(prevCarStatus + "->" + currentCarStatus);
            }
            return;
        }


        if ( wheelLB.motorTorque > 0.1f)
        {
            currentCarStatus = "accelerating";

            prevCarStatus = temp;
            if (currentCarStatus != prevCarStatus)
            {
                print(prevCarStatus + "->" + currentCarStatus);
            }
            return;
        }

        currentCarStatus = "moving";
        prevCarStatus = temp;
        if (currentCarStatus != prevCarStatus)
        {
            print(prevCarStatus + "->" + currentCarStatus);
        }
    }

    private void TestControl() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (paused == false)
            {
                paused = true;
                print("Game Unpaused");
            }
            else
            {
                print("Please press Arrow keys to reload the scene");
            }
        }
    }
}
