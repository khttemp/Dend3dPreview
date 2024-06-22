using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CameraMgrClass
{
    public class CameraMgr : MonoBehaviour
    {
        public GameObject mainCamObj;
        Vector3 camPos;
        float zoomper;
        public bool moveFlag;

        // Start is called before the first frame update
        void Start()
        {
            mainCamObj = GameObject.FindGameObjectWithTag("MainCamera");
            camPos = mainCamObj.transform.localPosition;
            moveFlag = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (!moveFlag)
            {
                return;
            }
            if (Input.GetMouseButton(0))
            {
                float sensitiveMove = 0.15f * Mathf.Abs(mainCamObj.transform.position.y);
                if (sensitiveMove < 0.15f)
                {
                    sensitiveMove = 0.15f;
                }
                float moveX = Input.GetAxis("Mouse X") * sensitiveMove;
                float moveY = Input.GetAxis("Mouse Y") * sensitiveMove;
                Vector3 moveForward = mainCamObj.transform.up * moveY + mainCamObj.transform.right * moveX;
                mainCamObj.transform.position -= moveForward;
            }
            if(Input.GetMouseButton(1))
            {
                float sensitiveRotate = 5f;
                float rotateX = Input.GetAxis("Mouse X") * sensitiveRotate;
                float rotateY = Input.GetAxis("Mouse Y") * sensitiveRotate;
                mainCamObj.transform.Rotate(rotateY, rotateX, 0.0f);
            }
            if (Input.mouseScrollDelta.y != 0f)
            {
                float scrollSensitiveZoom = 1f * Mathf.Abs(mainCamObj.transform.position.y);
                if (scrollSensitiveZoom < 1f)
                {
                    scrollSensitiveZoom = 1f;
                }
                float moveZ = Input.GetAxis("Mouse ScrollWheel") * scrollSensitiveZoom;
                mainCamObj.transform.position += (mainCamObj.transform.forward * moveZ);
            }
        }
    }
}