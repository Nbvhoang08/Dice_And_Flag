using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using static UnityEngine.GraphicsBuffer;
using static UnityEditor.Experimental.GraphView.GraphView;


public class Enemy : Character
{
    // Start is called before the first frame update
    public GameObject dicePrefab; // Prefab xúc xắc
    public float throwForce = 10f; // Lực ném
    public float maxDistance = 5f; // Tầm ném tối đa
    public Transform spawnPosition; // Vị trí cố định để tạo xúc xắc

    [SerializeField] private GameObject _currentDice; // Viên xúc xắc hiện tại
    [SerializeField] private EnemyGridMoveMent gridMovement;
    public int stepDice = 0;
    public bool CanMove;
    [SerializeField] private Animator anim;
    private string _currentAnimName;
    public GameManager gameManager;
    public SpriteRenderer sprite;
    public String Name;
    public TextMeshPro NameText;
    [SerializeField] private Transform player1; // Đối tượng player 1
    [SerializeField] private Transform player2; // Đối tượng player 2
    [SerializeField] private float detectionRange = 10f; // Phạm vi phát hiện player
    [SerializeField] private float minDistanceToPlayer = 2f; // Khoảng cách ném tối thiểu tới player
    [SerializeField] private float maxDistanceToPlayer = 5f; // Khoảng cách ném tối đa tới player

    private void Awake()
    {
        isYourTurn = false;
    }

    private void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        gridMovement = GetComponent<EnemyGridMoveMent>();
        anim = GetComponent<Animator>();
        CanMove = false;
    }

    void Update()
    {
        if (isYourTurn)
        {
            StartCoroutine(EnemyTurn());
            isYourTurn = false; // Đảm bảo chỉ thực hiện một lần khi đến lượt
        }
    }

    private IEnumerator EnemyTurn()
    {
        // Spawn dice
        SpawnDice();
        ChangeAnim("hold");
        yield return new WaitForSeconds(1f); // Giữ trạng thái "hold" trong 1 giây

        // Chọn hướng ngẫu nhiên và ném xúc xắc
        Vector3 randomDirection = GetRandomTarget();
        ThrowDice(randomDirection);
    }

    private Vector3 GetRandomTarget()
    {
        // Giới hạn số lần thử (trong trường hợp đặc biệt cần tạo mục tiêu ngẫu nhiên)
        const int maxAttempts = 100;
        int attempts = 0;

        Vector3 randomTarget;

        // Kiểm tra khoảng cách đến từng player
        bool isPlayer1InRange = Vector3.Distance(player1.position, spawnPosition.position) <= detectionRange;
        bool isPlayer2InRange = Vector3.Distance(player2.position, spawnPosition.position) <= detectionRange;

        // Nếu chỉ player1 trong phạm vi, ném về phía player1
        if (isPlayer1InRange && !isPlayer2InRange)
        {
            return GetTargetTowardsPlayer(player1);
        }
        // Nếu chỉ player2 trong phạm vi, ném về phía player2
        else if (isPlayer2InRange && !isPlayer1InRange)
        {
            return GetTargetTowardsPlayer(player2);
        }
        // Nếu cả hai player trong phạm vi, ném ngẫu nhiên về một trong hai
        else if (isPlayer1InRange && isPlayer2InRange)
        {
            Transform chosenPlayer = UnityEngine.Random.value > 0.5f ? player1 : player2;
            return GetTargetTowardsPlayer(chosenPlayer);
        }

        // Nếu không có player nào trong phạm vi, chọn một vị trí ngẫu nhiên trong màn hình
        do
        {
            float randomAngle = UnityEngine.Random.Range(60f, 300f);
            Debug.Log(randomAngle);
            float randomDistance = UnityEngine.Random.Range(0f, maxDistance);

            float x = Mathf.Cos(randomAngle * Mathf.Deg2Rad) * randomDistance;
            float y = Mathf.Sin(randomAngle * Mathf.Deg2Rad) * randomDistance;

            randomTarget = spawnPosition.position + new Vector3(x, y, 0);
            
            // Kiểm tra xem mục tiêu có nằm trong camera view frustum hay không
            Vector3 viewportPoint = Camera.main.WorldToViewportPoint(randomTarget);
            bool isInsideViewport = viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
                            viewportPoint.y >= 0 && viewportPoint.y <= 1 &&
                            viewportPoint.z > 0; // Đảm bảo mục tiêu nằm trước camera
     
            if (isInsideViewport) return randomTarget;

            attempts++;
        } while (attempts < maxAttempts);

            Debug.LogWarning("Không tìm được vị trí hợp lệ, trả về vị trí mặc định.");
            return spawnPosition.position;
    }
    private Vector3 GetTargetTowardsPlayer(Transform player)
    {
        Vector3 direction = (player.position - spawnPosition.position).normalized;
        float randomDistance = UnityEngine.Random.Range(minDistanceToPlayer, maxDistanceToPlayer);
        return spawnPosition.position + direction * randomDistance;
    }

    void ThrowDice(Vector3 targetPoint)
    {
        Rigidbody2D rb = _currentDice.GetComponent<Rigidbody2D>();
   
        if (rb != null)
        {
            StartCoroutine(ThrowDiceCoroutine(rb, targetPoint));
            _currentDice.GetComponent<BoxCollider2D>().enabled = true;
        }
    }
        private Vector3 GetRandomDirection()
        {
            // Lấy một hướng ngẫu nhiên từ các hướng hợp lệ
            List<Vector2> validDirections = gridMovement.GetValidDirections(gridMovement._currentCell);
            if (validDirections.Count == 0) return Vector3.zero;

            Vector2 chosenDirection = validDirections[UnityEngine.Random.Range(0, validDirections.Count)];
            return new Vector3(chosenDirection.x, chosenDirection.y, 0);
        }


        IEnumerator ThrowDiceCoroutine(Rigidbody2D rb, Vector3 targetPoint)
        {
            Vector3 startPos = rb.transform.position;
            float journeyLength = Vector3.Distance(startPos, targetPoint);
            float throwDuration = journeyLength / throwForce;

            float elapsedTime = 0;
            while (elapsedTime < throwDuration && rb != null)
            {
                float t = elapsedTime / throwDuration;
                Vector3 newPosition = Vector3.Lerp(startPos, targetPoint, t);
                rb.MovePosition(newPosition);

                // Thêm hiệu ứng xoay
                rb.angularVelocity = 360f;

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            stepDice = RandomStep();
            gridMovement.Move(GetRandomDirection(),stepDice);
            if (_currentDice != null)
            {
                _currentDice.GetComponent<Dice>().step = stepDice;
            }
            CanMove = true;
            StartCoroutine(BounceEffect(rb));
            _currentDice.GetComponent<Dice>().Invicable = false;
            _currentDice = null;
        }
    IEnumerator BounceEffect(Rigidbody2D rb)
    {
        if (rb != null)
        {
            float bounceHeight = 0.5f; // Độ cao nảy ban đầu
            float bounceDuration = 0.2f; // Thời gian mỗi lần nảy
            int bounceCount = 3; // Số lần nảy

            Vector3 originalPos = rb.transform.position;


            for (int i = 0; i < bounceCount; i++)
            {
                if (rb != null)
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
        }


    }

    void SpawnDice()
    {
        _currentDice = Instantiate(dicePrefab, spawnPosition.position, Quaternion.identity);
        _currentDice.GetComponent<Rigidbody2D>().gravityScale = 0;
        _currentDice.GetComponent<BoxCollider2D>().enabled = false;
    }

    public int RandomStep()
    {
        return UnityEngine.Random.Range(1, 6);
    }

    public void ChangeAnim(string animName)
    {
        if (_currentAnimName != animName)
        {
            anim.ResetTrigger(animName);
            _currentAnimName = animName;
            anim.SetTrigger(animName);
        }
    }
}
