using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 0, -10);
    [Range(0f, 10f)] public float smoothSpeed = 5f;

    [Header("Rango fijo de cámara (Full HD)")]
    public Vector2 minLimit = new Vector2(-8.9f, -5f);
    public Vector2 maxLimit = new Vector2(8.9f, 5f);

    private Camera cam;
    private float camHalfHeight;
    private float camHalfWidth;

    void Start()
    {
        cam = GetComponent<Camera>();
        UpdateCameraBounds();
    }

    void UpdateCameraBounds()
    {
        camHalfHeight = cam.orthographicSize;
        camHalfWidth = camHalfHeight * cam.aspect;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                target = playerObj.transform;
            else
                return;
        }

        UpdateCameraBounds();

        Vector3 desired = target.position + offset;

        // Limita el movimiento dentro del recuadro
        desired.x = Mathf.Clamp(desired.x, minLimit.x + camHalfWidth, maxLimit.x - camHalfWidth);
        desired.y = Mathf.Clamp(desired.y, minLimit.y + camHalfHeight, maxLimit.y - camHalfHeight);

        Vector3 smoothed = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
        transform.position = smoothed;
    }
}