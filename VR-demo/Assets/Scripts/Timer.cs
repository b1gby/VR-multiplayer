using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public Text txt;
    public string displayTxt;
    public float second = 7.0f;
    private bool isTimerStart = false;
    // Start is called before the first frame update
    void Start()
    {
        txt = this.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isTimerStart)
        {
            txt.text = displayTxt;
            second -= Time.deltaTime;
            if (second <= 0)
            {
                txt.text = "";
                isTimerStart = false;
            }
        }
    }

    public void startTimer(float second)
    {
        this.second = second;
        isTimerStart = true;
    }
}
