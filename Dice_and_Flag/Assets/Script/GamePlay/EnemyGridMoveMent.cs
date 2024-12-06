using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyGridMoveMent : MonoBehaviour
{
    // Start is called before the first frame update
    public Tilemap tilemap;
    public float moveSpeed = 5f;
    public Vector3Int _currentCell;
    public bool isMoving = false;
    public Enemy enemy;
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

        enemy = GetComponent<Enemy>();

        Stunning = false;
    }

    public void Move(Vector2 direction, int steps)
    {
        if (isMoving) return;
        if (Stunning) return;
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
                enemy.ChangeAnim("stun");
                Stunning = true;
                StartCoroutine(ResetStun());
                if (collision.gameObject.transform.position.x <= transform.position.x)
                {
                    enemy.sprite.flipX = false;
                }
                else
                {
                    enemy.sprite.flipX = true;
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
            enemy.ChangeAnim("walk");
            yield return null;
        }

        // Đảm bảo đối tượng ở đúng vị trí mục tiêu sau khi di chuyển
        transform.position = targetPosition;
        isMoving = false; // Đặt lại cờ khi di chuyển hoàn tất
        enemy.ChangeAnim("idle");
    }



    private IEnumerator MoveThroughCells(Vector3Int startCell, Vector2 initialDirection, int steps)
    {
        isMoving = true;
        Vector3Int currentCell = startCell;
        Vector2 currentDirection = initialDirection;
        int remainingSteps = steps;
        if (initialDirection == Vector2.right)
        {
            enemy.sprite.flipX = true;
        }
        else if (initialDirection == Vector2.left)
        {
            enemy.sprite.flipX = false;
        }
        while (remainingSteps > 0)
        {
            // Tìm ô tiếp theo
            Vector3Int nextCell = currentCell + new Vector3Int((int)currentDirection.x, (int)currentDirection.y, 0);
            enemy.ChangeAnim("walk");
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
                    // Nếu có nhiều hơn 2 hướng đi tại vị trí hiện tại
                    validDirections.Remove(oppositeDirection); // Loại bỏ hướng ngược lại
                    if (validDirections.Count > 0)
                    {
                        // Chọn ngẫu nhiên một hướng hợp lệ từ danh sách còn lại
                        currentDirection = validDirections[UnityEngine.Random.Range(0, validDirections.Count)];
                        nextCell = currentCell + new Vector3Int((int)currentDirection.x, (int)currentDirection.y, 0);
                    }
                    else
                    {
                        // Nếu không còn hướng nào khác ngoài hướng ngược lại, dừng lại
                        break;
                    }
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
            enemy.NameText.text = remainingSteps.ToString();
            List<Vector2> currentValidDirections = GetValidDirections(currentCell);

            if (currentValidDirections.Count >= 3 && remainingSteps > 0)
            {

                Debug.Log(currentValidDirections.Count);
                enemy.stepDice = remainingSteps;
                enemy.ChangeAnim("idle");
                break;

            }
        }
        _currentCell = currentCell;
        isMoving = false;
        enemy.ChangeAnim("idle");
        if (remainingSteps <= 0)
        {
            enemy.gameManager.Next();
        }

    }
}
