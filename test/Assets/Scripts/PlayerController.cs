using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public List<Vector3> WayPoints; // Danh sách các waypoint
    public float moveSpeed = 5f;
    [SerializeField] private int currentWaypointIndex = 1;
    Rigidbody rb;
    private bool isAutoMoving = false;

    void Start()
    {
        currentWaypointIndex = 1;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        Move();
        MoveByPath();
    }

    public void SetData(List<Vector3> wayPoints,  bool autoMove = false)
    {
        WayPoints = wayPoints;
        isAutoMoving = autoMove;
        currentWaypointIndex = 1;

        // Nếu không tự động di chuyển, reset vị trí về waypoint đầu tiên
        if (!isAutoMoving && WayPoints.Count > 0)
        {
            transform.position = WayPoints[0];
            rb.velocity = Vector3.zero; // Reset velocity
        }
    }

    private void Move()
    {
        if(isAutoMoving)return;
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 dir = new Vector3(h, 0, v).normalized;
        rb.velocity = dir * moveSpeed + new Vector3(0, rb.velocity.y, 0);
    }

    private void MoveByPath()
    {
        if (!isAutoMoving) return;

        Vector3 targetPos = WayPoints[currentWaypointIndex];

        // 3. Di chuyển về phía waypoint
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            moveSpeed * Time.deltaTime
        );

        // 5. Kiểm tra nếu đã đến waypoint
        Debug.Log($"Moving to waypoint {Vector3.Distance(transform.position, targetPos)}: {targetPos}");
        if (Vector3.Distance(transform.position, targetPos) < 1)
        {
            currentWaypointIndex++;

            // Đến waypoint cuối → chạm base
            if (currentWaypointIndex >= WayPoints.Count - 1)
            {
                isAutoMoving = false;
            }
        }
    }
}
