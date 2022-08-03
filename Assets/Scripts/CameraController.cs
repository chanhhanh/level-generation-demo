using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera playerCamera;
    public BoxCollider2D cameraBounds;
    private Vector3 minBounds;
    private Vector3 maxBounds;
    private float halfHeight;
    private float halfWidth;
    private void Start()
    {
        if (TilesGenerator.instance.area != null)
        {
            cameraBounds.size = new Vector2(TilesGenerator.instance.area.x, TilesGenerator.instance.area.y);
        }
        minBounds = cameraBounds.bounds.min;
        maxBounds = cameraBounds.bounds.max;
        halfHeight = playerCamera.orthographicSize;
        halfWidth = playerCamera.orthographicSize * Screen.width / Screen.height;
    }
    private void Update()
    {
        FollowPlayer();
    }
    void FollowPlayer()
    {
        if (Player.instance)
        {
            Vector3 playerPos = Player.instance.transform.position;
            float x = Mathf.Clamp(playerPos.x, minBounds.x + halfWidth, maxBounds.x - halfWidth);
            float y = Mathf.Clamp(playerPos.y, minBounds.y + halfHeight, maxBounds.y - halfHeight);
            playerCamera.transform.position = new Vector3(x, y) + new Vector3(0, 0, -10);
        }
    }
}
