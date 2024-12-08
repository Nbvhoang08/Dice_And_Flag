using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
public class GamePlayCanvas : UICanvas
{
    // Start is called before the first frame update
    public GameManager gameManager;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    public List<HeartImageList> heartImageLists; // Danh sách các danh sách hình ảnh trái tim cho mỗi nhân vật
    public void PauseBtn()
    {
        UIManager.Instance.OpenUI<PauseCanvas>();
        SoundManager.Instance.PlayClickSound();
        Time.timeScale = 0;
    }
    private void OnEnable()
    {
        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
        }
    }


    private void Update()
    {
        if(gameManager == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
        }
        else
        {
            UpdateHeartImages();
        }

        
    }
    public void UpdateHeartImages()
    {
        for (int i = 0; i < gameManager.character.Length; i++)
        {
            int hp = gameManager.character[i].currentHp ;
            for (int j = 0; j < 3; j++)
            {
                if (j < hp)
                {
                    heartImageLists[i].heartImages[j].sprite = fullHeart; // Hiển thị trái tim
                }
                else
                {
                    heartImageLists[i].heartImages[j].sprite = emptyHeart; // Ẩn trái tim
                }
            }
        }
    }

}
[System.Serializable]
public class HeartImageList
{
    public List<Image> heartImages;
}