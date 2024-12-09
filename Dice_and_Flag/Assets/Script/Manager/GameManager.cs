using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Character[] character;
    public int currentIndex;
    // Start is called before the first frame update
    public bool HasWon;
    public bool GameOver;

    private void Awake()
    {
        currentIndex = 0;
    }
    void Start()
    {
        character[currentIndex].isYourTurn = true;
        HasWon = false;
        GameOver = false;
    }

    // Update is called once per frame
    private void Update()
    {
        CheckWonConditions();
        CheckLoseConditions();
       
    }
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
        for (int i = 0; i <= character.Length - 1; i++)
        {
            if (i != currentIndex)
            {
                character[i].isYourTurn = false;
            }
            else
            {
                character[i].isYourTurn = true;
            }
         
        }

    }

    public void CheckWonConditions()
    {
        if(HasWon && !GameOver) 
        {
            UIManager.Instance.OpenUI<WinCanvas>();
            HasWon = false;
        }
    }
    public void CheckLoseConditions()
    {
        if (!HasWon && GameOver)
        {
            UIManager.Instance.OpenUI<LoseCanvas>();
            GameOver = false;
        }
    }



}
