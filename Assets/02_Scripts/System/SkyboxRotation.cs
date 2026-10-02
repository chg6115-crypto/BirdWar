using UnityEngine;

public class SkyboxRotator : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 0.5f;

    private float rotation;

    void Update()
    {
        if (RenderSettings.skybox == null)
            return;

        rotation += rotationSpeed * Time.deltaTime;
        rotation %= 360f;

        RenderSettings.skybox.SetFloat("_Rotation", rotation);
    }
}