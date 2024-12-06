using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D.Aseprite;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GirdMoveMent : MonoBehaviour
{
    public Tilemap tilemap;
    public float moveSpeed = 5f;
    public Vector3Int _currentCell;
    public bool isMoving = false;
    public Player player;
    public bool Stunning;
    public void Awake()
    {
        if (tilemap == null)
        {
            tilemap = FindAnyObjectByType<Tilemap>();
        }

    }
    
    private void Start()
    {
        _currentCell = tilemap.WorldToCell(transform.position);

        // Lấy tọa độ chính giữa của ô lưới
        Vector3 cellCenterPosition = tilemap.GetCellCenterWorld(_currentCell);

        // Đặt vị trí của đối tượng vào chính giữa ô
        transform.position = cellCenterPosition;

        player = GetComponent<Player>();
     
        Stunning = false;
    }
    
    public void Move(Vector2 direction, int steps)
    {
        if (isMoving) return;
        if(Stunning) return;
        StartCoroutine(MoveThroughCells(_currentCell, direction, steps));
    }

    private bool IsValidCell(Vector3Int cell)
    {
        return tilemap.HasTile(cell);
    }

    public List<Vector2> GetValidDirections(Vector3Int currentCell)
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
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Dice"))
        {
            if (collision.gameObject.GetComponent<Dice>().Invicable && !Stunning)
            {
                player.ChangeAnim("stun");
                Stunning = true;
                StartCoroutine(ResetStun());
                if(collision.gameObject.transform.position.x<= transform.position.x)
                {
                    player.sprite.flipX = false;
                }
                else
                {
                    player.sprite.flipX = true;    
                }
            }
        }
    }
    IEnumerator ResetStun()
    {
        yield return new WaitForSeconds(2f);
        Stunning = false;
        MoveReturn();
    }
    void MoveReturn()
    {
        // Vị trí ban đầu hoặc vị trí mục tiêu để trở về
        Vector3 targetPosition = _currentCell; // startPosition là vị trí ban đầu của đối tượng

        // Bắt đầu coroutine để di chuyển đối tượng
        StartCoroutine(MoveToPosition(targetPosition));
    }

    IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        if (isMoving) yield break; // Nếu đối tượng đang di chuyển, không bắt đầu di chuyển mới

        isMoving = true;
        Vector3 startPos = transform.position;
        float time = 0;
        float moveSpeed = 2f; // Tốc độ di chuyển, bạn có thể điều chỉnh

        while (time < 1f)
        {
            time += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(startPos, targetPosition, time);
            player.ChangeAnim("walk");
            yield return null;
        }

        // Đảm bảo đối tượng ở đúng vị trí mục tiêu sau khi di chuyển
        transform.position = targetPosition;
        isMoving = false; // Đặt lại cờ khi di chuyển hoàn tất
        player.ChangeAnim("idle");
    }



    private IEnumerator MoveThroughCells(Vector3Int startCell, Vector2 initialDirection, int steps)
    {
        isMoving = true;
        Vector3Int currentCell = startCell;
        Vector2 currentDirection = initialDirection;
        int remainingSteps = steps;
        if (initialDirection == Vector2.right)
        {
            player.sprite.flipX = true;
        }
        else if (initialDirection == Vector2.left)
        {
            player.sprite.flipX = false;
        }
        while (remainingSteps > 0)
        {
            // Tìm ô tiếp theo
            Vector3Int nextCell = currentCell + new Vector3Int((int)currentDirection.x, (int)currentDirection.y, 0);
            player.ChangeAnim("walk");
            // Nếu ô tiếp theo không hợp lệ
            if (!IsValidCell(nextCell))
            {
                // Lấy các hướng hợp lệ tại ô hiện tại
                List<Vector2> validDirections = GetValidDirections(currentCell);

                // Tìm hướng ngược lại của initialDirection
                Vector2 oppositeDirection = -initialDirection;

                if (validDirections.Count == 2)
                {
                    // Chọn hướng hợp lệ duy nhất khác với hướng ngược lại của initialDirection
                    foreach (Vector2 direction in validDirections)
                    {
                        if (direction != oppositeDirection)
                        {
                            currentDirection = direction;
                            break;
                        }
                    }
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
            //Debug.Log(remainingSteps);
            // Kiểm tra số hướng đi tại ô hiện tại
            List<Vector2> currentValidDirections = GetValidDirections(currentCell);

            if (currentValidDirections.Count >= 3 && remainingSteps > 0)
            {

                Debug.Log(currentValidDirections.Count);
                player.stepDice = remainingSteps;
                player.ChangeAnim("idle");
                player.CanMove = true;
                break;

            }
        }
        _currentCell = currentCell;
        isMoving = false;
        player.ChangeAnim("idle");
        if (remainingSteps <= 0)
        {
            player.gameManager.Next();
        }

    }

}
