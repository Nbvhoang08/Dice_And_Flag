using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class Dice : MonoBehaviour
{
    // Start is called before the first frame update
    public SpriteRenderer scoreSprite;
    public Sprite[] ScoreNumber;
    public  SpriteRenderer diceSprite;
    public Sprite[] diceDurability;
    public int durability;
    public int step;
    Rigidbody2D rb;
    public GameObject[] fragmentPrefabs; // Mảng chứa các prefab của mảnh vỡ
    public int fragmentCount = 5; // Số lượng mảnh vỡ
    public float explosionForce = 5f; // Lực tung mảnh vỡ
    public float explosionRadius = 2f; // Bán kính tung mảnh vỡ
    public bool Invicable;
    // Update is called once per frame
    private void Start()
    {
        diceSprite = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        Invicable = true;
    }
    void Update()
    {
        if ((IsRotating()))
        {
            scoreSprite.sprite = null;
            diceSprite.sprite = diceDurability[3];
           
        }
        else
        {
            switch (durability)
            {
                case <= 0:
                    Despawn();
                    break;
                case 1:
                    diceSprite.sprite = diceDurability[0];
                    break;
                case 2:
                    diceSprite.sprite = diceDurability[1];
                    break;
                case >= 3:

                    diceSprite.sprite = diceDurability[2];
                    break;
            }
            switch (step)
            {
                case 1:
                    scoreSprite.sprite = ScoreNumber[0];
                    break;
                case 2:
                    scoreSprite.sprite = ScoreNumber[1];
                    break;
                case 3:
                    scoreSprite.sprite = ScoreNumber[2];
                    break;
                case 4:
                    scoreSprite.sprite = ScoreNumber[3];
                    break;
                case 5:
                    scoreSprite.sprite = ScoreNumber[4];
                    break;
                case 6:
                    scoreSprite.sprite = ScoreNumber[5];
                    break;
            }
            
        }

    }
    void Despawn()
    {
        // Tung các mảnh vỡ từ mỗi prefab
        foreach (GameObject fragmentPrefab in fragmentPrefabs)
        {
            GameObject fragment = Instantiate(fragmentPrefab, transform.position, Quaternion.identity);
            Rigidbody2D rb = fragment.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Tạo hướng ngẫu nhiên trong bán kính explosionRadius
                Vector2 randomDirection = Random.insideUnitCircle.normalized;
                rb.AddForce(randomDirection * explosionForce, ForceMode2D.Impulse);
            }
            Destroy(fragment, 2f); // Hủy mảnh vỡ sau 2 giây
        }

        // Hủy đối tượng dice
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Dice"))
        {
            if (!Invicable)
            {
                durability--;
            }
           
        }
    }

    public bool IsRotating()
    {
        return Mathf.Abs(rb.angularVelocity) > 0.5f;
    }



}
