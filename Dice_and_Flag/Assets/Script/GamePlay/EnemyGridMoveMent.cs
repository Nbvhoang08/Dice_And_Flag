using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class EnemyGridMoveMent : MonoBehaviour
{
    // Start is called before the first frame update
    
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
    private void Update()
    {

        if (!Stunning && !isMoving)
        {
            Vector2 position = new Vector2(transform.position.x, transform.position.y);
            Vector2 currentCell = new Vector2(_currentCell.x, _currentCell.y);
            if (Vector2.Distance(position, currentCell) >= 0.1f)
            {

                MoveReturn();
            }

        }
    }
     


    public void Move(Vector2 direction, int steps)
    {
        if (isMoving) return;
        if (Stunning) return;
        StartCoroutine(MoveThroughCells(_currentCell, direction, steps));
    }
    public Tilemap tilemap;
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
    public Vector3 GetRandomDirection(Vector3Int targetCell)
    {
        // Nếu đã ở ô đích, trả về vector không
        if (_currentCell == targetCell) return Vector3.zero;

        // Sử dụng thuật toán tìm kiếm theo chiều rộng (BFS)
        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        Dictionary<Vector3Int, Vector3Int> parentMap = new Dictionary<Vector3Int, Vector3Int>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();

        queue.Enqueue(_currentCell);
        visited.Add(_currentCell);

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();

            // Nếu đã đến ô đích, quay lui để tìm bước đầu tiên
            if (current == targetCell)
            {
                Vector3Int pathStep = current;
                Vector3Int lastStep = current;

                // Quay lui để tìm bước đầu tiên từ ô hiện tại
                while (parentMap.ContainsKey(pathStep) && parentMap[pathStep] != _currentCell)
                {
                    lastStep = pathStep;
                    pathStep = parentMap[pathStep];
                }

                // Tính toán hướng của bước đầu tiên
                Vector2 direction = new Vector2(
                    lastStep.x - _currentCell.x,
                    lastStep.y - _currentCell.y
                );

                return new Vector3(direction.x, direction.y, 0);
            }

            // Kiểm tra các ô kề
            Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
            foreach (var dir in directions)
            {
                Vector3Int adjacentCell = current + new Vector3Int((int)dir.x, (int)dir.y, 0);

                // Kiểm tra xem ô kề có hợp lệ và chưa được thăm
                if (IsValidCell(adjacentCell) && !visited.Contains(adjacentCell))
                {
                    queue.Enqueue(adjacentCell);
                    visited.Add(adjacentCell);
                    parentMap[adjacentCell] = current;
                }
            }
           
        }

        // Không tìm thấy đường đi
        return Vector3.zero;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Dice"))
        {
            if (collision.gameObject.GetComponent<Dice>().Invicable && !Stunning && collision.gameObject != enemy._currentDice && !enemy.Death)
            {
                enemy.ChangeAnim("stun");
                Stunning = true;
                StartCoroutine(ResetStun());
            
                collision.gameObject.GetComponent<Dice>().durability = 0;
                enemy.currentHp --;
                SoundManager.Instance.PlayVFXSound(1);
                if (collision.gameObject.transform.position.x <= transform.position.x)
                {
                    enemy.sprite.flipX = false;
                }
                else
                {
                    enemy.sprite.flipX = true;
                }
            }
            else if (!collision.gameObject.GetComponent<Dice>().Invicable)
            {
                
                Vector2 position = new Vector2(transform.position.x, transform.position.y);
                Vector2 currentCell = new Vector2(_currentCell.x, _currentCell.y);
                if (Vector2.Distance(position, currentCell) >= 0.1f)
                { 
                    MoveReturn();
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
    public void MoveReturn()
    {
        if (Stunning)
        {
            enemy.ChangeAnim("stun");
            Debug.Log("stun ");
            StartCoroutine(ResetStun());
            return;
        }
          
        
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
        Stunning = false;
  
      

    }
    private IEnumerator MoveThroughCells(Vector3Int startCell, Vector2 initialDirection, int steps)
    {
        isMoving = true;
        Vector3Int currentCell = startCell;
        Vector2 currentDirection = initialDirection;
        int remainingSteps = steps;

        while (remainingSteps > 0)
        {
            // Xử lý việc lật sprite
            if (currentDirection == Vector2.right)
            {
                enemy.sprite.flipX = true;
            }
            else if (currentDirection == Vector2.left)
            {
                enemy.sprite.flipX = false;
            }

            Vector3Int nextCell = currentCell + new Vector3Int((int)currentDirection.x/2, (int)currentDirection.y/2, 0);
            Debug.Log(currentDirection.x + " " + currentDirection.y);
            enemy.ChangeAnim("walk");

            // Kiểm tra nếu ô tiếp theo không hợp lệ
            if (!IsValidCell(nextCell))
            {
                
                // Lấy các hướng hợp lệ tại ô hiện tại
                List<Vector2> validDirections = GetValidDirections(currentCell);

                // Tìm hướng ngược lại của initialDirection
                Vector2 oppositeDirection = -initialDirection;

                // Loại bỏ hướng ngược lại khỏi các hướng hợp lệ
                validDirections.Remove(oppositeDirection);

                // Nếu không còn hướng đi nào
                if (validDirections.Count == 0)
                {
                    Debug.Log("Không còn hướng đi hợp lệ. Dừng di chuyển.");
                    break;
                }

                // Nếu chỉ còn một hướng đi
                if (validDirections.Count == 1)
                {
                    currentDirection = validDirections[0];           
                }
                else
                {
                    // Nếu có nhiều hướng, loại bỏ hướng ban đầu
                    validDirections.Remove(initialDirection);

                    // Nếu sau khi loại bỏ vẫn còn hướng
                    if (validDirections.Count > 0)
                    {
                        // Chọn ngẫu nhiên một hướng
                        currentDirection = validDirections[UnityEngine.Random.Range(0, validDirections.Count)];
                    }
                    else
                    {
                        // Quay lại hướng ban đầu nếu không còn lựa chọn khác
                        currentDirection = initialDirection;
                    }
                }

                // Cập nhật lại ô tiếp theo
                nextCell = currentCell + new Vector3Int((int)currentDirection.x / 2, (int)currentDirection.y/2, 0);

                // Kiểm tra lại tính hợp lệ của ô
                if (!IsValidCell(nextCell))
                {
                    Debug.Log("Không thể tìm được ô di chuyển hợp lệ.");
                    break;
                }

                // Giảm số bước ngay khi thay đổi hướng do gặp ô không hợp lệ
                remainingSteps--;
            }

            // Di chuyển đến ô tiếp theo
            Vector3 startPos = transform.position;
            Vector3 endPos = tilemap.CellToWorld(nextCell) + tilemap.tileAnchor;
            float time = 0;
            while (time < 1f)
            {
                time += Time.deltaTime * moveSpeed;
                transform.position = Vector3.Lerp(startPos, endPos, time);
                //Debug.Log(startPos + " " + endPos);
                yield return null;
            }

            // Cập nhật vị trí sau khi đến ô
            transform.position = endPos;
            currentCell = nextCell;
            remainingSteps--;
            enemy.NameText.text = remainingSteps.ToString();

            // Kiểm tra điều kiện dừng
            if (remainingSteps <= 0)
            {
                break;
            }
        }

        // Kết thúc di chuyển
        _currentCell = currentCell;
        isMoving = false;
        enemy.ChangeAnim("idle");

        // Chuyển lượt
        if (remainingSteps <= 0)
        {
            enemy.gameManager.Next();
        }
    }
}
