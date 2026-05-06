using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float cameraFollowSpeed;
    [SerializeField] private Vector3 cameraOffset;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 targetPosition = PlayerController.Instance.transform.position + cameraOffset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * cameraFollowSpeed);
    }
}
