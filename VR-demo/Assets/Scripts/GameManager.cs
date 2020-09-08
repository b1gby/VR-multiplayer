using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    static int escCount = 0;

    public GameObject cube;
    
    public GameObject lockObjectImgTip;
    public GameObject escMenu;
    public Button btnResume;
    public Button btnExit;
    public bool is_clickEscMenu = false;
    static public GameObject snapShots;
    static public GameObject TimelineUI;
    static public GameObject UI3dRawImage;
    static public GameObject btnBackTrack;
    static public GameObject btnShare;
    static public Camera UI3dCamera;

    static public float startTime;
    // Start is called before the first frame update
    void Start()
    {
        startTime = Time.time;

        lockObjectImgTip.transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(Screen.width / 2 - 180, -Screen.height / 2 + 130);

        UI3dCamera = GameObject.Find("UI3dCamera").GetComponent<Camera>();

        UI3dRawImage = GameObject.Find("UI3dRawImage");

        snapShots = GameObject.Find("SnapShots");
        snapShots.SetActive(true);

        TimelineUI = GameObject.Find("TimelineUI");

        btnBackTrack = GameObject.Find("BtnBackTrack");
        btnBackTrack.transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(Screen.width / 2-80, -Screen.height / 2+25);

        //btnShare = GameObject.Find("BtnShare");
        //btnShare.transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(-Screen.width / 2+80, -Screen.height / 2+25);

        UI3dRawImage.SetActive(false);
        TimelineUI.SetActive(false);
        UI3dRawImage.transform.parent.GetChild(5).gameObject.SetActive(false);
        btnBackTrack.SetActive(false);
        //btnShare.SetActive(false);
        GameObject[] addOnlyOneTags = GameObject.FindGameObjectsWithTag("Onlyone");
        foreach(GameObject per_object in addOnlyOneTags)
        {
            per_object.AddComponent<LockDispatcher>();
        }

        Cursor.lockState = CursorLockMode.Locked;//锁定指针到视图中心
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(UI3dCamera.WorldToScreenPoint(cube.transform.position));
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            escCount++;
        }
        if (escCount % 2 == 0 && !TimelineUI.activeSelf)
        {
            Cursor.lockState = CursorLockMode.Locked;//锁定指针到视图中心
            Cursor.visible = false;

            escMenu.SetActive(false);
            is_clickEscMenu = false;
        }
        else if(TimelineUI)
        {
            Cursor.lockState = CursorLockMode.None;//不锁定指针
            Cursor.visible = true;
        }
        else
        {

            Cursor.lockState = CursorLockMode.None;//不锁定指针
            Cursor.visible = true;

            escMenu.SetActive(true);
            is_clickEscMenu = true;
        }
    }

    public void ClickExitButton()
    {
        Application.Quit();
    }

    public void ClickResumeButton()
    {
        escCount++;
    }
}
