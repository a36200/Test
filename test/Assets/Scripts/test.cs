using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    [Header("Maze Size (odd numbers work best)")]
    [Min(5)] public int width = 21;
    [Min(5)] public int height = 15;

    [Header("Sprites / Prefabs")]
    public Sprite wallSprite;
    public Sprite floorSprite;

    [Header("Cell Settings")]
    public float cellSize = 1f;
    public bool regenerateOnPlay = true;
    public int seed = 0; // 0 = random

    bool[,] passable; // true = floor, false = wall

    void Start()
    {
        if (wallSprite == null || floorSprite == null)
        {
            Debug.LogError("Hãy kéo thả wallSprite và floorSprite vào Inspector!");
            return;
        }

        if (regenerateOnPlay)
        {
            GenerateMaze();
            RenderMaze();
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
        // Bảo đảm kích thước lẻ
        if (width % 2 == 0) width += 1;
        if (height % 2 == 0) height += 1;

        passable = new bool[width, height];

        // Seed
        if (seed == 0) Random.InitState(System.Environment.TickCount);
        else Random.InitState(seed);

        // Mặc định tường
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                passable[x, y] = false;

        // Carve từ (1,1)
        CarveMaze(1, 1);

        // Cửa vào/ra
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
        // Xóa object cũ (nếu regenerate)
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Sprite spriteToUse = passable[x, y] ? floorSprite : wallSprite;
                GameObject go = new GameObject($"Cell_{x}_{y}");
                go.transform.parent = transform;
                go.transform.position = new Vector3(x * cellSize, y * cellSize, 0);

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = spriteToUse;
            }
        }

        // Focus camera (nếu có Camera Main)
        var cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.transform.position = new Vector3(width * cellSize / 2f, height * cellSize / 2f, -10f);
            cam.orthographicSize = Mathf.Max(width, height) * cellSize * 0.6f;
        }
    }
}
