using UnityEngine;
using System;

public class DroppedItem : MonoBehaviour
{
    public static event Action<Vector3> OnItemLanded;

    [Header("Item Settings")]
    public bool isDropped = false;
    public bool isInvestigated = false;
    public float detectionRadius = 5f;

    [Header("Collision Settings")]
    [SerializeField] private string groundTag = "Ground";

    private Renderer itemRenderer;
    private Material originalMaterial;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        itemRenderer = GetComponent<Renderer>();

        if (itemRenderer != null)
        {
            originalMaterial = itemRenderer.material;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isDropped) return; // Prevent duplicate landing calls

        if (collision.gameObject.CompareTag(groundTag))
        {
            HandleLanding();
        }
    }

    void HandleLanding()
    {
        isDropped = true;
        OnItemLanded?.Invoke(transform.position);

        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (Collider col in colliders)
        {
           
        }

        GetComponent<Rigidbody>().isKinematic = true;
    }


    public void MarkAsInvestigated()
    {
        isInvestigated = true;

        if (itemRenderer != null)
        {
            itemRenderer.material.color = Color.gray; // Change color as feedback
        }
    }

    public void ResetInvestigation()
    {
        isInvestigated = false;

        if (itemRenderer != null && originalMaterial != null)
        {
            itemRenderer.material = originalMaterial; // Restore original material
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

}