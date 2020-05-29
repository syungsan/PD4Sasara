using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleDirector : MonoBehaviour
{
    [HeaderAttribute("TextLabel")]

    public Text appNameText;
    public Text versionNoText;

    // Start is called before the first frame update
    void Start()
    {
        // （仮）
        this.appNameText.text = "ささら日和";
        this.versionNoText.text = "Ver0.8";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClickStartButton()
    {
        // SceneManager.LoadScene("GameScene");
        FadeManager.Instance.LoadScene("GameScene", 1.0f);
    }
}
