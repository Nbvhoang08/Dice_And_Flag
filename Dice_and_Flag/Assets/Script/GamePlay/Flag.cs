using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Flag : MonoBehaviour
{
    // Start is called before the first frame update
    public FlagType type;
    private GameObject player;
    private bool followPlayer = false;
    public GameObject enemy;
    public bool followEnemy = false;
    public bool isDone;
    public GameManager gameManager;
    void Start()
    {
       isDone = false;
    }

    void Update()
    {
        switch (type)
        {
            case FlagType.Enemy:
                FollowPlayer(); 
                 break;
            case FlagType.Player:
                FollowEnemy();
                break;
        }

        
    }
    void FollowPlayer()
    {
        if (followPlayer && player != null)
        {
            // Di chuyển cờ để theo dõi người chơi
            Vector3 FollowTranfom = new Vector3(player.transform.position.x, player.transform.position.y, 0);
            transform.position = FollowTranfom;
            if (player.GetComponent<Player>().Death )
            {
                followPlayer = false;
                player = null;
            }
        }
    }
    void FollowEnemy()
    {
        if (followEnemy && enemy != null)
        {
            if (enemy.GetComponent<Enemy>().bringFlag)
            {
                Vector3 FollowTranfom = new Vector3(enemy.transform.position.x, enemy.transform.position.y, 0);
                transform.position = enemy.transform.position;
            }
            // Di chuyển cờ để theo dõi người chơi
            if (enemy.GetComponent<Enemy>().Death)
            {
                followEnemy = false;
                enemy = null;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (type == FlagType.Enemy && collision.CompareTag("Player") && !isDone)
        {
            followPlayer = true;
            player = collision.gameObject;
        }
        else if (type == FlagType.Enemy && collision.CompareTag("PlayerGoal") && player!=null && !isDone)
        {
            if(Vector2.Distance(collision.transform.position,player.GetComponent<Player>().startPos.transform.position) <= 0.3f)
            {
                followPlayer = false;
                player = null;
                transform.position = collision.transform.position;
                isDone = true;
                gameManager.HasWon = true;
            }
        }


        if (type == FlagType.Player && collision.CompareTag("Enemy") && !isDone && !followEnemy)
        {
            followEnemy = true;
            enemy = collision.gameObject;
            enemy.GetComponent<Enemy>().bringFlag = true;

        }
        else if (type == FlagType.Player && collision.CompareTag("EnemyGoal") && enemy != null && !isDone)
        {
            if (Vector2.Distance(collision.transform.position, enemy.GetComponent<Enemy>().startPos.transform.position) <= 0.3f) 
            {
                followEnemy = false;
                enemy = null;
                
                transform.position = collision.transform.position;
                isDone = true;
                gameManager.GameOver = true;
            }
        }

    }
    
}
public enum FlagType
{
    Enemy,
    Player
}
