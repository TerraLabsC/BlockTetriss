using UnityEngine;

public class ShakeCameraSet : MonoBehaviour
{
    void Start()
    {
        Camera camera = Camera.main;

        camera.gameObject.GetComponent<CameraShake>().ShakeCamera();
    }
}
