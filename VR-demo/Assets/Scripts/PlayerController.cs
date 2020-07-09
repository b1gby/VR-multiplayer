using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using System;
using UnityEditor;


public class PlayerController : MonoBehaviourPun, IPunObservable
{
    public void Initialize(GameObject character)
    {
        m_animator = character.GetComponent<Animator>();
        m_rigidBody = character.GetComponent<Rigidbody>();
    }

    
    [SerializeField] private float m_moveSpeed = 2.0f;
    [SerializeField] private float m_jumpForce = 4;

    [SerializeField] private Animator m_animator;
    [SerializeField] private Rigidbody m_rigidBody;


    private float m_currentV = 0;
    private float m_currentH = 0;

    private readonly float m_interpolation = 10;
    private readonly float m_runScale = 2.0f;


    private bool m_wasGrounded;
    private Vector3 m_currentDirection = Vector3.zero;

    private float m_jumpTimeStamp = 0;
    private float m_minJumpInterval = 0.25f;

    private bool m_isGrounded;
    
    private List<Collider> m_collisions = new List<Collider>();

    Transform cameraTransform;
    private Transform targetTransform;
    private bool isDragObject = false;
    private static int pressECount = 0;

    private static bool isRecvCopyOnce = false;
    private Transform recvTransform = null;
    private Transform recvTransformTmp = null;

    private float m_r = 0;
    private float m_g = 0;
    private float m_b = 0;

    private string m_viewID;
    void Awake()
    {
        if(!m_animator) { gameObject.GetComponent<Animator>(); }
        if(!m_rigidBody) { gameObject.GetComponent<Animator>(); }
    }


    void Start()
    {
        m_viewID = this.photonView.ViewID.ToString();
        m_r = UnityEngine.Random.value * 256;
        m_g = UnityEngine.Random.value * 256;
        m_b = UnityEngine.Random.value * 256;
        CameraController _cameraController = this.gameObject.GetComponent<CameraController>();
        cameraTransform = Camera.main.transform;
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
        if (stream.IsWriting)
        {
            GameObject[] sendTargets = GameObject.FindGameObjectsWithTag(m_viewID);

            //if(targetTransform!=null)
            if(sendTargets.Length != 0)
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
        }
        if(stream.IsReading)
        {
            

            string[] recvArray = { "GG"};
            try
            {
                string recvStr = (string)stream.ReceiveNext();
                recvArray = recvStr.Split(new char[1] {':' });
            }
            catch(Exception e)
            {
                //nothing to do;
                //TODO:如果之后收到的是字符串并且有错误，这里可能会忽略.因为为了接收selectTarget调试方便，将错误忽略了，最后完成时要将其加上。
                // Debug.Log(e);
            }

            if(recvArray[0] == "syncSelectTarget")
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

                    recvTransform.GetComponent<MeshRenderer>().materials = new Material[]
                    {
                        Resources.Load("Materials/New_Chair_Material") as Material,
                    };
                    newColor.a = 0.3f;
                    recvTransform.GetComponent<MeshRenderer>().materials[0].color = newColor;
                }
            }
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
        if(isDragObject)
        {
            targetTransform.position = cameraTransform.position + cameraTransform.forward * 2f;
        }
    }

    void FixedUpdate ()
    {
        //避免误操作其他玩家
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            return;
        }
        //Debug.Log(photonView.IsMine);

        m_animator.SetBool("Grounded", m_isGrounded);

        // movement
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        Transform camera = Camera.main.transform;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            v *= m_runScale;
            h *= m_runScale;
        }

        m_currentV = Mathf.Lerp(m_currentV, v, Time.deltaTime * m_interpolation);
        m_currentH = Mathf.Lerp(m_currentH, h, Time.deltaTime * m_interpolation);

        Vector3 direction = camera.forward * m_currentV + camera.right * m_currentH;

        float directionLength = direction.magnitude;
        direction.y = 0;
        direction = direction.normalized * directionLength;

        if (direction != Vector3.zero)
        {
            m_currentDirection = Vector3.Slerp(m_currentDirection, direction, Time.deltaTime * m_interpolation);

            transform.position += m_currentDirection * m_moveSpeed * Time.deltaTime;
            //Debug.Log(direction.magnitude / 2.0f);
            m_animator.SetFloat("MoveSpeed", direction.magnitude / 2.0f);
        }


        // jump
        JumpingAndLanding();


        // some action
        // wave
        if (Input.GetKeyDown(KeyCode.Mouse4))
        {
            m_animator.SetTrigger("Wave");
        }

        // pickup
        if (Input.GetKeyDown(KeyCode.E))
        {
            pressECount++;
            if(pressECount%2 == 1)
            {
                SelectObject();
            }
            else
            {
                SelectDownObject();
            }
        }
        

        m_wasGrounded = m_isGrounded;
         
    }

    private void SelectObject()
    {
        
        RaycastHit screenCenterTarget;

        if(Physics.Raycast(cameraTransform.position, cameraTransform.forward,out screenCenterTarget,100f))
        {
            targetTransform = screenCenterTarget.transform;
            //Debug.Log(targetTransform.name);
        }

        //这里if加上&&后面，是因为在别人选择物体移动时，只能选择基本的（Everyone），不能动其他人动过的
        if (targetTransform!=null && (targetTransform.tag == "Everyone" || targetTransform.tag == m_viewID))
        {
            if(targetTransform.tag == m_viewID)
            {
                //nothing to do
            }
            else if (GameObject.Find(targetTransform.name + "(Clone)" + m_viewID) == null)
            {
                targetTransform = Instantiate(targetTransform.gameObject,targetTransform.parent).transform;

                Material[] m = new Material[]
                    {
                        Resources.Load("Materials/New_Chair_Material") as Material,
                    };
                
                m[0].color = new Color(m_r/255,m_g/255,m_b/255);

                targetTransform.name += m_viewID;
                targetTransform.GetComponent<MeshRenderer>().materials = m;
                targetTransform.tag = m_viewID;
            }
            else
            {
                targetTransform = GameObject.Find(targetTransform.name + "(Clone)" + m_viewID).transform;
            }
            isDragObject = true;
        }
    }

    private void SelectDownObject()
    {
        isDragObject = false;
        targetTransform = null;
    }

    private void JumpingAndLanding()
    {
        bool jumpCooldownOver = (Time.time - m_jumpTimeStamp) >= m_minJumpInterval;

        if (jumpCooldownOver && m_isGrounded && Input.GetKey(KeyCode.Space))
        {
            m_jumpTimeStamp = Time.time;
            m_rigidBody.AddForce(Vector3.up * m_jumpForce, ForceMode.Impulse);
        }

        if (!m_wasGrounded && m_isGrounded)
        {
            m_animator.SetTrigger("Land");
        }

        if (!m_isGrounded && m_wasGrounded)
        {
            m_animator.SetTrigger("Jump");
        }
    }

}
