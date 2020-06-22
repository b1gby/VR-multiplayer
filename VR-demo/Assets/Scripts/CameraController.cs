using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;
    private float mouseX, mouseY;//获取鼠标移动的值
    public float mouseSensitivity;//获取鼠标移动速度

    public float yRotation;
   
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        
        yRotation -= mouseY;
        yRotation = Mathf.Clamp(yRotation, -50f, 50f); //限制相机俯仰高度（-50，50）
        player.Rotate(Vector3.up * mouseX);
        transform.localRotation = Quaternion.Euler(yRotation, 0, 0);
    }
}
