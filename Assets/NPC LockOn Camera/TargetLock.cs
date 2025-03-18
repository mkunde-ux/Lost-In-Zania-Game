using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;

public class TargetLock : MonoBehaviour
{
    [Header("Objects")]
    [Space]
    // Reference to the main camera in the scene.
    [SerializeField] private Camera mainCamera;
    // Reference to the CinemachineFreeLook camera component.
    [SerializeField] private CinemachineFreeLook cinemachineFreeLook;
    [Space]
    [Header("UI")]
    // UI image for the aim icon (can be null if not used).
    [SerializeField] private Image aimIcon;
    [Space]
    [Header("Settings")]
    [Space]
    // Tag used to identify enemy GameObjects.
    [SerializeField] private string enemyTag;
    // Key to toggle target locking.
    [SerializeField] private KeyCode _Input;
    // Offset for target lock position.
    [SerializeField] private Vector2 targetLockOffset;
    // Minimum distance to stop rotation when close to the target.
    [SerializeField] private float minDistance;
    // Maximum distance to search for targets.
    [SerializeField] private float maxDistance;

    // Flag indicating whether target locking is active.
    public bool isTargeting;

    // Maximum angle to target enemies in front of the camera.
    private float maxAngle;
    // Current target Transform.
    private Transform currentTarget;
    // Mouse X input value.
    private float mouseX;
    // Mouse Y input value.
    private float mouseY;

    void Start()
    {
        // Set the maximum angle to 90 degrees.
        maxAngle = 90f;
        // Disable CinemachineFreeLook input when target locking is off.
        cinemachineFreeLook.m_XAxis.m_InputAxisName = "";
        cinemachineFreeLook.m_YAxis.m_InputAxisName = "";
    }

    void Update()
    {
        // If not targeting, capture mouse input.
        if (!isTargeting)
        {
            mouseX = Input.GetAxis("Mouse X");
            mouseY = Input.GetAxis("Mouse Y");
        }
        // If targeting, update input to focus on the current target.
        else
        {
            NewInputTarget(currentTarget);
        }

        // Activate/deactivate aim icon based on targeting state.
        if (aimIcon)
            aimIcon.gameObject.SetActive(isTargeting);

        // Set CinemachineFreeLook input values.
        cinemachineFreeLook.m_XAxis.m_InputAxisValue = mouseX;
        cinemachineFreeLook.m_YAxis.m_InputAxisValue = mouseY;

        // Toggle target locking on input.
        if (Input.GetKeyDown(_Input))
        {
            AssignTarget();
        }
    }

    // Assigns or clears the target based on the current state.
    private void AssignTarget()
    {
        // If already targeting, clear the target and disable targeting.
        if (isTargeting)
        {
            isTargeting = false;
            currentTarget = null;
            return;
        }

        // If not targeting, find the closest target and enable targeting.
        if (ClosestTarget())
        {
            currentTarget = ClosestTarget().transform;
            isTargeting = true;
        }
    }

    // Sets new input values to focus on the current target.
    private void NewInputTarget(Transform target)
    {
        // Return if there is no current target.
        if (!currentTarget) return;

        // Convert target's world position to viewport position.
        Vector3 viewPos = mainCamera.WorldToViewportPoint(target.position);

        // Update aim icon position.
        if (aimIcon)
            aimIcon.transform.position = mainCamera.WorldToScreenPoint(target.position);

        // Stop rotation if the target is too close.
        if ((target.position - transform.position).magnitude < minDistance) return;

        // Calculate mouse input values based on the target's viewport position.
        mouseX = (viewPos.x - 0.5f + targetLockOffset.x) * 3f;
        mouseY = (viewPos.y - 0.5f + targetLockOffset.y) * 3f;
    }

    // Finds and returns the closest GameObject with the enemy tag.
    private GameObject ClosestTarget()
    {
        // Find all GameObjects with the enemy tag.
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag(enemyTag);
        GameObject closest = null;
        float distance = maxDistance;
        float currAngle = maxAngle;
        Vector3 position = transform.position;
        // Iterate through all found GameObjects.
        foreach (GameObject go in gos)
        {
            // Calculate distance to the current GameObject.
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.magnitude;
            // Check if the current GameObject is closer than the current closest.
            if (curDistance < distance)
            {
                // Convert GameObject's world position to viewport position.
                Vector3 viewPos = mainCamera.WorldToViewportPoint(go.transform.position);
                Vector2 newPos = new Vector3(viewPos.x - 0.5f, viewPos.y - 0.5f);
                // Check if the GameObject is within the maximum angle.
                if (Vector3.Angle(diff.normalized, mainCamera.transform.forward) < maxAngle)
                {
                    // Update closest GameObject and distance.
                    closest = go;
                    currAngle = Vector3.Angle(diff.normalized, mainCamera.transform.forward.normalized);
                    distance = curDistance;
                }
            }
        }
        // Return the closest GameObject.
        return closest;
    }

    // Draws a yellow wire sphere in the editor to visualize the maximum distance.
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxDistance);
    }
}