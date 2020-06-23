using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TipsTimer : MonoBehaviour
{
    private Text txt;
    private float nextTime = 1;
    private float second = 7.0f;
    // Start is called before the first frame update
    void Start()
    {
        txt = this.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if(second>0)
        {
            second -= Time.deltaTime;
        }
        if(second<=0)
        {
            txt.text = "";
        }
    }
}
