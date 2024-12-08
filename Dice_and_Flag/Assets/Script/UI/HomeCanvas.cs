using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeCanvas : UICanvas
{
    // Start is called before the first frame update
    public void PlayBtn()
    {
        SceneManager.LoadScene("GamePlay");
        //StartCoroutine(Play());
        UIManager.Instance.CloseUI<HomeCanvas>(0.3f);
        UIManager.Instance.OpenUI<GamePlayCanvas>();
        SoundManager.Instance.PlayClickSound();
    }

}
