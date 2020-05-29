using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartDummyDirector : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        FadeManager.Instance.LoadScene("EmoteLogoCopy", 1.0f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
