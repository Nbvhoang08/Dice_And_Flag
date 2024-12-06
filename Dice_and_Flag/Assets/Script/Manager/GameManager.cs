using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Character[] character;
    public int currentIndex;
    // Start is called before the first frame update
    private void Awake()
    {
        currentIndex = 0;
    }
    void Start()
    {
        character[currentIndex].isYourTurn = true;
    }
  
    // Update is called once per frame

    public void Next()
    {
        if (currentIndex < character.Length-1)
        {
            currentIndex++;
        }
        else
        {
            currentIndex = 0;
        }
        
        for(int i = 0; i <=  character.Length-1; i++)
        {
            if(i != currentIndex)
            {
                character[i].isYourTurn = false;
            }
            else
            {
                character[i].isYourTurn = true;
            }
            
        }

    }





}
