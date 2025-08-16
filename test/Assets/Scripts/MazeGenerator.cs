using System.Collections.Generic;
using UnityEngine;
using System.Linq; // để dùng .Reverse()

public class MazeGenerator : MonoBehaviour
{
    [Header("Maze Size (odd numbers work best)")]
    [Min(5)] public int width = 21;
    [Min(5)] public int height = 15;

    [Header("Prefabs")]
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject playerPrefab;
    public GameObject goalPrefab;

    [Header("Cell Settings")]
    public float cellSize = 1f;
    public bool regenerateOnPlay = true;
    public int seed = 0; // 0 = random

    bool[,] passable; // true = floor, false = wall

    void Start()
    {
        if (wallPrefab == null || floorPrefab == null || playerPrefab == null || goalPrefab == null)
        {
            Debug.LogError("Kéo thả Wall, Floor, Player, Goal Prefab vào Inspector!");
            return;
        }

        if (regenerateOnPlay)
        {
            GenerateMaze();
            RenderMaze();
            FindShortestPath();
        }
    }

    [ContextMenu("Generate & Render Maze")]
    public void GenerateMazeAndRender()
    {
        GenerateMaze();
        RenderMaze();
    }

    void GenerateMaze()
    {
        if (width % 2 == 0) width += 1;
        if (height % 2 == 0) height += 1;

        passable = new bool[width, height];

        if (seed == 0) Random.InitState(System.Environment.TickCount);
        else Random.InitState(seed);

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                passable[x, y] = false;

        CarveMaze(1, 1);

        passable[0, 1] = true;
        passable[width - 1, height - 2] = true;
        passable[width - 2, height - 2] = true;
    }

    void CarveMaze(int sx, int sy)
    {
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        Vector2Int current = new Vector2Int(sx, sy);
        passable[current.x, current.y] = true;
        stack.Push(current);

        Vector2Int[] dirs = new Vector2Int[]
        {
            new Vector2Int( 0,  2),
            new Vector2Int( 0, -2),
            new Vector2Int( 2,  0),
            new Vector2Int(-2,  0),
        };

        while (stack.Count > 0)
        {
            current = stack.Peek();

            List<Vector2Int> neighbors = new List<Vector2Int>();
            foreach (var d in dirs)
            {
                int nx = current.x + d.x;
                int ny = current.y + d.y;
                if (nx > 0 && nx < width - 1 && ny > 0 && ny < height - 1 && !passable[nx, ny])
                    neighbors.Add(new Vector2Int(nx, ny));
            }

            if (neighbors.Count == 0)
            {
                stack.Pop();
                continue;
            }

            var next = neighbors[Random.Range(0, neighbors.Count)];

            int mx = (current.x + next.x) / 2;
            int my = (current.y + next.y) / 2;
            passable[mx, my] = true;
            passable[next.x, next.y] = true;

            stack.Push(next);
        }
    }

    void RenderMaze()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        Vector3 playerStart = Vector3.zero;
        Vector3 goalPos = Vector3.zero;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject prefabToUse = passable[x, y] ? floorPrefab : wallPrefab;
                Vector3 pos = new Vector3(x * cellSize, 0, y * cellSize);
                GameObject go = Instantiate(prefabToUse, pos, Quaternion.identity, transform);
                go.transform.localScale = new Vector3(cellSize, go.transform.localScale.y, cellSize);

                // Lưu vị trí Player và Goal
                if (x == 0 && y == 1) playerStart = pos + Vector3.up * 0.5f;
                if (x == width - 1 && y == height - 2) goalPos = pos + Vector3.up * 0.5f;
            }
        }

        // Spawn Player
        GameObject player = Instantiate(playerPrefab, playerStart, Quaternion.identity);
        Player = player.GetComponent<PlayerController>();
        Camera.main.GetComponent<CameraFollow>()?.SetTarget(player.transform);

        // Spawn Goal
        Instantiate(goalPrefab, goalPos, Quaternion.identity);
    }
    private PlayerController Player;

    List<Vector3> shortestPath = new List<Vector3>();
    void FindShortestPath()
    {
        shortestPath.Clear();

        Vector2Int start = new Vector2Int(0, 1);
        Vector2Int goal = new Vector2Int(width - 1, height - 2);

        Queue<Vector2Int> q = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        q.Enqueue(start);
        cameFrom[start] = start;

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (q.Count > 0)
        {
            var current = q.Dequeue();
            if (current == goal)
                break;

            foreach (var d in dirs)
            {
                var next = current + d;
                if (next.x >= 0 && next.x < width && next.y >= 0 && next.y < height)
                {
                    if (passable[next.x, next.y] && !cameFrom.ContainsKey(next))
                    {
                        q.Enqueue(next);
                        cameFrom[next] = current;
                    }
                }
            }
        }

        // reconstruct path
        if (!cameFrom.ContainsKey(goal)) return;

        Vector2Int cur = goal;
        while (cur != start)
        {
            shortestPath.Add(new Vector3(cur.x * cellSize, 0.2f, cur.y * cellSize));
            cur = cameFrom[cur];
        }
        shortestPath.Add(new Vector3(start.x * cellSize, 0.2f, start.y * cellSize));
        shortestPath.Reverse();
        Player.SetData(shortestPath, true); // Set data for PlayerController to auto-move along the path
    }
    void OnDrawGizmos()
    {
        if (shortestPath == null || shortestPath.Count < 2) return;

        Gizmos.color = Color.red;
        for (int i = 0; i < shortestPath.Count - 1; i++)
        {
            Gizmos.DrawSphere(shortestPath[i], 0.1f);
            Gizmos.DrawLine(shortestPath[i], shortestPath[i + 1]);
        }
    }

}
