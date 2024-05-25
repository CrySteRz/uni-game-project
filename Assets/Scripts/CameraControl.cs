using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    public CinemachineFreeLook playerFreeLookCamera;
    public CinemachineFreeLook sweepoFreeLookCamera;

    public PlayerMovement playerMovement; // Reference to the PlayerMovement script

    void Update()
    {
        if (playerMovement.IsDriving())
        {
            ActivateSweepoCamera();
        }
        else
        {
            ActivatePlayerCamera();
        }
    }

    void ActivatePlayerCamera()
    {
        playerFreeLookCamera.Priority = 10;
        sweepoFreeLookCamera.Priority = 0;
    }

    void ActivateSweepoCamera()
    {
        playerFreeLookCamera.Priority = 0;
        sweepoFreeLookCamera.Priority = 10;
    }
}
