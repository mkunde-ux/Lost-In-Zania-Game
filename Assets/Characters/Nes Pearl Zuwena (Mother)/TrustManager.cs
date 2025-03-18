using UnityEngine;

public class TrustManager : MonoBehaviour
{
    // Trust levels for each NPC
    public float imaniTrust = 0f;
    public float emanuelTrust = 0f;
    public float fahariTrust = 0f;

    // Adjust trust for a specific NPC
    public void AdjustTrust(string npcName, float amount)
    {
        switch (npcName)
        {
            case "Imani":
                imaniTrust = Mathf.Clamp(imaniTrust + amount, 0f, 100f);
                Debug.Log($"Imani's trust is now {imaniTrust}");
                break;
            case "Emanuel":
                emanuelTrust = Mathf.Clamp(emanuelTrust + amount, 0f, 100f);
                Debug.Log($"Emanuel's trust is now {emanuelTrust}");
                break;
            case "Fahari":
                fahariTrust = Mathf.Clamp(fahariTrust + amount, 0f, 100f);
                Debug.Log($"Fahari's trust is now {fahariTrust}");
                break;
            default:
                Debug.LogWarning($"Unknown NPC: {npcName}");
                break;
        }
    }

    // Check if the player can interact with Nes
    public bool CanInteractWithNes()
    {
        int highTrustCount = 0;
        if (imaniTrust >= 85) highTrustCount++;
        if (emanuelTrust >= 85) highTrustCount++;
        if (fahariTrust >= 85) highTrustCount++;

        bool canInteract = highTrustCount >= 2;
        Debug.Log($"Can interact with Nes: {canInteract}");
        return canInteract;
    }
}