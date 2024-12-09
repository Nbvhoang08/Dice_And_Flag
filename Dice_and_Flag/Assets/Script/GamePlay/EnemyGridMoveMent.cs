using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class EnemyGridMoveMent : MonoBehaviour
{
    // Start is called before the first frame update
    public Tilemap tilemap;
    public float moveSpeed = 5f;
    public Vector3Int _currentCell;
    public bool isMoving = false;
    public Enemy enemy;
    public bool Stunning;
    public bool arrivedAdress;
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
        arrivedAdress = false;
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
   
    public bool IsValidCell(Vector3Int cell)
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
        // Nếu đã đến ô mục tiêu
        if (_currentCell == targetCell)
        {
            arrivedAdress = true;
            Debug.Log($"Target cell reached: {_currentCell}");

            // Lấy danh sách các hướng hợp lệ từ ô hiện tại
            List<Vector2> validDirections = GetValidDirections(_currentCell);
            if (validDirections.Count > 0)
            {
                // Chọn ngẫu nhiên một hướng hợp lệ
                Vector2 randomDirection = validDirections[Random.Range(0, validDirections.Count)];
                Debug.Log($"Random direction chosen: {randomDirection}");
                return new Vector3(randomDirection.x, randomDirection.y, 0);
            }
            else
            {
                Debug.LogWarning($"No valid directions available from {_currentCell}");
                return Vector3.zero; // Không có hướng hợp lệ nào
            }
        }

        

        // Thuật toán BFS
        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        Dictionary<Vector3Int, Vector3Int> parentMap = new Dictionary<Vector3Int, Vector3Int>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();

        // Khởi tạo BFS
        queue.Enqueue(_currentCell);
        visited.Add(_currentCell);

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();

            // Debug: thông báo ô đang xử lý
            Debug.Log($"Processing cell: {current}");

            // Nếu tìm thấy ô mục tiêu
            if (current == targetCell)
            {
              

                Vector3Int pathStep = current;
                Vector3Int lastStep = current;

                // Truy ngược để tìm bước đầu tiên
                while (parentMap.ContainsKey(pathStep) && parentMap[pathStep] != _currentCell)
                {
                    lastStep = pathStep;
                    pathStep = parentMap[pathStep];
                }

                // Tính hướng đi
                Vector3Int diff = lastStep - _currentCell;
         

                // Chỉ trả về hướng đi chính
                if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
                {
                    return new Vector3(Mathf.Sign(diff.x), 0, 0);
                }
                else
                {
                    return new Vector3(0, Mathf.Sign(diff.y), 0);
                }
            }

            // Kiểm tra các ô liền kề
            Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
            foreach (var dir in directions)
            {
                Vector3Int adjacentCell = current + new Vector3Int((int)dir.x, (int)dir.y, 0);

                /*// Debug: kiểm tra ô liền kề
                if (!IsValidCell(adjacentCell))
                {
                    Debug.Log($"Invalid cell skipped: {adjacentCell}");
                }
                else if (visited.Contains(adjacentCell))
                {
                    Debug.Log($"Already visited cell skipped: {adjacentCell}");
                }*/

                // Thêm ô hợp lệ vào hàng đợi
                if (IsValidCell(adjacentCell) && !visited.Contains(adjacentCell))
                {
                    queue.Enqueue(adjacentCell);
                    visited.Add(adjacentCell);
                    parentMap[adjacentCell] = current;
                   
                }
            }
        }

  
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

            Vector3Int nextCell = currentCell + new Vector3Int((int)currentDirection.x, (int)currentDirection.y, 0);
           
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
                nextCell = currentCell + new Vector3Int((int)currentDirection.x, (int)currentDirection.y, 0);

                // Kiểm tra lại tính hợp lệ của ô
                if (!IsValidCell(nextCell))
                {
                   
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
