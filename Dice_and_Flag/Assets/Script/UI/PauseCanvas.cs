using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseCanvas : UICanvas
{
    public Text VolumeText;
  
    void Start()
    {

        UpdateButtonImage();

    }
    public void Resume()
    {
        Time.timeScale = 1;
        UIManager.Instance.CloseUI<PauseCanvas>(0.2f);

        SoundManager.Instance.PlayClickSound();

    }

    public void HomeBtn()
    {
        UIManager.Instance.CloseAll();
        Time.timeScale = 1;
        SceneManager.LoadScene("Home");
        SoundManager.Instance.PlayClickSound();
        UIManager.Instance.OpenUI<HomeCanvas>();

    }
    public void SoundBtn()
    {
        SoundManager.Instance.TurnOn = !SoundManager.Instance.TurnOn;
        UpdateButtonImage();
        SoundManager.Instance.PlayClickSound();

    }

    private void UpdateButtonImage()
    {
        if (SoundManager.Instance.TurnOn)
        {
            VolumeText.text = "Music: On";
        }
        else
        {
            VolumeText.text = "Music: Off";
        }
    }
}
