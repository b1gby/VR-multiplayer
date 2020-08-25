using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimelineUIController : MonoBehaviour
{
    private GameObject snapShotsGameObject;
    private Camera UI3dCamera;
    private Transform snapDetail;

    private bool isClickSnapBtn = false;
    private bool is_displaySnapShotDetail = false;

    // Start is called before the first frame update
    void Start()
    {
        snapShotsGameObject = GameManager.snapShots;
        UI3dCamera = GameObject.Find("UI3dCamera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isClickSnapBtn && Input.GetMouseButtonUp(0))
        {
            float screenX = Input.mousePosition.x / Screen.width * 1024;
            float screenY = Input.mousePosition.y / Screen.height * 768;
            if (screenX <= 512 + 20 && screenX >= 512 - 20 && screenY <= 640 + 20 && screenY >= 640 - 20)
            {
                is_displaySnapShotDetail = true;
            }
        }
    }

   

    public void ClickSnapBtn(string strBtnName)
    {
        foreach (Transform child in snapShotsGameObject.transform)
        {
            child.localPosition = UI3dCamera.ScreenToWorldPoint(new Vector3(2000, 640, 5f));
        }
        snapDetail = null;
        GameObject snap = GameObject.Find(strBtnName.Replace("btn", ""));
        Vector3 screenPos = new Vector3(512, 640, 5f);
        snap.transform.localPosition = UI3dCamera.ScreenToWorldPoint(screenPos);
        snapDetail = snap.transform;
        Debug.Log("Detail1: " + snapDetail.name);
        isClickSnapBtn = true;
    }
}
