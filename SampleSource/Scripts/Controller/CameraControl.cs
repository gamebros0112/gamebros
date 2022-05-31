using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public Camera mainCamera;
    [Header("Camera Movement Speed by Key Control")]
    public float movementSpeed;
    [Header("Camera Rotation Speed by Mouse Control")]
    public float rotationSpeed;
    
    private float horizontal;
    private float vertical;
    
    public void OnMoveInput(float x, float y)
    {
        horizontal = x;
        vertical = y;
    }
    private void Update()
    {
        Vector3 moveDirection = Vector3.forward * vertical + Vector3.right * horizontal;
        Vector3 projCamForward = Vector3.ProjectOnPlane(mainCamera.transform.forward, Vector3.up);
        Quaternion rotationToCam = Quaternion.LookRotation(projCamForward, Vector3.up);
        transform.rotation =
            Quaternion.RotateTowards(transform.rotation, rotationToCam, rotationSpeed * Time.deltaTime);
        //moveDirection = moveDirection * rotationToCam;
        transform.position += moveDirection * movementSpeed * Time.deltaTime;
    }
}
