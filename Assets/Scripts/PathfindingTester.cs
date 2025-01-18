using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using UnityEngine;
using TMPro;

public class PathfindingTester : MonoBehaviour
{
    private AStarManager AStarManager = new AStarManager();
    private List<GameObject> Waypoints = new List<GameObject>();
    private List<Connection> ConnectionArray = new List<Connection>();

    [SerializeField] private GameObject start;
    [SerializeField] private GameObject end;
    [SerializeField] private GameObject itemToDrop;
    [SerializeField] private AudioSource movementSound;
    [SerializeField] private float startingSpeed = 20f;
    [SerializeField] private float slowSpeedFactor = 0.75f;
    [SerializeField] private float collisionSlowSpeed = 10f;
    [SerializeField] private float collisionDetectionRadius = 5f;
    [SerializeField] private float speedIncreasePerStop = 2f;
    [SerializeField] private List<GameObject> stopWaypoints;
    [SerializeField] private int initialItems = 5;

    private float currentSpeed;
    private int currentTarget = 0;
    private Vector3 currentTargetPos;
    private int moveDirection = 1;
    private bool agentMove = true;

    private float taskStartTime;
    private float totalDistance;
    private Vector3 lastPosition;

    private int itemsRemaining;
    private bool isStoppedAtWaypoint = false;
    private HashSet<GameObject> visitedStopWaypoints = new HashSet<GameObject>();

    private TextMeshProUGUI distanceText1, timeText1, speedText1, itemsRemainingText1;
    private TextMeshProUGUI distanceText2, timeText2, speedText2, itemsRemainingText2;
    private TextMeshProUGUI distanceText3, timeText3, speedText3, itemsRemainingText3;
    private TextMeshProUGUI itemDroppedText;


    [SerializeField] private VehicleInfo_UI vehicleInfo;

    private void ValidateInspectorFields()
    {
        if (start == null) Debug.LogError("Start waypoint is not assigned.");
        if (end == null) Debug.LogError("End waypoint is not assigned.");
    }

    void Start()
    {
        ValidateInspectorFields();

        if (start == null || end == null)
        {
            Debug.LogError("No start or end waypoints assigned.");
            return;
        }

        transform.position = start.transform.position;

        GameObject[] GameObjectsWithWaypointTag = GameObject.FindGameObjectsWithTag("Waypoint");
        foreach (GameObject waypoint in GameObjectsWithWaypointTag)
        {
            Waypoints.Add(waypoint);
        }

        foreach (GameObject waypoint in Waypoints)
        {
            VisGraphWaypointManager tmpWaypointMan = waypoint.GetComponent<VisGraphWaypointManager>();
            foreach (VisGraphConnection aVisGraphConnection in tmpWaypointMan.Connections)
            {
                if (aVisGraphConnection.ToNode != null)
                {
                    Connection aConnection = new Connection
                    {
                        FromNode = waypoint,
                        ToNode = aVisGraphConnection.ToNode
                    };
                    AStarManager.AddConnection(aConnection);
                }
                else
                {
                    Debug.Log("Warning, " + waypoint.name + " has a missing to node for a connection!");
                }
            }
        }

        ConnectionArray = AStarManager.PathfindAStar(start, end);
        if (ConnectionArray == null || ConnectionArray.Count == 0)
        {
            Debug.LogError("A* did not return a path between the start and end node.");
            return;
        }

        currentSpeed = startingSpeed;
        itemsRemaining = initialItems;
        taskStartTime = Time.time;
        totalDistance = 0f;
        lastPosition = transform.position;

        if (agentMove && movementSound != null)
        {
            movementSound.Play();
        }
    }

    void Update()
    {
        if (ConnectionArray == null || ConnectionArray.Count == 0) return;

        if (agentMove)
        {
            HandleCollisionAvoidance();

            if (!isStoppedAtWaypoint)
            {
                if (moveDirection > 0)
                {
                    currentTargetPos = ConnectionArray[currentTarget].GetToNode().transform.position;
                    currentSpeed = Mathf.Clamp(currentSpeed, startingSpeed, float.MaxValue);
                }
                else
                {
                    currentTargetPos = ConnectionArray[currentTarget].GetFromNode().transform.position;
                }

                currentTargetPos.y = transform.position.y;
                Vector3 direction = currentTargetPos - transform.position;

                float distance = direction.magnitude;
                direction.y = 0;

                if (direction.magnitude > 0)
                {
                    Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
                    transform.rotation = rotation;
                }

                Vector3 normDirection = direction / distance;
                transform.position += normDirection * currentSpeed * Time.deltaTime;

                float distanceThisFrame = Vector3.Distance(transform.position, lastPosition);
                totalDistance += distanceThisFrame;
                lastPosition = transform.position;

                if (distance < 1)
                {
                    HandleWaypointStopping();

                    //    Debug.Log(stopWaypoints[currentTarget].name);

                    currentTarget += moveDirection;

                    if (currentTarget == ConnectionArray.Count)
                    {
                        StartCoroutine(HandleEndWaypointBehavior());
                        moveDirection = -1;
                        currentTarget += moveDirection;
                    }

                    if (currentTarget < 0)
                    {
                        moveDirection = 1;
                        currentTarget += moveDirection;

                        agentMove = false;
                        Debug.Log("Agent has returned to the start and stopped.");

                        if (movementSound != null)
                        {
                            movementSound.Stop();
                        }

                        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + 180, 0);
                    }
                }
            }
        }

        UpdateGUI();
    }

    private void HandleCollisionAvoidance()
    {
        PathfindingTester[] otherTrucks = FindObjectsOfType<PathfindingTester>();
        string info = "";
        foreach (PathfindingTester truck in otherTrucks)
        {
            if (truck != this)
            {
                float distanceToOtherTruck = Vector3.Distance(transform.position, truck.transform.position);
                if (distanceToOtherTruck <= collisionDetectionRadius)
                {
                    if (Mathf.Abs(transform.position.z - truck.transform.position.z) < 1f)
                    {
                        transform.position += transform.right * -2.5f;
                        info += $"{truck.name} Provided Path \n";
                    }
                    else
                    {
                        currentSpeed = Mathf.Min(currentSpeed, collisionSlowSpeed);
                        info += $"{truck.name} Slowing Down \n";
                    }
                }
            }
        }

        if (!string.IsNullOrEmpty(info) && hideRoutine == null)
        {
            vehicleInfo.UpdateInformation(info);
            hideRoutine = StartCoroutine((HideVericalInfo()));
        }
    }

    private Coroutine hideRoutine;

    IEnumerator HideVericalInfo()
    {
        yield return new WaitForSeconds(1);
        vehicleInfo.HideInformationText();
        hideRoutine = null;
    }

    private void HandleWaypointStopping()
    {
        if (stopWaypoints.Contains(ConnectionArray[currentTarget].ToNode) &&
            !visitedStopWaypoints.Contains(ConnectionArray[currentTarget].ToNode))
        {
            StartCoroutine(StopAtWaypointCoroutine());
            visitedStopWaypoints.Add(ConnectionArray[currentTarget].ToNode);
        }
    }

    private IEnumerator StopAtWaypointCoroutine()
    {
        isStoppedAtWaypoint = true;

        vehicleInfo.ShowDeliveryText();
        yield return new WaitForSeconds(2f);

        currentSpeed += speedIncreasePerStop;
        itemsRemaining = Mathf.Max(itemsRemaining - 1, 0);

        isStoppedAtWaypoint = false;
    }

    private IEnumerator HandleEndWaypointBehavior()
    {
        agentMove = false;

        if (movementSound != null && movementSound.isPlaying)
        {
            movementSound.Stop();
        }

        if (itemToDrop != null && end != null)
        {
            Vector3 forwardOffset = transform.forward * 1f;
            Vector3 rightOffset = transform.right * 2f;
            Vector3 dropPosition = end.transform.position + forwardOffset + rightOffset;

            Instantiate(itemToDrop, dropPosition, Quaternion.identity);
        }

        yield return new WaitForSeconds(3f);

        agentMove = true;

        if (movementSound != null)
        {
            movementSound.Play();
        }
    }

    private void UpdateGUI()
    {
        vehicleInfo.UpdateDistance(totalDistance);
        vehicleInfo.UpdateSpeed(currentSpeed);
        if (agentMove == true)
        {
            vehicleInfo.UpdateTime((Time.time - taskStartTime));
        }

        vehicleInfo.UpdateP_Count(itemsRemaining);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, collisionDetectionRadius);
    }
}