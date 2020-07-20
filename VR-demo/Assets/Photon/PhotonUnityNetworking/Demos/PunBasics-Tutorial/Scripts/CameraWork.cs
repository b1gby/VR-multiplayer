using UnityEngine;

namespace Photon.Pun.Demo.PunBasics
{
	public class CameraWork : MonoBehaviour
	{

	    [Tooltip("The distance in the local x-z plane to the target")]
	    [SerializeField]
	    private float distance = 7.0f;
	    
	    [Tooltip("The height we want the camera to be above the target")]
	    [SerializeField]
	    private float height = 3.0f;
	    
	    [Tooltip("Allow the camera to be offseted vertically from the target, for example giving more view of the sceneray and less ground.")]
	    [SerializeField]
	    private Vector3 centerOffset = Vector3.zero;

	    [Tooltip("Set this as false if a component of a prefab being instanciated by Photon Network, and manually call OnStartFollowing() when and if needed.")]
	    [SerializeField]
	    private bool followOnStart = false;

	    [Tooltip("The Smoothing for the camera to follow the target")]
	    [SerializeField]
	    private float smoothSpeed = 0.125f;

        // cached transform of the target
        Transform cameraTransform;

		// maintain a flag internally to reconnect if target is lost or camera is switched
		bool isFollowing;
		
		// Cache for camera offset
		Vector3 cameraOffset = Vector3.zero;

        [SerializeField]
        private float mouseX, mouseY;//获取鼠标移动的值
        [SerializeField]
        private float mouseSensitivity = 200;//获取鼠标移动速度

        [SerializeField]
        private float xRotation,yRotation;
        [SerializeField]
        private int Tcount=0;

        void Start()
		{
            cameraTransform = Camera.main.transform;
            // Start following the target if wanted.
            if (followOnStart)
			{
				OnStartFollowing();
			}
            Tcount = 0;
		}


		void LateUpdate()
		{
            if (Input.GetKeyDown(KeyCode.T))
            {
                Tcount++;
            }
            if(Tcount%2 == 0)
            {
                LookAsFirstPersonView();
            }
            else
            {
                // The transform target may not destroy on level load, 
                // so we need to cover corner cases where the Main Camera is different everytime we load a new scene, and reconnect when that happens
                if (cameraTransform == null && isFollowing)
                {
                    OnStartFollowing();
                }

                // only follow is explicitly declared
                if (isFollowing)
                {
                    Follow();
                }
            }
            
		}


		public void OnStartFollowing()
		{	      
			
			isFollowing = true;
			// we don't smooth anything, we go straight to the right camera shot
			Cut();
		}
		
		
        //third-person view
		void Follow()
		{
			cameraOffset.z = -distance;
			cameraOffset.y = height;

            cameraTransform.position = Vector3.Lerp(cameraTransform.position, this.transform.position + this.transform.TransformVector(cameraOffset), smoothSpeed * Time.deltaTime);

            cameraTransform.LookAt(this.transform.position + centerOffset);

            mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            //mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            //xRotation -= mouseY;
            //xRotation = Mathf.Clamp(xRotation, -50f, 50f); //限制相机俯仰高度（-50，50）
            //yRotation += mouseX;

            this.transform.Rotate(Vector3.up * mouseX);

            

        }

	   
		void Cut()
		{
			cameraOffset.z = -distance;
			cameraOffset.y = height;

			cameraTransform.position = this.transform.position + this.transform.TransformVector(cameraOffset);

			cameraTransform.LookAt(this.transform.position + centerOffset);
		}

        //first-person view
        void LookAsFirstPersonView()
        {
            cameraTransform.position = this.transform.position + new Vector3(0, 0.8f, 0.1f);
            //cameraTransform.LookAt(cameraTransform.position);

            mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
            
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -50f, 50f); //限制相机俯仰高度（-50，50）
            yRotation += mouseX;

            this.transform.Rotate(Vector3.up * mouseX);

            cameraTransform.rotation = Quaternion.Euler(xRotation, yRotation, 0);

            
        }
	}
}