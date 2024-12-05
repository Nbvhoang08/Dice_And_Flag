using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GirdMoveMent : MonoBehaviour
{
    public Tilemap tilemap;
    public float moveSpeed = 5f;
    [SerializeField]private Vector3Int _currentCell;
    [SerializeField]private bool isMoving = false;

    private void Start()
    {
        _currentCell = tilemap.WorldToCell(transform.position);
        transform.position = tilemap.CellToWorld(_currentCell) + tilemap.tileAnchor;
    }

    public void Move(Vector2 direction, int steps)
    {
        if (isMoving) return;
        StartCoroutine(MoveThroughCells(_currentCell, direction, steps));
    }

    private bool IsValidCell(Vector3Int cell)
    {
        return tilemap.HasTile(cell);
    }

    private List<Vector2> GetValidDirections(Vector3Int currentCell)
    {
        List<Vector2> validDirections = new List<Vector2>();
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        foreach (var dir in directions)
        {
            Vector3Int adjacentCell = currentCell + new Vector3Int((int)dir.x, (int)dir.y, 0);
            if (IsValidCell(adjacentCell))
            {
                validDirections.Add(dir);
            }
        }
        return validDirections;
    }

    private IEnumerator MoveThroughCells(Vector3Int startCell, Vector2 initialDirection, int steps)
    {
        isMoving = true;
        Vector3Int currentCell = startCell;
        Vector2 currentDirection = initialDirection;
        int remainingSteps = steps;
 
        while (remainingSteps > 0)
        {
            // Tìm ô tiếp theo
            Vector3Int nextCell = currentCell + new Vector3Int((int)currentDirection.x, (int)currentDirection.y, 0);

            // Nếu ô tiếp theo không hợp lệ
            if (!IsValidCell(nextCell))
            {
                // Lấy các hướng hợp lệ tại ô hiện tại
                List<Vector2> validDirections = GetValidDirections(currentCell);

                if (validDirections.Count == 1)
                {
                    // Chọn hướng hợp lệ duy nhất
                    currentDirection = validDirections[0];
                    nextCell = currentCell + new Vector3Int((int)currentDirection.x, (int)currentDirection.y, 0);
                }
                else
                {
                    // Nếu có nhiều hơn 1 hướng đi tại vị trí hiện tại, dừng lại
                    break;
                }
            }

            // Di chuyển đến ô tiếp theo
            Vector3 startPos = transform.position;
            Vector3 endPos = tilemap.CellToWorld(nextCell) + tilemap.tileAnchor;
            float time = 0;
            while (time < 1f)
            {
                time += Time.deltaTime * moveSpeed;
                transform.position = Vector3.Lerp(startPos, endPos, time);
                yield return null;
            }

            // Cập nhật vị trí sau khi đến ô
            transform.position = endPos;
            currentCell = nextCell;
            remainingSteps--;
            Debug.Log(remainingSteps);
            // Kiểm tra số hướng đi tại ô hiện tại
            List<Vector2> currentValidDirections = GetValidDirections(currentCell);
            /*
            if (currentValidDirections.Count > 1)
            {
                // Nếu có nhiều hơn 1 hướng đi, dừng lại
                Debug.Log("?");
                break;

            }*/
        }
        _currentCell = currentCell;
        isMoving = false;
    }

    private void Update()
    {
        if (!isMoving)
        {
            
            if (Input.GetKeyDown(KeyCode.W)) Move(Vector2.right, 6);
            if (Input.GetKeyDown(KeyCode.S)) Move(Vector2.down, 6);
            if (Input.GetKeyDown(KeyCode.A)) Move(Vector2.left, 6);
            if (Input.GetKeyDown(KeyCode.D)) Move(Vector2.right, 6);
        }

       
        
    }
}
