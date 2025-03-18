using UnityEngine;
using UnityEngine.AI;

public abstract class Guard : MonoBehaviour
{
    // Abstract properties to be implemented by derived classes.
    public abstract bool isFollowingPlayer { get; set; }
    public abstract bool isChasingPlayer { get; set; }
    public abstract NavMeshAgent navMeshAgent { get; set; }
    public abstract float runSpeed { get; set; }
    public abstract Transform player { get; set; }

    // Virtual methods that can be overridden by derived classes.
    public virtual void PlayerDetected() { }
    public virtual void PlayerLost() { }

    // Abstract methods to be implemented by derived classes.
    public abstract void FollowPlayer(Transform player);
    public abstract void ResumePatrolling();
    public abstract void StartChasingPlayer(Transform player);
    public abstract void StopChasingPlayer();

    // Shared alert method allows a guard to move toward a designated position.
    public virtual void SharedAlert(Vector3 playerPosition)
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.SetDestination(playerPosition);
        }
    }

    // Notifies the GuardManager that a chase has started.
    public void NotifyChaseStarted()
    {
        GuardManager.StartGlobalChase();
    }

    // Notifies the GuardManager that a chase has stopped.
    public void NotifyChaseStopped()
    {
        GuardManager.StopGlobalChase();
    }
}