using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.EventSystems;

public class DisplayControllerLine : MonoBehaviourPun
{

    public GameObject leftController;
    public GameObject rightController;
    public GameObject controllerLeftLine;
    public GameObject controllerRightLine;
    public GameObject controllerLeftSphere;
    public GameObject controllerRightSphere;
    public GameObject test;
    private bool isAdded = false;
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

            if (!isAdded)
            {
                //leftController.AddComponent<KeyBoardControllerDemo>();
                //leftController.AddComponent<SteamVR_TrackedObjectDemo>();

                //rightController.AddComponent<KeyBoardControllerDemo>();
                //rightController.AddComponent<SteamVR_TrackedObjectDemo>();

                //leftController.GetComponent<SteamVR_TrackedObjectDemo>().index = SteamVR_TrackedObjectDemo.EIndex.Device1;
                //rightController.GetComponent<SteamVR_TrackedObjectDemo>().index = SteamVR_TrackedObjectDemo.EIndex.Device2;

                GameObject eventSystem = GameObject.Find("EventSystem");
                eventSystem.GetComponent<OVRInputModule>().rayTransform = rightController.transform;
                isAdded = true;
            }

            //Debug.Log(leftController.transform.forward);
            controllerLeftLine.GetComponent<LineRenderer>().SetPositions(new Vector3[]
            {
            leftController.transform.position,
            leftController.transform.forward+leftController.transform.position,
            });
            controllerRightLine.GetComponent<LineRenderer>().SetPositions(new Vector3[]
            {
            rightController.transform.position,
            rightController.transform.forward+rightController.transform.position,
            });

            controllerLeftSphere.transform.position = leftController.transform.forward + leftController.transform.position;
            controllerRightSphere.transform.position = rightController.transform.forward + rightController.transform.position;

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
