using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeButton : MonoBehaviour
{
    // Start is called before the first frame update
    public Player player;
    public GirdMoveMent move;
    public Vector2 dir;
    private void OnMouseDown()
    {
        if (player.CanMove)
        {
            player.dir = dir;
            move.Move(player.dir, player.stepDice);
            player.CanMove = false; 
        }
    }
}
