using UnityEngine;
using UnityEngine.Animations.Rigging;

public class EmanuelHeadLook : MonoBehaviour
{
    //Check the Head Look Script for comments
    [Header("Settings")]
    [SerializeField] private Rig rig;
    [SerializeField] private Transform emanuelHeadLook;
    [SerializeField] private float lookSpeed = 3f;

    private bool isLookingAtPosition;
    private float currentWeight;
    private Vector3 defaultLookPosition;

    private void Start()
    {
        defaultLookPosition = emanuelHeadLook.position;
    }

    private void Update()
    {
        UpdateLookWeight();
    }

    private void UpdateLookWeight()
    {
        float targetWeight = isLookingAtPosition ? 1f : 0f;
        currentWeight = Mathf.Lerp(currentWeight, targetWeight, lookSpeed * Time.deltaTime);
        rig.weight = currentWeight;
    }

    public void LookAtPosition(Vector3 position)
    {
        isLookingAtPosition = true;
        emanuelHeadLook.position = position;
    }

    public void StopLooking()
    {
        isLookingAtPosition = false;
        emanuelHeadLook.position = defaultLookPosition;
    }
}
