using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayCanvas : UICanvas
{
    // Start is called before the first frame update
    public void PauseBtn()
    {
        UIManager.Instance.OpenUI<PauseCanvas>();
        SoundManager.Instance.PlayClickSound();
        Time.timeScale = 0;
    }


}
