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
        if (Input.GetMouseButtonDown(0) && _currentDice == null)
        {
            SpawnDice();
            _isDragging = true;
        }

        if (_isDragging && _currentDice != null)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = mousePosition - spawnPosition.position;

            if (direction.magnitude > maxDistance)
            {
                direction = direction.normalized * maxDistance;
            }

            DrawTrajectory(spawnPosition.position, direction);

            if (Input.GetMouseButtonUp(0))
            {
                // Lấy điểm cuối cùng từ LineRenderer
                Vector3 targetPoint = lineRenderer.GetPosition(lineRenderer.positionCount - 1);
                ThrowDice(targetPoint);
                _isDragging = false;
            }
        }
    }

    void ThrowDice(Vector3 targetPoint)
    {
        Rigidbody2D rb = _currentDice.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            StartCoroutine(ThrowDiceCoroutine(rb, targetPoint));
        }

        _currentDice = null;
        lineRenderer.positionCount = 0;
    }

    IEnumerator ThrowDiceCoroutine(Rigidbody2D rb, Vector3 targetPoint)
    {
        Vector3 startPos = rb.transform.position;
        float journeyLength = Vector3.Distance(startPos, targetPoint);
        float throwDuration = journeyLength / throwForce; // Thời gian di chuyển dựa vào khoảng cách

        float elapsedTime = 0;

        // Tính toán đường cong
        Vector3 controlPoint = (startPos + targetPoint) / 2;
        controlPoint.y += journeyLength * 0.5f; // Độ cao của đường cong

        while (elapsedTime < throwDuration)
        {
            float t = elapsedTime / throwDuration;

            // Tính toán vị trí trên đường cong Bezier bậc 2
            Vector3 newPosition = Vector3.Lerp(
                Vector3.Lerp(startPos, controlPoint, t),
                Vector3.Lerp(controlPoint, targetPoint, t),
                t
            );

            rb.MovePosition(newPosition);

            // Thêm hiệu ứng xoay
            rb.angularVelocity = 360f;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

       

        // Tạo hiệu ứng nảy
        StartCoroutine(BounceEffect(rb));
    }

    IEnumerator BounceEffect(Rigidbody2D rb)
    {
        float bounceHeight = 0.5f; // Độ cao nảy ban đầu
        float bounceDuration = 0.2f; // Thời gian mỗi lần nảy
        int bounceCount = 3; // Số lần nảy

        Vector3 originalPos = rb.transform.position;

        for (int i = 0; i < bounceCount; i++)
        {
            // Nảy lên
            float elapsedTime = 0;
            Vector3 startPos = rb.transform.position;
            Vector3 bouncePos = startPos + Vector3.up * (bounceHeight);

            while (elapsedTime < bounceDuration / 2)
            {
                float t = elapsedTime / (bounceDuration / 2);
                rb.MovePosition(Vector3.Lerp(startPos, bouncePos, t));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Rơi xuống
            elapsedTime = 0;
            startPos = rb.transform.position;

            while (elapsedTime < bounceDuration / 2)
            {
                float t = elapsedTime / (bounceDuration / 2);
                rb.MovePosition(Vector3.Lerp(startPos, originalPos, t));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Giảm độ cao cho lần nảy tiếp theo
            bounceHeight *= 0.5f;
            bounceDuration *= 0.8f;
            // Khi đến đích
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0;
            yield return new WaitForSeconds(0.1f);
          
        }
    }

    void DrawTrajectory(Vector2 start, Vector2 direction)
    {
        lineRenderer.positionCount = resolution;
        Vector3[] points = new Vector3[resolution];

        // Điều chỉnh hướng để phù hợp với lực ném mới
        Vector2 throwDirection = direction.normalized;
        throwDirection += Vector2.up * 0.5f;
        throwDirection.Normalize();

        float velocity = direction.magnitude * throwForce;
        float g = Physics2D.gravity.y * 0.5f; // Phải nhân với gravityScale

        float maxTime = 2f; // Thời gian tối đa để vẽ quỹ đạo

        for (int i = 0; i < resolution; i++)
        {
            float t = maxTime * (i / (float)(resolution - 1));

            // Tính toán vị trí dựa trên phương trình chuyển động
            float x = start.x + throwDirection.x * velocity * t;
            float y = start.y + throwDirection.y * velocity * t + 0.5f * g * t * t;

            points[i] = new Vector3(x, y, 0);
        }

        lineRenderer.SetPositions(points);
    }
    void SpawnDice()
    {
        _currentDice = Instantiate(dicePrefab, spawnPosition.position, Quaternion.identity);
        _currentDice.GetComponent<Rigidbody2D>().gravityScale = 0;
        lineRenderer.positionCount = 0; // Xóa quỹ đạo cũ
    }
    public int RandomStep()
    {
        return Random.Range(1, 6);
    }
    public void Move()
    {

    }
}
