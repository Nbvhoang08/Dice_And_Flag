using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject dicePrefab; // Prefab xúc xắc
    public LineRenderer lineRenderer; // LineRenderer để vẽ quỹ đạo
    public float throwForce = 10f; // Lực ném
    public float maxDistance = 5f; // Tầm ném tối đa
    public int resolution = 50; // Số điểm trên quỹ đạo
    public Transform spawnPosition ; // Vị trí cố định để tạo xúc xắc

    private GameObject _currentDice; // Viên xúc xắc hiện tại
    private bool _isDragging = false; // Để kiểm tra trạng thái kéo chuột
    private Vector2[] _segment;
    [SerializeField]private GirdMoveMent gridMovement;

    private void Start()
    {
        gridMovement = GetComponent<GirdMoveMent>();
    }


    void Update()
    {
       /* if (Input.GetMouseButtonDown(0) && _currentDice == null)
        {
            SpawnDice();
            _isDragging = true;
        }

        if (_isDragging && _currentDice != null)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = mousePosition - spawnPosition.position;

            // Giới hạn hướng ném trong tầm ném
            if (direction.magnitude > maxDistance)
            {
                direction = direction.normalized * maxDistance;
            }

            DrawTrajectory(spawnPosition.position, direction);

            if (Input.GetMouseButtonUp(0)) // Chuột trái để ném
            {
                ThrowDice(direction);
                _isDragging = false;
            }
        }*/
       




    }

    void SpawnDice()
    {
        _currentDice = Instantiate(dicePrefab, spawnPosition.position, Quaternion.identity);
        _currentDice.GetComponent<Rigidbody2D>().gravityScale = 0;
        lineRenderer.positionCount = 0; // Xóa quỹ đạo cũ
    }

    void DrawTrajectory(Vector2 start, Vector2 direction)
    {
        lineRenderer.positionCount = resolution; // Số lượng điểm trên quỹ đạo
        Vector3[] points = new Vector3[resolution];

        float angle = Mathf.Atan2(direction.y, direction.x); // Góc ném
        float velocity = direction.magnitude * throwForce; // Vận tốc ban đầu
        float g = Mathf.Abs(Physics2D.gravity.y); // Gia tốc trọng trường

        // Tính thời gian tối đa dựa trên vận tốc và góc
        float totalTime = (2 * velocity * Mathf.Sin(angle)) / g;

        for (int i = 0; i < resolution; i++)
        {
            // Tính thời gian dựa trên vị trí trong resolution
            float t = totalTime * (i / (float)(resolution - 1));

            // Tính tọa độ x và y theo công thức vật lý
            float x = velocity * t * Mathf.Cos(angle);
            float y = velocity * t * Mathf.Sin(angle) - 0.5f * g * t * t;

            // Cập nhật điểm trên quỹ đạo
            points[i] = new Vector3(start.x + x, start.y + y, 0);
        }

        // Gán các điểm vào LineRenderer
        lineRenderer.SetPositions(points);

        // Điều chỉnh khoảng cách và kích thước của sprite trong LineRenderer
      
       
    }
    
   
    public int RandomStep()
    {
        return Random.Range(1,6);    
    }
    public void Move()
    {
           
    }


    void ThrowDice(Vector2 direction)
    {
        Rigidbody2D rb = _currentDice.GetComponent<Rigidbody2D>();
        
        if (rb != null)
        {
            rb.velocity = direction.normalized * direction.magnitude * throwForce;
            rb.gravityScale = 1;
        }

        _currentDice = null; // Reset để chuẩn bị cho lần tiếp theo
        lineRenderer.positionCount = 0; // Xóa quỹ đạo sau khi ném
    }
}
