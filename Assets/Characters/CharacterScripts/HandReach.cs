using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class HandReach : MonoBehaviour
{
    // Reference to the TwoBoneIKConstraint component for hand IK.
    [SerializeField] private TwoBoneIKConstraint handIK;
    // Target Transform for the hand IK.
    [SerializeField] private Transform handTarget;
    // Reference to the player's Transform.
    [SerializeField] private Transform player;
    // Distance at which the hand can reach.
    public float reachDistance = 2f;
    // Speed at which the hand reaches.
    [SerializeField] private float reachSpeed = 5f;

    // Property to get the current weight of the hand IK.
    public float reachWeight => handIK.weight;
    // Flag to indicate if the hand is reaching out.
    private bool isReachingOut;
    // Reference to the current reach Coroutine.
    private Coroutine currentRoutine;

    // Called when the script instance is being loaded.
    private void Start()
    {
        // Find the player Transform if it's not already assigned.
        if (!player) player = GameObject.FindGameObjectWithTag("Player").transform;
        // Initialize the hand IK weight to 0.
        handIK.weight = 0f;
    }

    // Starts the hand reaching towards the target.
    public void StartReaching(Transform target)
    {
        // Stop the current reach Coroutine if it's running.
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        // Set the hand target position to the target position.
        handTarget.position = target.position;
        // Set the reaching out flag to true.
        isReachingOut = true;
        // Start the reach Coroutine to reach full weight.
        currentRoutine = StartCoroutine(ReachRoutine(1f));
    }

    // Stops the hand reaching.
    public void StopReaching()
    {
        // Stop the current reach Coroutine if it's running.
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        // Set the reaching out flag to false.
        isReachingOut = false;
        // Start the reach Coroutine to return to zero weight.
        currentRoutine = StartCoroutine(ReachRoutine(0f));
    }

    // Coroutine to animate the hand reaching.
    private IEnumerator ReachRoutine(float targetWeight)
    {
        // Loop until the hand IK weight is approximately equal to the target weight.
        while (!Mathf.Approximately(handIK.weight, targetWeight))
        {
            // Move the hand IK weight towards the target weight.
            handIK.weight = Mathf.MoveTowards(
                handIK.weight,
                targetWeight,
                reachSpeed * Time.deltaTime
            );
            // If the hand is reaching out and the player Transform is assigned, update the hand target position.
            if (isReachingOut && player)
            {
                handTarget.position = player.position;
            }

            yield return null;
        }
    }

    // Draws Gizmos in the editor.
    void OnDrawGizmos()
    {
        // If the player Transform is assigned and the hand is reaching out, draw Gizmos.
        if (player && isReachingOut)
        {
            // Set the Gizmos color to red.
            Gizmos.color = Color.red;
            // Draw a line from the hand target to the player position.
            Gizmos.DrawLine(handTarget.position, player.position);
            // Draw a sphere at the player position.
            Gizmos.DrawSphere(player.position, 0.2f);
        }
    }
}