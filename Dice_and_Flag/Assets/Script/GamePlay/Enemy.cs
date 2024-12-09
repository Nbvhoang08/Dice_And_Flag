using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using JetBrains.Annotations;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;



public class Enemy : Character
{
    // Start is called before the first frame update
    public GameObject dicePrefab; // Prefab xúc xắc
    public float throwForce = 10f; // Lực ném
    public float maxDistance = 5f; // Tầm ném tối đa
    public Transform spawnPosition; // Vị trí cố định để tạo xúc xắc
    public bool bringFlag;
    public GameObject _currentDice; // Viên xúc xắc hiện tại
    [SerializeField] private EnemyGridMoveMent gridMovement;
    public int stepDice = 0;
    public bool CanMove;
    [SerializeField] private Animator anim;
    public Enemy teamMember;
    public Vector3Int targetCell;
    public Vector3Int BaseCell ;
    public Vector3Int FlagCell;
    public Transform flag;
    private string _currentAnimName;
    public GameManager gameManager;
    public SpriteRenderer sprite;
    public String Name;
    public TextMeshPro NameText;


    private void Awake()
    {
        isYourTurn = false;
    }

    public override void Start()
    {
        base.Start();
        gameManager = FindAnyObjectByType<GameManager>();
        gridMovement = GetComponent<EnemyGridMoveMent>();
        anim = GetComponent<Animator>();
        CanMove = false;
    }
    void LateUpdate()
    {
        FlagCell = GetValidRoundedCell(flag.position);
    }
    void Update()
    {
     
        if (!Death)
        {
            if (isYourTurn)
            {
                if (!gridMovement.isMoving && !gridMovement.Stunning)
                {
                    StartCoroutine(EnemyTurn());
                    // Đảm bảo chỉ thực hiện một lần khi đến lượt
                    isYourTurn = false;
                }     
            }
            else
            {
                Vector2 position = new Vector2(transform.position.x, transform.position.y);
                Vector2 currentCell = new Vector2(gridMovement._currentCell.x, gridMovement._currentCell.y);
                if (Vector2.Distance(position, currentCell) >= 0.5)
                {
                    gridMovement.MoveReturn();
                }
            }
            if(!bringFlag)
            {
                targetCell = FlagCell;
            }
            else
            {
                targetCell = BaseCell;
            }
            
        }
        else
        {
            if (isYourTurn)
            {
                currentHp = maxHp;
                gameManager.Next();
            } 
            else
            {
                Dead();
            }
            bringFlag = false;
        }
        if (!gridMovement.isMoving)
        {
            NameText.text = Name;
        }

    }
    public void Dead()
    {

        transform.position = startPos.transform.position;
        gridMovement._currentCell = Vector3Int.RoundToInt(startPos.transform.position);
        if (Vector2.Distance(transform.position, startPos.transform.position) <= 0.1f)
        {
            ChangeAnim("stun");
        }
   


    }
    public Vector3Int GetValidRoundedCell(Vector3 position)
    {
        // Bước 1: Làm tròn tọa độ
        Vector3Int roundedCell = Vector3Int.RoundToInt(new Vector3(position.x, position.y, 0));

        // Bước 2: Kiểm tra ô làm tròn có hợp lệ không
        if (gridMovement.IsValidCell(roundedCell))
        {
            return roundedCell; // Nếu hợp lệ, trả về ngay
        }

        // Bước 3: Tìm ô hợp lệ gần nhất
        Vector3Int[] neighbors = new Vector3Int[]
        {
        roundedCell + Vector3Int.up,
        roundedCell + Vector3Int.down,
        roundedCell + Vector3Int.left,
        roundedCell + Vector3Int.right,
        roundedCell + new Vector3Int(1, 1, 0),
        roundedCell + new Vector3Int(-1, 1, 0),
        roundedCell + new Vector3Int(1, -1, 0),
        roundedCell + new Vector3Int(-1, -1, 0)
        };

        foreach (var neighbor in neighbors)
        {
            if (gridMovement.IsValidCell(neighbor))
            {
                return neighbor; // Trả về ô hợp lệ đầu tiên tìm thấy
            }
        }

        // Bước 4: Trường hợp không tìm thấy ô hợp lệ
        Debug.LogWarning($"No valid cell found near position {position}");
        return roundedCell; // Trả về giá trị làm tròn gốc (hoặc xử lý tùy trường hợp)
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
    
        // Nếu không có player nào trong phạm vi, chọn một vị trí ngẫu nhiên trong màn hình
        do
        {
            float randomAngle = UnityEngine.Random.Range(-30f,210f);
          
            float randomDistance = UnityEngine.Random.Range(2f, maxDistance);

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

            return spawnPosition.position;
    }


    void ThrowDice(Vector3 targetPoint)
    {
        Rigidbody2D rb = _currentDice.GetComponent<Rigidbody2D>();
        SoundManager.Instance.PlayVFXSound(0);
        ChangeAnim("throw");
        if (rb != null)
        {
            StartCoroutine(ThrowDiceCoroutine(rb, targetPoint));
            _currentDice.GetComponent<BoxCollider2D>().enabled = true;
        }
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
            gridMovement.Move(gridMovement.GetRandomDirection(targetCell),stepDice);
            //Debug.Log(gridMovement.GetRandomDirection(targetCell));   
           
            if (_currentDice != null)
            {
                _currentDice.GetComponent<Dice>().step = stepDice;
            }
            CanMove = true;
            StartCoroutine(BounceEffect(rb));
            
            if(_currentDice != null)
            {
                _currentDice.GetComponent<Dice>().Invicable = false;
                _currentDice = null;
            }
           
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
                        if(rb != null)
                        {
                            rb.MovePosition(Vector3.Lerp(startPos, bouncePos, t));
                        }
                       
                        elapsedTime += Time.deltaTime;
                        yield return null;
                    }

                    // Rơi xuống
                    elapsedTime = 0;
                    startPos = rb.transform.position;

                    while (elapsedTime < bounceDuration / 2)
                    {
                        float t = elapsedTime / (bounceDuration / 2);
                        if (rb != null)
                        {
                            rb.MovePosition(Vector3.Lerp(startPos, originalPos, t));
                        }
                        elapsedTime += Time.deltaTime;
                        yield return null;
                    }

                    // Giảm độ cao cho lần nảy tiếp theo
                    bounceHeight *= 0.5f;
                    bounceDuration *= 0.8f;
                    // Khi đến đích
                    if(rb != null)
                    {
                        rb.velocity = Vector2.zero;
                        rb.angularVelocity = 0;
                    }
                   
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
        return UnityEngine.Random.Range(1, 7);
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
