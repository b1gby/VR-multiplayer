using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DisplayControllerLine : MonoBehaviourPun
{

    public GameObject leftController;
    public GameObject rightController;
    public GameObject controllerLeftLine;
    public GameObject controllerRightLine;
    public GameObject test;
    // Start is called before the first frame update
    void Start()
    {
        //// avoid controlling player not mine
        //if ((!photonView.IsMine && PhotonNetwork.IsConnected))
        //{
        //    return;
        //}

        controllerLeftLine = this.transform.Find("controller_left_line").gameObject;
        controllerRightLine = this.transform.Find("controller_right_line").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        //// avoid controlling player not mine
        //if ((!photonView.IsMine && PhotonNetwork.IsConnected))
        //{
        //    return;
        //}
        try
        {
            leftController = this.transform.Find("controller_left").gameObject;
            rightController = this.transform.Find("controller_right").gameObject;
            //Debug.Log(leftController.transform.forward);
            controllerLeftLine.GetComponent<LineRenderer>().SetPositions(new Vector3[]
            {
            leftController.transform.position,
            leftController.transform.forward*2+leftController.transform.position,
            });
            controllerRightLine.GetComponent<LineRenderer>().SetPositions(new Vector3[]
            {
            rightController.transform.position,
            rightController.transform.forward*2+rightController.transform.position,
            });
        }
        catch
        {
            controllerLeftLine.GetComponent<LineRenderer>().SetPositions(new Vector3[]
            {
            Vector3.zero,
            Vector3.zero,
            });
            controllerRightLine.GetComponent<LineRenderer>().SetPositions(new Vector3[]
            {
            Vector3.zero,
            Vector3.zero,
            });
        }
        
    }
}
