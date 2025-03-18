using UnityEngine;

public class ChaseEffect : MonoBehaviour
{
    public Color chaseColor = Color.red; // Color overlay when chased
    public float chaseIntensity = 0.5f; // Intensity of the color overlay (0-1)
    public float distortionAmount = 0.05f; // Amount of screen distortion
    public float chaseDuration = 2f; // Duration of the chase effect
    public bool isChased = false; // Flag to indicate if the player is being chased

    private float chaseTimer = 0f;
    private Material material; // Material for the post-processing effect

    void Start()
    {
        // Create a material for the post-processing shader
        material = new Material(Shader.Find("Hidden/ChaseEffectShader"));
    }

    void Update()
    {
        if (isChased)
        {
            chaseTimer += Time.deltaTime;
            if (chaseTimer > chaseDuration)
            {
                isChased = false;
                chaseTimer = 0f;
            }
        }
        else
        {
            chaseTimer = 0f; // Reset timer if not chased
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (material == null) return; // Safeguard if material isn't created

        material.SetColor("_ChaseColor", chaseColor);
        material.SetFloat("_ChaseIntensity", chaseIntensity);
        material.SetFloat("_DistortionAmount", distortionAmount);
        material.SetFloat("_ChaseProgress", Mathf.Clamp01(chaseTimer / chaseDuration)); // For smooth transition

        Graphics.Blit(source, destination, material); // Apply the shader
    }


    // Example of how to trigger the chase effect (you would adapt this)
    public void StartChase()
    {
        isChased = true;
    }

    public void StopChase()
    {
        isChased = false;
    }
}