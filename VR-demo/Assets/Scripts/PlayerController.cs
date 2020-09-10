using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using System;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;
using Oculus.Platform;
using Oculus.Platform.Models;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviourPun, IPunObservable
{
    public void Initialize(GameObject character)
    {
        m_rigidBody = character.GetComponent<Rigidbody>();
    }

    
    [SerializeField] private float m_moveSpeed = 2.0f;
    [SerializeField] private float m_jumpForce = 4;
    
    [SerializeField] private Rigidbody m_rigidBody;


    private float m_currentV = 0;
    private float m_currentH = 0;

    private readonly float m_interpolation = 10;
    private readonly float m_runScale = 2.0f;
    private bool isMoveKeyBoard;
    private bool m_wasGrounded;
    private Vector3 m_currentDirection = Vector3.zero;

    private float m_jumpTimeStamp = 0;
    private float m_minJumpInterval = 0.25f;

    private bool m_isGrounded;
    
    private List<Collider> m_collisions = new List<Collider>();

    public Transform cameraTransform;
    private Transform targetTransform;
    private bool isDragObjectWithoutLock = false;
    private bool isDragObjectWithLock = false;
    
    private Transform recvTransform = null;
    private Transform recvTransformTmp = null;

    private float m_r = 0;
    private float m_g = 0;
    private float m_b = 0;

    private string m_viewID;

    private Text hasLockTip;
    private Text isSelectedTip;

    private bool isLockStatusChanged = false; // 两种方案：一是锁的状态改变就为真，之后把数据同步后置为假；二是上锁为真，解锁为假，但要设置延时，让他延时一会解除数据同步
    private bool isSleepToUnlock = false;
    private float time_sleepToUnlock = 0.2f;

    private int m_pressGNum = 0;
    private List<List<GameObject>> m_snapshotsList;
    public GameObject snapShotsGameObject;
    private bool is_takingSnapShots = false;
    public GameObject TimelineUI;
    public GameObject UI3dRawImage;
    private bool is_displaySnapShotDetail = false;
    private Transform snapDetail = null;
    private Vector3 oldSnapDetailPosition = Vector3.zero;
    private int click_snapIndex = -1;
    private GameObject btnBackTrack;
    private Camera UI3dCamera;

    //private GameObject btnShare;
    private bool isShareToOther = false;

    private Text detailTitleTxt;

    private float startTime;
    private float createDeltaTime;
    private GameObject newCreateSnap;
    private bool isClickSnapBtn = false;

    private TimelineUIController timelineUIController;
    private float timelineSize = 60;

    private List<Transform> targetTransformList;
    private List<Vector3> snapMovePosList;
    private bool isMoveSnapList = false;

    public GameObject leftController;
    public GameObject rightController;

    private bool isOnAir = false;

    private float[,] snapDisplayList = { {1024*2/8,768*3/5 }, { 1024 * 3 / 8, 768 * 4/ 5 }, 
        { 1024 * 4/ 8, 768 * 3 / 5 },{ 1024 * 5 / 8, 768 * 4 / 5 }, { 1024 * 6 / 8, 768 * 3/ 5 } ,
        { 1024 * 6 / 8, 768 * 2/ 5 },{ 1024 * 5 / 8, 768 * 1 / 5 },{ 1024 * 4 / 8, 768 * 2 / 5 },
        { 1024 * 3 / 8, 768 * 1/ 5 },{ 1024 * 2 / 8, 768 * 2 / 5 }
    };
    private bool isMoveGO;
    private bool isMoveDownGO;
    private List<GameObject> outlineTargets;

    public GameObject KeyBoard;
    private bool isDragKeyBoard;
    private bool isAdjustCamera;
    private float time_adjustCamera = 2.2f;
    private bool isReadyChangeColor = false;
    private float time_readyChangeGOColor = 0.5f;

    void Awake()
    {
        if(!m_rigidBody) { gameObject.GetComponent<Rigidbody>(); }
    }


    void Start()
    {
        isAdjustCamera = true;
        
        startTime = GameManager.startTime;
        m_viewID = this.photonView.ViewID.ToString();
        m_r = UnityEngine.Random.value * 256;
        m_g = UnityEngine.Random.value * 256;
        m_b = UnityEngine.Random.value * 256;
        CameraController _cameraController = this.gameObject.GetComponent<CameraController>();
        //cameraTransform = Camera.main.transform;
        if(photonView.IsMine)
        {
            //cameraTransform = this.transform.Find("OVRCameraRig").transform;
            GameObject UIforSnapshot = GameObject.Find("UIforSnapshot");
            UIforSnapshot.GetComponent<Canvas>().worldCamera = cameraTransform.GetChild(0).GetChild(1).GetComponent<Camera>();
            //GameObject InputFieldCanvas = GameObject.Find("InputFieldCanvas");
            //InputFieldCanvas.GetComponent<Canvas>().worldCamera = cameraTransform.GetChild(0).GetChild(1).GetComponent<Camera>();
            GameObject PredictionCanvas = GameObject.Find("PredictionCanvas");
            PredictionCanvas.GetComponent<Canvas>().worldCamera = cameraTransform.GetChild(0).GetChild(1).GetComponent<Camera>();
        }
        

        hasLockTip = GameObject.Find("HasLockTip").transform.GetComponent<Text>();
        isSelectedTip = GameObject.Find("IsSelectedTip").transform.GetComponent<Text>();

        m_snapshotsList = new List<List<GameObject>>();
        snapShotsGameObject = GameManager.snapShots;
        
        TimelineUI = GameManager.TimelineUI;

        btnBackTrack = GameManager.btnBackTrack;
        btnBackTrack.GetComponent<Button>().onClick.AddListener(ClickBackTrackBtn);

        //btnShare = GameManager.btnShare;
        //btnShare.GetComponent<Button>().onClick.AddListener(ClickShareBtn);

        UI3dCamera = GameManager.UI3dCamera;
        UI3dRawImage = GameManager.UI3dRawImage;

        detailTitleTxt = GameObject.Find("DetailTitleTxt").GetComponent<Text>();

        timelineUIController = TimelineUI.GetComponent<TimelineUIController>();

        targetTransformList = new List<Transform>();
        snapMovePosList = new List<Vector3>();

        outlineTargets = new List<GameObject>();

        KeyBoard = GameObject.Find("KeyBoard");

        if (_cameraController != null)
        {
            if (photonView.IsMine)
            {
                _cameraController.SetFolling();
            }
        }
        else
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> CameraWork Component on playerPrefab.", this);
        }

    }


    private void OnCollisionEnter(Collision collision)
    {
        ContactPoint[] contactPoints = collision.contacts;
        for(int i = 0; i < contactPoints.Length; i++)
        {
            if (Vector3.Dot(contactPoints[i].normal, Vector3.up) > 0.5f)
            {
                if (!m_collisions.Contains(collision.collider)) {
                    m_collisions.Add(collision.collider);
                }
                m_isGrounded = true;
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        ContactPoint[] contactPoints = collision.contacts;
        bool validSurfaceNormal = false;
        for (int i = 0; i < contactPoints.Length; i++)
        {
            if (Vector3.Dot(contactPoints[i].normal, Vector3.up) > 0.5f)
            {
                validSurfaceNormal = true; break;
            }
        }

        if(validSurfaceNormal)
        {
            m_isGrounded = true;
            if (!m_collisions.Contains(collision.collider))
            {
                m_collisions.Add(collision.collider);
            }
        } else
        {
            if (m_collisions.Contains(collision.collider))
            {
                m_collisions.Remove(collision.collider);
            }
            if (m_collisions.Count == 0) { m_isGrounded = false; }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if(m_collisions.Contains(collision.collider))
        {
            m_collisions.Remove(collision.collider);
        }
        if (m_collisions.Count == 0) { m_isGrounded = false; }
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // something changed locally, and send it to other
        if (stream.IsWriting)
        {

            #region Send something can collaborate
            GameObject[] sendTargets = GameObject.FindGameObjectsWithTag(m_viewID);

            //if(targetTransform!=null)
            if (sendTargets.Length != 0 && isMoveGO)
            {
                stream.SendNext("syncSelectTarget:" + sendTargets.Length.ToString());
                foreach (GameObject per_sendTarget in sendTargets)
                {
                    Transform per_Transform = per_sendTarget.transform;
                    stream.SendNext(per_Transform.name);

                    stream.SendNext(m_viewID);

                    stream.SendNext(per_Transform.position);

                    stream.SendNext(per_Transform.rotation);

                    stream.SendNext(per_Transform.localScale);

                    stream.SendNext(ColorUtility.ToHtmlStringRGBA(
                        per_Transform.GetComponent<MeshRenderer>().materials[0].color));

                }

            }
            #endregion

            #region Send somthing can not collaborate ( should be locked )

            //这里不用isDragObjectWithLock进行判断的原因在于：
            // isLockStatusChanged 在置为false时设置了0.2s的延时，见SelectDownObject()函数中
            // 设置延时的目的，因为当放下带有锁的物体的时候，本地锁的状态还没来得及被置为false就发送同步给其他人，这里延时进行同步
            if(isLockStatusChanged)
            {
                //Debug.Log("send!");
                GameObject[] sendMsgWithLock = GameObject.FindGameObjectsWithTag("Onlyone");

                stream.SendNext("syncLock:" + sendMsgWithLock.Length.ToString());
                foreach (GameObject per_target in sendMsgWithLock)
                {
                    LockDispatcher dispatcher = per_target.GetComponent<LockDispatcher>();
                    stream.SendNext(dispatcher.hasLock);
                    //Debug.Log("send:" + dispatcher.hasLock);
                    stream.SendNext(per_target.transform.position);

                    stream.SendNext(per_target.transform.rotation);

                    stream.SendNext(per_target.transform.localScale);
                }

                
            }

            #endregion

            #region Share Snapshot to Others
            if(isShareToOther)
            {
                stream.SendNext("syncShareSnapshot:" + newCreateSnap.transform.childCount.ToString());
                stream.SendNext(newCreateSnap.name);
                stream.SendNext(createDeltaTime);
                stream.SendNext(m_viewID);
                foreach (Transform child in newCreateSnap.transform)
                {
                    if(child.name!="Sphere")
                    {
                        stream.SendNext(child.name);
                        stream.SendNext(child.localPosition);
                    }
                        
                }
                isShareToOther = false;
            }

            #endregion
        }

        // do something after recv other changes
        if (stream.IsReading)
        {
            

            string[] recvArray = { "GG"};
            try
            {
                string recvStr = (string)stream.ReceiveNext();
                //Debug.Log(recvStr);
                recvArray = recvStr.Split(new char[1] {':' });
            }
            catch(Exception e)
            {
                //nothing to do;
                //TODO:如果之后收到的是字符串并且有错误，这里可能会忽略.因为为了接收selectTarget调试方便，将错误忽略了，最后完成时要将其加上。
                // Debug.Log(e);
            }

            #region sync something can collaborate
            if (recvArray[0] == "syncSelectTarget")
            {
                int recvNum = int.Parse(recvArray[1]);
                for(int i = 0;i<recvNum;i++)
                {
                    string recvName = (string)stream.ReceiveNext();
                    string recvViewID = (string)stream.ReceiveNext();
                    Vector3 recvPosition = (Vector3)stream.ReceiveNext();
                    Quaternion recvRotation = (Quaternion)stream.ReceiveNext();
                    Vector3 recvScale = (Vector3)stream.ReceiveNext();

                    Color newColor;
                    ColorUtility.TryParseHtmlString("#" + (string)stream.ReceiveNext(), out newColor);

                    if(GameObject.Find(recvName)==null)
                    {
                        recvTransformTmp = GameObject.Find(recvName.Replace("(Clone)" + recvViewID, "")).transform;
                        recvTransform = Instantiate(recvTransformTmp.gameObject, recvTransformTmp.parent).transform;
                        
                        //recvTransform.SetPositionAndRotation(Vector3.zero, recvTransform.parent.rotation);
                        //recvTransform.localScale = Vector3.one;
                        recvTransform.name = recvName;
                        recvTransform.tag = recvViewID;
                        
                    }
                    else
                    {
                        recvTransform = GameObject.Find(recvName).transform;
                    }

                    recvTransform.position = recvPosition;

                    recvTransform.rotation = recvRotation;

                    recvTransform.localScale = recvScale;

                    //recvTransform.GetComponent<MeshRenderer>().materials = new Material[]
                    //{
                    //    Resources.Load("Materials/New_Chair_Material") as Material,
                    //};
                    newColor.a = 0.3f;
                    recvTransform.GetComponent<MeshRenderer>().materials[0].color = newColor;
                }
            }

            #endregion

            #region sync somthing can not collaborate
            if (recvArray[0] == "syncLock")
            {
                GameObject[] recvMsgWithLock = GameObject.FindGameObjectsWithTag("Onlyone");
                
                foreach (GameObject per_target in recvMsgWithLock)
                {
                    LockDispatcher dispatcher = per_target.GetComponent<LockDispatcher>();
                    dispatcher.hasLock = (bool)stream.ReceiveNext();
                    //Debug.Log("recv:" + dispatcher.hasLock);

                    if (dispatcher.hasLock)
                    {
                        per_target.GetComponent<MeshRenderer>().materials[0].color = new Color(1.0f, 1.0f, 0.0f);
                    }
                    else
                    {
                        per_target.GetComponent<MeshRenderer>().materials[0].color = new Color(1.0f, 1.0f, 1.0f);
                    }

                    Vector3 recvPosition = (Vector3)stream.ReceiveNext();
                    Quaternion recvRotation = (Quaternion)stream.ReceiveNext();
                    Vector3 recvScale = (Vector3)stream.ReceiveNext();

                    per_target.transform.position = recvPosition;

                    per_target.transform.rotation = recvRotation;

                    per_target.transform.localScale = recvScale;

                   
                }

            }
            #endregion

            #region sync snapshot need to share
            if (recvArray[0] == "syncShareSnapshot")
            {

                string recvName = (string)stream.ReceiveNext();
                float recvDeltaTime = (float)stream.ReceiveNext();
                string recvViewID = (string)stream.ReceiveNext();

                GameObject tmp = new GameObject(recvName);
                //tmp.name += recvViewID;
                tmp.transform.SetParent(snapShotsGameObject.transform);
                tmp.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                Vector3 screenPos = new Vector3(2000, 640, 5f);
                tmp.transform.localPosition = UI3dCamera.ScreenToWorldPoint(screenPos);

                int recvNum = int.Parse(recvArray[1]);
                for (int i = 0; i < recvNum-1; i++)
                {
                    string recvChildName = (string)stream.ReceiveNext();
                    Vector3 recvLocalPos = (Vector3)stream.ReceiveNext();
                    GameObject childGo = Instantiate(GameObject.Find(recvChildName), tmp.transform);
                    childGo.transform.localPosition = recvLocalPos;
                }

                foreach (Transform child in tmp.transform)
                {
                    child.name = child.name.Substring(0, child.name.Length - 7);
                }

                // Sphere
                GameObject tmpSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                tmpSphere.transform.SetParent(tmp.transform);
                tmpSphere.transform.localPosition = Vector3.zero;
                tmpSphere.transform.localScale = Vector3.one * 3.8f;

                tmpSphere.GetComponent<MeshRenderer>().materials = new Material[]
                {
                Resources.Load("Materials/wall_copy") as Material,
                };


                GameObject Button = new GameObject("Button", typeof(Button), typeof(RectTransform), typeof(Image), typeof(DeltaTime));
                Button.transform.SetParent(TimelineUI.transform.GetChild(2));
                Button.name = tmp.name + "btn";
                Button.transform.localRotation = Quaternion.identity;
                Button.transform.localScale = Vector3.one;
                Button.GetComponent<RectTransform>().sizeDelta = new Vector2(30, 30);
                Button.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(recvDeltaTime / timelineSize * 768 - 384, 128, 0);
                Button.GetComponent<DeltaTime>().CreateDeltaTime = recvDeltaTime;
                Button.GetComponent<Button>().onClick.AddListener(() =>ClickSnapBtn(Button.name));
                Button.GetComponent<Image>().color = new Color(1, 0, 0);
                //Button.GetComponent<Button>().targetGraphic = Button.GetComponent<Image>();
                //ColorBlock cb = new ColorBlock();
                //cb.normalColor = Color.white;
                //cb.highlightedColor = new Color(0f / 255f, 153f / 255f, 255f / 255f);
                //cb.pressedColor = new Color(155f / 255f, 138f / 255f, 255f / 255f);
                //cb.selectedColor = new Color(5f / 255f, 0f / 255f, 255f / 255f);
                //Button.GetComponent<Button>().colors = cb;

                while (recvDeltaTime > timelineSize)
                {
                    timelineSize += 60;
                    ResortTimeline();
                }
            }
            #endregion
        }
    }

    //#region PunRPC
    //[PunRPC]
    //public void moveObject()
    //{
    //    if(targetTransform!= null)
    //    {
            
    //    }
        
    //}
    //#endregion

    private void Update()
    {
        if(isDragObjectWithLock || isDragObjectWithoutLock || isMoveSnapList)
        {
            targetTransform.position = cameraTransform.position + cameraTransform.forward * 2f;
        }
        
        if(isAdjustCamera && photonView.IsMine)
        {
            time_adjustCamera -= Time.deltaTime;
            if(time_adjustCamera <=0)
            {
                cameraTransform.gameObject.SetActive(false);
                cameraTransform.gameObject.SetActive(true);
                isAdjustCamera = false;
            }
            
        }

        if (!isReadyChangeColor)
        {
            time_readyChangeGOColor -= Time.deltaTime;
            if (time_readyChangeGOColor <= 0)
            {
                isReadyChangeColor = true;
            }
        }

        if (isSleepToUnlock)
        {
            time_sleepToUnlock -= Time.deltaTime;
            if (time_sleepToUnlock <= 0)
            {
                isLockStatusChanged = false;
                isSleepToUnlock = false;
            }
        }

        if(snapShotsGameObject.activeSelf)
        {
            foreach(Transform snap in snapShotsGameObject.transform)
            {
                snap.RotateAround(snap.position, Vector3.up, 0.1f);
            }
        }

        //display snapshots and snapshots details
        DisplaySnapShots();


        if (isMoveSnapList && Input.GetKeyUp(KeyCode.F))
        {
            for (int i = 0, k = 0; i < targetTransform.childCount; i++)
            {
                targetTransform.GetChild(i).SetParent(targetTransform.parent);
                i--;
            }

            Destroy(targetTransform.gameObject);

            isMoveSnapList = false;
        }

        
    }

    void FixedUpdate ()
    {
        
        // avoid controlling player not mine
        if ((!photonView.IsMine && PhotonNetwork.IsConnected))
        {
            return;
        }


        


        // movement
        MovementController();


        // jump
        JumpingAndLanding();
        

        //// pickup
        //if (Input.GetKeyDown(KeyCode.E))
        //{
        //    //Debug.Log(isDragObject);
        //    if(!isDragObjectWithLock && !isDragObjectWithoutLock)
        //    {
        //        SelectObject();
        //    }
        //    else
        //    {
        //        SelectDownObject();
        //    }
        //}

        // record snapshot
        RecordSnapShots();


        // markGO
        if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) > 0.55f)
        {
            MarkGO();
        }

        // moveGO
        if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger) >= 0.99f)
        {
            isMoveGO = true;
        }
        else if(OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger) < 0.35f)
        {
            if(isMoveGO)
            {
                isMoveDownGO = true;
            }
            isMoveGO = false;
        }
        MoveGO();

        //move KeyBoard to eye
        if(IsFocusOnInputText() && !isMoveKeyBoard)
        {
            KeyBoard.transform.position = cameraTransform.position + cameraTransform.forward;
            isMoveKeyBoard = true;
        }
        if (OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger) >= 0.99f)
        {
            RaycastHit target;
            rightController = FindObjectOfType<DisplayControllerLine>().rightController;
            if (Physics.Raycast(rightController.transform.position, rightController.transform.forward, out target, 100f))
            {
                if(target.transform.tag=="Key")
                {
                    isDragKeyBoard = true;
                }
                //Debug.Log(targetTransform.name);
            }
        }
        if (OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger) <= 0.35f)
        {
            isDragKeyBoard = false;
        }
        if(isDragKeyBoard)
        {
            KeyBoard.transform.position = rightController.transform.position + rightController.transform.forward;
            KeyBoard.transform.rotation = rightController.transform.rotation;
        }

        m_wasGrounded = m_isGrounded;
         
    }



    private void SelectObject()
    {

        RaycastHit screenCenterTarget;

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out screenCenterTarget, 100f))
        {
            targetTransform = screenCenterTarget.transform;
            //Debug.Log(targetTransform.name);
        }

        // select other people is controlling
        int parseTag;
        if(int.TryParse(targetTransform.tag, out parseTag))
        {
            if(parseTag!=int.Parse(m_viewID) && parseTag>=1001 && parseTag<=10000)
            {
                isSelectedTip.GetComponent<Timer>().startTimer(3f);
            }
        }

        //这里if加上&&后面，是因为在别人选择物体移动时，只能选择基本的（Everyone），不能动其他人动过的
        if (targetTransform != null && (targetTransform.tag == "Everyone" || targetTransform.tag == m_viewID))
        {
            if(targetTransform.tag == m_viewID)
            {
                //nothing to do
            }
            else if (GameObject.Find(targetTransform.name + "(Clone)" + m_viewID) == null)
            {
                targetTransform = Instantiate(targetTransform.gameObject,targetTransform.parent).transform;
                //Material[] m = new Material[]
                //    {
                //        Resources.Load("Materials/New_Chair_Material") as Material,
                //    };

                //m[0].color = new Color(m_r/255,m_g/255,m_b/255);

                //targetTransform.SetPositionAndRotation(Vector3.zero, targetTransform.parent.rotation);
                //targetTransform.localScale = Vector3.one;
                targetTransform.name += m_viewID;
                targetTransform.GetComponent<MeshRenderer>().materials[0].color = new Color(m_r / 255, m_g / 255, m_b / 255);
                targetTransform.tag = m_viewID;
            }
            else
            {
                targetTransform = GameObject.Find(targetTransform.name + "(Clone)" + m_viewID).transform;
            }
            isDragObjectWithoutLock = true;
        }


        if(targetTransform.tag == "Onlyone")
        {
            LockDispatcher dispatcher = targetTransform.GetComponent<LockDispatcher>();
            if (dispatcher.hasLock)
            {
                hasLockTip.GetComponent<Timer>().startTimer(3f);
            }
            else
            {
                // find OnlyoneContainer
                Transform containerT = targetTransform.parent;
                while(containerT.tag!="OnlyoneContainer")
                {
                    containerT = containerT.parent;
                }
                
                
                // find child in OnlyoneContainer
                Transform[] allChild = containerT.GetComponentsInChildren<Transform>();
                foreach(Transform child in allChild)
                {
                    if(child.tag == "Onlyone")
                    {
                        LockDispatcher allDispatcher = child.GetComponent<LockDispatcher>();
                        allDispatcher.GetLock();
                    }
                }
                isLockStatusChanged = true;
                isDragObjectWithLock = true;
            }
        }
        
    }

    private void SelectDownObject()
    {
        if (targetTransform.tag == "Onlyone")
        {
            // find OnlyoneContainer
            Transform containerT = targetTransform.parent;
            while (containerT.tag != "OnlyoneContainer")
            {
                containerT = containerT.parent;
            }

            Debug.Log("T:" + containerT.name);
            // find child in OnlyoneContainer
            Transform[] allChild = containerT.GetComponentsInChildren<Transform>();
            foreach (Transform child in allChild)
            {
                if (child.tag == "Onlyone")
                {
                    LockDispatcher allDispatcher = child.GetComponent<LockDispatcher>();
                    allDispatcher.ReleaseLock();
                }
            }
            isSleepToUnlock = true;
            time_sleepToUnlock = 0.2f;
            isDragObjectWithLock = false;
            targetTransform = null;
        }
        else
        {
            isDragObjectWithoutLock = false;
            targetTransform = null;
        }
        
        
    }

    private void MovementController()
    {
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        //Transform camera = Camera.main.transform;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            v *= m_runScale;
            h *= m_runScale;
        }

        m_currentV = Mathf.Lerp(m_currentV, v, Time.deltaTime * m_interpolation);
        m_currentH = Mathf.Lerp(m_currentH, h, Time.deltaTime * m_interpolation);

        Vector3 direction;
        if(OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger) >= 0.99f)
        {
            direction = cameraTransform.up * m_currentV + cameraTransform.right * m_currentH;
            m_rigidBody.useGravity = false;
            m_rigidBody.isKinematic = true;
            isOnAir = true;
        }
        else
        {
            direction = cameraTransform.forward * m_currentV + cameraTransform.right * m_currentH;
            //direction.y = 0;
        }
        

        float directionLength = direction.magnitude;
        
        direction = direction.normalized * directionLength;

        if (direction != Vector3.zero)
        {
            m_currentDirection = Vector3.Slerp(m_currentDirection, direction, Time.deltaTime * m_interpolation);

            transform.position += m_currentDirection * m_moveSpeed * Time.deltaTime;
        }
    }

    private void JumpingAndLanding()
    {
        bool jumpCooldownOver = (Time.time - m_jumpTimeStamp) >= m_minJumpInterval;

        if (jumpCooldownOver && m_isGrounded && OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger)>0.55f)
        {
            m_jumpTimeStamp = Time.time;
            m_rigidBody.AddForce(Vector3.up * m_jumpForce, ForceMode.Impulse);
        }
    }

    private void MarkGO()
    {
        leftController = FindObjectOfType<DisplayControllerLine>().leftController;
        rightController = FindObjectOfType<DisplayControllerLine>().rightController;
        
        RaycastHit controllerTarget;

        if (Physics.Raycast(leftController.transform.position, leftController.transform.forward,
            out controllerTarget, 5f))
        {
            targetTransform = controllerTarget.transform;
        }

        if(targetTransform)
        {
            // select other people is controlling
            int parseTag;
            if (int.TryParse(targetTransform.tag, out parseTag))
            {
                if (parseTag != int.Parse(m_viewID) && parseTag >= 1001 && parseTag <= 10000)
                {
                    isSelectedTip.GetComponent<Timer>().startTimer(3f);
                }
            }

            if (targetTransform.tag == "Everyone" || targetTransform.tag == "Onlyone" || targetTransform.tag == m_viewID)
            {
                if (targetTransform.GetComponent<Grabbable>().CurColor == Color.white && isReadyChangeColor)
                {
                    isReadyChangeColor = false;
                    time_readyChangeGOColor = 0.5f;
                    targetTransform.GetComponent<Grabbable>().SetColor(FindObjectOfType<GrabManager>().OutlineColorHighlighted);
                    targetTransformList.Add(targetTransform);
                }
                else if(isReadyChangeColor)
                {
                    isReadyChangeColor = false;
                    time_readyChangeGOColor = 0.5f;
                    targetTransform.GetComponent<Grabbable>().ClearColor();
                    targetTransformList.Remove(targetTransform);
                }
            }
        }
        
            

    }

    private void MoveGO()
    {
        if(isMoveGO)
        {
            foreach(Transform tf in targetTransformList)
            {
                if (tf.tag == "Onlyone")
                {
                    LockDispatcher dispatcher = tf.GetComponent<LockDispatcher>();
                    if (dispatcher.hasLock)
                    {
                        hasLockTip.GetComponent<Timer>().startTimer(3f);
                    }
                    else
                    {
                        // find OnlyoneContainer
                        Transform containerT = tf.parent;
                        while (containerT.tag != "OnlyoneContainer")
                        {
                            containerT = containerT.parent;
                        }


                        // find child in OnlyoneContainer
                        Transform[] allChild = containerT.GetComponentsInChildren<Transform>();
                        foreach (Transform child in allChild)
                        {
                            if (child.tag == "Onlyone" && child !=tf)
                            {
                                LockDispatcher allDispatcher = child.GetComponent<LockDispatcher>();
                                allDispatcher.GetLock();
                            }
                        }
                        isLockStatusChanged = true;
                        tf.position = leftController.transform.position + leftController.transform.forward;
                    }
                }
                else if (tf.tag == m_viewID)
                {
                    tf.position = leftController.transform.position + leftController.transform.forward;
                }
                else if (GameObject.Find(tf.name + "(Clone)" + m_viewID) == null)
                {
                    Transform tmp = Instantiate(tf.gameObject, tf.parent).transform;
                    //Material[] m = new Material[]
                    //    {
                    //        Resources.Load("Materials/New_Chair_Material") as Material,
                    //    };

                    //m[0].color = new Color(m_r/255,m_g/255,m_b/255);

                    //targetTransform.SetPositionAndRotation(Vector3.zero, targetTransform.parent.rotation);
                    //targetTransform.localScale = Vector3.one;
                    tmp.name += m_viewID;
                    tmp.GetComponent<MeshRenderer>().materials[0].color = new Color(m_r / 255, m_g / 255, m_b / 255);
                    tmp.tag = m_viewID;
                    tmp.position = leftController.transform.position + leftController.transform.forward;
                }
                else
                {
                    Transform tmp = GameObject.Find(tf.name + "(Clone)" + m_viewID).transform;
                    tmp.position = leftController.transform.position + leftController.transform.forward;
                }
                
            }
        }
        else if(isMoveDownGO)
        {
            foreach (Transform transform in targetTransformList)
            {
                if (targetTransform.tag == "Onlyone")
                {
                    // find OnlyoneContainer
                    Transform containerT = targetTransform.parent;
                    while (containerT.tag != "OnlyoneContainer")
                    {
                        containerT = containerT.parent;
                    }

                    //Debug.Log("T:" + containerT.name);
                    // find child in OnlyoneContainer
                    Transform[] allChild = containerT.GetComponentsInChildren<Transform>();
                    foreach (Transform child in allChild)
                    {
                        if (child.tag == "Onlyone")
                        {
                            LockDispatcher allDispatcher = child.GetComponent<LockDispatcher>();
                            allDispatcher.ReleaseLock();
                        }
                    }
                    isSleepToUnlock = true;
                    time_sleepToUnlock = 0.2f;
                    
                }
                transform.GetComponent<Grabbable>().ClearColor();
            }
            targetTransformList.Clear();
            isMoveDownGO = false;
        }
    }

    private void RecordSnapShots()
    {
        if (OVRInput.GetDown(OVRInput.RawButton.B) && !is_takingSnapShots)
        {
            leftController = FindObjectOfType<DisplayControllerLine>().leftController;
            rightController = FindObjectOfType<DisplayControllerLine>().rightController;

            GameObject eventSystem = GameObject.Find("EventSystem");
            eventSystem.GetComponent<OVRInputModule>().rayTransform = rightController.transform;

            RaycastHit target;
            //List<GameObject> outlineTargets = Camera.main.transform.GetComponent<DrawOutline>().targets;

            if (Physics.Raycast(rightController.transform.position, rightController.transform.forward,
            out target, 5f))
            {
                if (target.transform)
                {
                    if (target.transform.parent.tag == "EveryoneContainer")
                    {
                        foreach (Transform child in target.transform.parent)
                        {
                            if (child.tag != m_viewID.ToString() && child.tag != "Everyone")
                            {
                                continue;
                            }
                            child.GetComponent<Grabbable>().SetColor(FindObjectOfType<GrabManager>().OutlineColorHighlighted);
                            outlineTargets.Add(child.gameObject);
                            if (child.transform.tag == m_viewID)
                            {
                                outlineTargets.Remove(GameObject.Find(child.name.Replace("(Clone)" + m_viewID, "")));
                            }
                            //if (child.childCount != 0)
                            //{
                            //    bool hasChildNotMine = false;
                            //    foreach (Transform childOfChild in child.transform)
                            //    {
                            //        if(childOfChild.tag==m_viewID)
                            //        {
                            //            outlineTargets.Add(childOfChild.gameObject);
                            //            hasChildNotMine = true;
                            //        }
                            //    }
                            //    if(!hasChildNotMine)
                            //        outlineTargets.Add(child.gameObject);
                            //}
                            //else
                            //{
                            //    outlineTargets.Add(child.gameObject);
                            //}
                        }
                    }
                    targetTransform = target.transform;

                    is_takingSnapShots = true;
                    //Debug.Log(targetTransform.name);
                }

            }
        }

        if (OVRInput.GetUp(OVRInput.RawButton.B) && is_takingSnapShots)
        {
            DateTime nowTime = DateTime.Now.ToLocalTime();
            newCreateSnap = new GameObject(nowTime.ToString("yyyy-MM-dd HH:mm:ss"));
            newCreateSnap.transform.SetParent(snapShotsGameObject.transform);
            int n = snapShotsGameObject.transform.childCount;

            newCreateSnap.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            Vector3 screenPos = new Vector3(2000, 640 ,5f);
            newCreateSnap.transform.localPosition = UI3dCamera.ScreenToWorldPoint(screenPos);

            foreach (GameObject target in outlineTargets)
            {
                GameObject.Instantiate(target, newCreateSnap.transform);
            }
            foreach (Transform child in newCreateSnap.transform)
            {
                child.name = child.name.Substring(0, child.name.Length - 7);
            }
            m_snapshotsList.Add(outlineTargets);

            // Sphere
            GameObject tmpSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tmpSphere.transform.SetParent(newCreateSnap.transform);
            tmpSphere.transform.localPosition = Vector3.zero;
            tmpSphere.transform.localScale = Vector3.one * 3.8f;

            tmpSphere.GetComponent<MeshRenderer>().materials = new Material[]
            {
                Resources.Load("Materials/wall") as Material,
            };

            createDeltaTime = Time.time - startTime;
            GameObject button = new GameObject("Button", typeof(Button), typeof(RectTransform), typeof(Image), typeof(DeltaTime));
            button.transform.SetParent(TimelineUI.transform.GetChild(2));
            button.name = newCreateSnap.name + "btn";
            button.transform.localRotation = Quaternion.identity;
            button.transform.localScale = Vector3.one;
            button.GetComponent<RectTransform>().sizeDelta = new Vector2(30, 30);
            button.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(createDeltaTime / timelineSize * 768-384,128,0);
            button.GetComponent<DeltaTime>().CreateDeltaTime = createDeltaTime;
            button.GetComponent<Button>().onClick.AddListener(()=>ClickSnapBtn(button.name));
            //button.GetComponent<Button>().targetGraphic = button.GetComponent<Image>();
            //ColorBlock cb = new ColorBlock();
            //cb.normalColor = Color.white;
            //cb.highlightedColor = new Color(0f / 255f, 153f / 255f, 255f / 255f);
            //cb.pressedColor = new Color(155f / 255f, 138f / 255f, 255f / 255f);
            //cb.selectedColor = new Color(5f / 255f, 0f / 255f, 255f / 255f);
            //button.GetComponent<Button>().colors = cb;

            while (createDeltaTime > timelineSize)
            {
                timelineSize += 60;
                ResortTimeline();
            }
            foreach(GameObject child in outlineTargets)
            {
                child.GetComponent<Grabbable>().ClearColor();
            }
            isShareToOther = true;
            is_takingSnapShots = false;
        }
    }

    private void DisplaySnapShots()
    {
        if (Input.GetKeyDown(KeyCode.Tab) || OVRInput.GetDown(OVRInput.Button.Four))
        {
            m_pressGNum++;
            Debug.Log(snapShotsGameObject.activeSelf);
            if (m_pressGNum%2!=0)
            {
                //snapShotsGameObject.SetActive(true);
                TimelineUI.SetActive(true);
                UI3dRawImage.SetActive(true);
                UI3dRawImage.transform.parent.GetChild(5).gameObject.SetActive(true);
                Cursor.lockState = CursorLockMode.None;//锁定指针到视图中心
                Cursor.visible = true;
            }
            else
            {
                //snapShotsGameObject.SetActive(false);
                TimelineUI.SetActive(false);
                UI3dRawImage.SetActive(false);
                UI3dRawImage.transform.parent.GetChild(5).gameObject.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;//锁定指针到视图中心
                Cursor.visible = false;
            }
        }

        if (isClickSnapBtn&& OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0.55f)
        {
            //float screenX = Input.mousePosition.x / Screen.width * 1024;
            //float screenY = Input.mousePosition.y / Screen.height * 768;
            //if(screenX<=512+20&&screenX>=512-20&&screenY<=640+20&&screenY>=640-20)
            //{
                is_displaySnapShotDetail = true;
                isClickSnapBtn = false;
            //}
        }

        //if (snapShotsGameObject.activeSelf)
        //{
        //    btnBackTrack.SetActive(false);
        //    btnShare.SetActive(false);


        //    if (Input.GetMouseButtonUp(0))
        //    {

        //        float screenX = Input.mousePosition.x / Screen.width*1024;
        //        float screenY = Input.mousePosition.y / Screen.height * 768;
        //        //Debug.Log("s:" + screenX);
        //        //Debug.Log("s:"+screenY);
        //        //Debug.Log("d:"+snapDisplayList[0, 0]);
        //        //Debug.Log("d:"+snapDisplayList[0, 1]);
        //        for (int i=0;i<snapDisplayList.GetLength(0);i++)
        //        {
        //            //Debug.Log(snapDisplayList[i, 0]);
        //            //Debug.Log(snapDisplayList[i, 1]);
        //            if(screenX<=snapDisplayList[i,0]+20 && screenX >= snapDisplayList[i, 0] - 20
        //                && screenY <= snapDisplayList[i, 1] + 20 && screenY >= snapDisplayList[i, 1] - 20)
        //            {
        //                snapDetail = snapShotsGameObject.transform.GetChild(i).transform;

        //                //保存展示细节前的snapshot位置
        //                //用index而不直接用position表示，是因为在本地调试多开窗口时，点一下，会进入n次这里（n:多开的窗口数）
        //                //oldSnapDetailPosition = snapDetail.localPosition;
        //                click_snapIndex = i;

        //                snapDetail.localPosition = new Vector3(10000, 0, 5);
        //                snapDetail.localScale = Vector3.one * 1.5f;
        //                is_displaySnapShotDetail = true;

        //                detailTitleTxt.text = snapDetail.name;
        //                if(snapDetail.GetChild(4).GetComponent<MeshRenderer>().materials[0].name == "wall (Instance)")
        //                {
        //                    detailTitleTxt.text += " from me";
        //                }
        //                else
        //                {
        //                    detailTitleTxt.text += " from others";
        //                }
        //                break;
        //            }
        //        }
        //        if (is_displaySnapShotDetail)
        //        {
        //            foreach (Transform child in snapShotsGameObject.transform)
        //            {

        //                if (child != snapDetail)
        //                {
        //                    child.gameObject.SetActive(false);
        //                }
        //            }
        //        }
        //    }
        //}

        if (is_displaySnapShotDetail)
        {

            Debug.Log("Detail2: " + snapDetail.name);
            btnBackTrack.SetActive(true);
            //btnShare.SetActive(true);
            snapDetail.localPosition = new Vector3(10000, 0, 5);
            snapDetail.localScale = Vector3.one * 1.5f;
            detailTitleTxt.text = snapDetail.name;
            TimelineUI.SetActive(false);
            if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) > 0.55f)
            {
                UI3dCamera.orthographicSize = 5;
                TimelineUI.SetActive(true);
                Vector3 screenPos = new Vector3(512, 640, 5f);
                snapDetail.localPosition = UI3dCamera.ScreenToWorldPoint(screenPos);
                snapDetail.localScale = Vector3.one * 0.5f;

                btnBackTrack.SetActive(false);
                //btnShare.SetActive(false);
                detailTitleTxt.text = "";
                is_displaySnapShotDetail = false;

            }
            if (Input.GetMouseButton(1)&&TimelineUI.activeSelf == false)
            {
                if (Input.GetAxis("Mouse X") < 0)
                    snapDetail.RotateAround(snapDetail.position, Vector3.down, 1);
                if (Input.GetAxis("Mouse X") > 0)
                    snapDetail.RotateAround(snapDetail.position, Vector3.up, 1);

                if (Input.GetAxis("Mouse Y") < 0)
                    snapDetail.RotateAround(snapDetail.position, Vector3.right, 1);
                if (Input.GetAxis("Mouse Y") > 0)
                    snapDetail.RotateAround(snapDetail.position, Vector3.left, 1);
            }
            else if (Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                UI3dCamera.orthographicSize += Input.GetAxis("Mouse ScrollWheel")*5;
            }
            else if(Input.GetKeyDown(KeyCode.F))
            {
                MoveSnapTogether();
            }
        }
    }

    private void ClickSnapBtn(string strBtnName)
    {
        Debug.Log(strBtnName);
        foreach (Transform child in snapShotsGameObject.transform)
        { 
            child.localPosition = UI3dCamera.ScreenToWorldPoint(new Vector3(2000,640,5f));
        }
        snapDetail = null;
        GameObject snap = GameObject.Find(strBtnName.Replace("btn", ""));
        Vector3 screenPos = new Vector3(512, 640, 5f);
        snap.transform.localPosition = UI3dCamera.ScreenToWorldPoint(screenPos);
        snapDetail = snap.transform;
        Debug.Log("Detail1: " + snapDetail.name);
        isClickSnapBtn = true;
    }

    private void ClickBackTrackBtn()
    {
        Vector3 screenPos = new Vector3(512, 640, 5f);
        snapDetail.localPosition = UI3dCamera.ScreenToWorldPoint(screenPos);
        //snapDetail.localPosition = oldSnapDetailPosition;
        snapDetail.localScale = Vector3.one * 0.5f;
        is_displaySnapShotDetail = false;


        TimelineUI.SetActive(false);
        UI3dRawImage.SetActive(false);
        UI3dRawImage.transform.parent.GetChild(5).gameObject.SetActive(false);
        btnBackTrack.SetActive(false);
        //btnShare.SetActive(false);
        detailTitleTxt.text = "";
        Cursor.lockState = CursorLockMode.Locked;//锁定指针到视图中心
        Cursor.visible = false;

        Transform TableAndChairsT = GameObject.Find("TableAndChairs").transform;
        foreach(Transform newChild in snapDetail)
        {
            if (newChild.name == "Sphere")
                continue;
            int i;
            bool is_clone = false;
            string newChildName = newChild.name + "(Clone)" + m_viewID;
            for (i=0;i<TableAndChairsT.childCount;i++)
            {

                if (TableAndChairsT.GetChild(i).name == newChildName)
                {
                    is_clone = true;
                    break;
                }
            }
            if(is_clone)
            {
                TableAndChairsT.GetChild(i).localPosition = newChild.localPosition;
            }
            else
            {
                for (i = 0; i < TableAndChairsT.childCount; i++)
                {
                    if (TableAndChairsT.GetChild(i).name == newChild.name)
                    {
                        break;
                    }
                }
                TableAndChairsT.GetChild(i).localPosition = newChild.localPosition;
            }
            
            
        }
    }
    
    private void MoveSnapTogether()
    {
        Vector3 screenPos = new Vector3(512, 640, 5f);
        snapDetail.localPosition = UI3dCamera.ScreenToWorldPoint(screenPos);
        //snapDetail.localPosition = oldSnapDetailPosition;
        snapDetail.localScale = Vector3.one * 0.5f;
        is_displaySnapShotDetail = false;


        TimelineUI.SetActive(false);
        UI3dRawImage.SetActive(false);
        btnBackTrack.SetActive(false);
        UI3dRawImage.transform.parent.GetChild(5).gameObject.SetActive(false);
        //btnShare.SetActive(false);
        detailTitleTxt.text = "";
        Cursor.lockState = CursorLockMode.Locked;//锁定指针到视图中心
        Cursor.visible = false;

        Transform TableAndChairsT = GameObject.Find("TableAndChairs").transform;
        foreach(Transform child in TableAndChairsT)
        {
            if(child.name.Contains("(Clone)"+m_viewID))
            {
                DestroyImmediate(child.gameObject);
            }
        }
        Transform newGo = Instantiate(snapDetail, TableAndChairsT);
        newGo.localScale = Vector3.one;
        newGo.localRotation = Quaternion.identity;
        foreach (Transform child in newGo)
        {
            if (child.name == "Sphere")
            {
                DestroyImmediate(child.gameObject);
            }
            else
            {
                child.name += "(Clone)" + m_viewID;
                child.GetComponent<MeshRenderer>().materials[0].color = new Color(m_r / 255, m_g / 255, m_b / 255);
                child.tag = m_viewID;
            }
            
        }
        targetTransform = newGo;
        isMoveSnapList = true;
        //foreach (Transform newChild in snapDetail)
        //{
        //    if (newChild.name == "Sphere")
        //        continue;
        //    snapMovePosList.Add(newChild.localPosition);
        //    Transform newGo = Instantiate(newChild, TableAndChairsT);
        //    newGo.name += m_viewID;
        //    newGo.GetComponent<MeshRenderer>().materials[0].color = new Color(m_r / 255, m_g / 255, m_b / 255);
        //    newGo.tag = m_viewID;
        //    targetTransformList.Add(newGo);
        //    isMoveSnapList = true;
        //}
    }

    private void ResortTimeline()
    {
        Transform btnContainer = TimelineUI.transform.GetChild(2);
        foreach(Transform child in btnContainer)
        {
            float childDeltaTime = child.GetComponent<DeltaTime>().CreateDeltaTime;
            child.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(childDeltaTime / timelineSize * 768 - 384, 128, 0);
        }

        Transform txtContainer = TimelineUI.transform.GetChild(0);
        int i = 0;
        foreach(Transform child in txtContainer)
        {
            i++;
            if(i==8)
            {
                break;
            }
            string oldTxt = child.GetComponent<Text>().text;
            string[] strArray = oldTxt.Split(new char[1] { ':' });

            int time = int.Parse(strArray[0]) * 60 + int.Parse(strArray[1]);
            time *= 2;
            child.GetComponent<Text>().text = (time / 60).ToString() + ":" + string.Format("{0:d2}", (time % 60));
        }
    }


    private bool IsFocusOnInputText()
    {
        if (EventSystem.current.currentSelectedGameObject == null)
            return false;
        if (EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() != null)
            return true;
        return false;
    }
}
