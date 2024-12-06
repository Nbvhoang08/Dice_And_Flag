
using System;
using System.Collections;

using UnityEngine;
using TMPro;
public class Player : Character
{
    public GameObject dicePrefab; // Prefab xúc xắc
    public LineRenderer lineRenderer; // LineRenderer để vẽ quỹ đạo
    public float throwForce = 10f; // Lực ném
    public float maxDistance = 5f; // Tầm ném tối đa
    public int resolution = 50; // Số điểm trên quỹ đạo
    public Transform spawnPosition ; // Vị trí cố định để tạo xúc xắc

    [SerializeField] private GameObject _currentDice; // Viên xúc xắc hiện tại
    private bool _isDragging = false; // Để kiểm tra trạng thái kéo chuột

    [SerializeField]private GirdMoveMent gridMovement;
    public GameObject targetSpritePrefab;
    private GameObject targetSpriteInstance;
    public Vector2 dir = Vector2.zero;
    public int stepDice =0;
    public bool  CanMove;
    public SpriteRenderer sprite;

    [SerializeField] private Animator anim;
    public String currentAnimName;
    public GameManager gameManager;
    public String Name;
    public TextMeshPro NameText; 

    [SerializeField] private FakeButton[] btn;

    private void Awake()
    {
        isYourTurn = false;
    }
    private void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        gridMovement = GetComponent<GirdMoveMent>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        CanMove = false;
    }

    void Update()
    {
        
        if (isYourTurn)
        {
      
            if (Input.GetMouseButtonDown(0) && _currentDice == null && !gridMovement.isMoving && !CanMove)
            {
                SpawnDice();
               
                _isDragging = true;
                ChangeAnim("hold");
                // Tạo instance của targetSprite và điều chỉnh tọa độ Z
                if (targetSpriteInstance == null)
                {
                    targetSpriteInstance = Instantiate(targetSpritePrefab);
                    Vector3 position = targetSpriteInstance.transform.position;
                    position.z = -3;
                    targetSpriteInstance.transform.position = position;
                }
            }

            if (_isDragging && _currentDice != null)
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3 direction = mousePosition - spawnPosition.position;

                if (direction.magnitude > maxDistance)
                {
                    direction = direction.normalized * maxDistance;
                }

                // Cập nhật LineRenderer để theo dõi vị trí chuột
                DrawTrajectory(spawnPosition.position, spawnPosition.position + direction);

                if (Input.GetMouseButtonUp(0))
                {
                    // Lấy điểm cuối cùng từ LineRenderer
                    Vector3 targetPoint = lineRenderer.GetPosition(lineRenderer.positionCount - 1);
                    ThrowDice(targetPoint);
                    ChangeAnim("throw");
                    _isDragging = false;

                    // Hủy instance của targetSprite sau khi ném
                    if (targetSpriteInstance != null)
                    {
                        Destroy(targetSpriteInstance);
                        targetSpriteInstance = null;
                    }
                }
            }
        }
      

        if (CanMove)
        {
            NameText.text = "   ";
            foreach(Vector2 dir in gridMovement.GetValidDirections(gridMovement._currentCell))
            {
                for (int i = 0;i <= btn.Length-1; i++)
                {
                    if(dir == btn[i].dir)
                    {
                        btn[i].gameObject.SetActive(true);
                    }
                }
            }
        }
        else
        {
            if (gridMovement.isMoving)
            {
                NameText.text = stepDice.ToString();
            }
            else
            {
                NameText.text = Name;
            }
            
            for (int i = 0; i <= btn.Length-1; i++)
            {
                btn[i].gameObject.SetActive(false);
            }
        }
    }

    void DrawTrajectory(Vector2 start, Vector2 end)
    {
        lineRenderer.positionCount = resolution;
        Vector3[] points = new Vector3[resolution];

        Vector3 controlPoint = (start + end) / 2;
        controlPoint.y += Vector3.Distance(start, end) * 0.5f; // Độ cao của đường cong

        for (int i = 0; i < resolution; i++)
        {
            float t = i / (float)(resolution - 1);

            // Tính toán vị trí trên đường cong Bezier bậc 2
            Vector3 point1 = Vector3.Lerp(start, controlPoint, t);
            Vector3 point2 = Vector3.Lerp(controlPoint, end, t);
            points[i] = Vector3.Lerp(point1, point2, t);
        }

        lineRenderer.SetPositions(points);

        // Đặt sprite hình dấu X ở đầu target và điều chỉnh tọa độ Z
        if (targetSpriteInstance != null)
        {
            Vector3 endPosition = end;
            endPosition.z = -3;
            targetSpriteInstance.transform.position = endPosition;
            if(targetSpriteInstance.transform.position.x > transform.position.x)
            {
                sprite.flipX = true;
            }
            else
            {
                sprite.flipX = false;
            }
        }
    }

    void ThrowDice(Vector3 targetPoint)
    {
        Rigidbody2D rb = _currentDice.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            StartCoroutine(ThrowDiceCoroutine(rb, targetPoint));
            _currentDice.GetComponent<BoxCollider2D>().enabled = true;
        }

     
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

        while (elapsedTime < throwDuration && rb!= null)
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


        stepDice = RandomStep();
        yield return null;
        if (_currentDice!= null) 
        {
            _currentDice.GetComponent<Dice>().step = stepDice;
        }
        CanMove = true;
        _currentDice.GetComponent<Dice>().Invicable = false;

        StartCoroutine(BounceEffect(rb));
        _currentDice = null;
    }

    IEnumerator BounceEffect(Rigidbody2D rb)
    {
        if (rb != null) {
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
        lineRenderer.positionCount = 0; // Xóa quỹ đạo cũ
    }
    public int RandomStep()
    {
        return UnityEngine.Random.Range(1, 6);
    }

    public void ChangeAnim(string animName)
    {
        if (currentAnimName != animName)
        {
            anim.ResetTrigger(animName);
            currentAnimName = animName;
            anim.SetTrigger(animName);
        }
    }


}
