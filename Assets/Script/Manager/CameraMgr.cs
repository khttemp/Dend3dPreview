using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

using MainClass;

namespace CameraMgrClass
{
    public class CameraMgr : MonoBehaviour
    {
        public GameObject mainCamObj;
        Camera mCamera;
        Transform CanvasTr;
        Transform DefaultPanel;
        InputField camMoveInputField;
        InputField camRotInputField;
        InputField camFarInputField;
        Main mMain;

        string iniFilePath;
        Vector3 camPos;
        Vector3 camRot;
        float zoomper;
        public bool moveFlag;
        int keyDownCnt;
        bool holdFlag;

        string[] keyConfigList;

        // Start is called before the first frame update
        void Start()
        {
            mMain = FindMainClass();
            mCamera = mainCamObj.GetComponent<Camera>();
            CanvasTr = GameObject.Find("Canvas").transform;
            DefaultPanel = CanvasTr.Find("DefaultPanel").transform;

            camMoveInputField = DefaultPanel.Find("MoveInputField").GetComponent<InputField>();
            camRotInputField = DefaultPanel.Find("RotInputField").GetComponent<InputField>();
            camFarInputField = DefaultPanel.Find("FarInputField").GetComponent<InputField>();
            camFarInputField.onEndEdit.AddListener(UpdateCamFar);

            keyConfigList = new string[] {"w", "s", "a", "d", "q", "e", "0"};
            iniFilePath = UnityEngine.Application.dataPath + "/" + "config.ini";
            OpenIniFile();

            camPos = mainCamObj.transform.localPosition;
            camRot = mainCamObj.transform.localEulerAngles;
            moveFlag = false;
            keyDownCnt = 0;
            holdFlag = false;
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
                float sensitiveMove = GetCamMoveText();
                float moveX = Input.GetAxis("Mouse X") * sensitiveMove;
                float moveY = Input.GetAxis("Mouse Y") * sensitiveMove;
                Vector3 moveForward = mainCamObj.transform.right * moveX + mainCamObj.transform.up * moveY;
                mainCamObj.transform.localPosition -= moveForward;
            }
            if(Input.GetMouseButton(1))
            {
                float sensitiveRotate = GetCamRotText();
                float rotateX = Input.GetAxis("Mouse X") * sensitiveRotate;
                float rotateY = Input.GetAxis("Mouse Y") * sensitiveRotate;
                Vector3 curRotate = mainCamObj.transform.localEulerAngles;
                curRotate.x += rotateY;
                curRotate.y += rotateX;
                mainCamObj.transform.localEulerAngles = curRotate;
            }
            if (Input.mouseScrollDelta.y != 0f)
            {
                float scrollSensitiveZoom = GetCamMoveText();
                float moveZ = Input.GetAxis("Mouse ScrollWheel") * scrollSensitiveZoom;
                Vector3 moveForward = mainCamObj.transform.forward * moveZ;
                if (keyConfigList[6].Equals("0"))
                {
                    mainCamObj.transform.localPosition -= moveForward;
                }
                else
                {
                    mainCamObj.transform.localPosition += moveForward;
                }
            }
            if (!Input.GetMouseButton(0) && !Input.GetMouseButton(1))
            {
                if(Input.anyKey)
                {
                    if (!holdFlag)
                    {
                        holdFlag = true;
                        arrowMove();
                    }
                    else
                    {
                        if (keyDownCnt < 60)
                        {
                            keyDownCnt++;
                        }
                        else
                        {
                            arrowMove();
                        }
                    }
                }
                else
                {
                    keyDownCnt = 0;
                    holdFlag = false;
                }
            }
        }

        void arrowMove()
        {
            if (Input.GetKey(keyConfigList[0]) ||
                Input.GetKey(keyConfigList[1]) ||
                Input.GetKey(keyConfigList[2]) ||
                Input.GetKey(keyConfigList[3]) ||
                Input.GetKey(keyConfigList[4]) ||
                Input.GetKey(keyConfigList[5]))
            {
                float distance = GetCamMoveText();
                if (Input.GetKey(keyConfigList[2]))
                {
                    mainCamObj.transform.position -= mainCamObj.transform.right * distance;
                }
                if (Input.GetKey(keyConfigList[3]))
                {
                    mainCamObj.transform.position += mainCamObj.transform.right * distance;
                }
                if (Input.GetKey(keyConfigList[0]))
                {
                    mainCamObj.transform.position += mainCamObj.transform.forward * distance;
                }
                if (Input.GetKey(keyConfigList[1]))
                {
                    mainCamObj.transform.position -= mainCamObj.transform.forward * distance;
                }
                if (Input.GetKey(keyConfigList[4]))
                {
                    mainCamObj.transform.position += mainCamObj.transform.up * distance;
                }
                if (Input.GetKey(keyConfigList[5]))
                {
                    mainCamObj.transform.position -= mainCamObj.transform.up * distance;
                }
            }
        }

        Main FindMainClass()
        {
            var findObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var findObject in findObjects)
            {
                if (findObject.name == "Main") {
                    return findObject.GetComponent<Main>();
                }
            }
            return null;
        }

        public void OpenIniFile()
        {
            try
            {
                FileInfo fi = new FileInfo(iniFilePath);
                CheckIniFile(fi);
            }
            catch (System.Exception ex)
            {
                mMain.DebugError(ex.ToString());
            }
        }

        public void WriteIniFile(FileInfo fi, string label, string value)
        {
            using (StreamWriter sw = fi.AppendText()){
                sw.WriteLine($"{label}={value}");
            }
        }

        public void CheckIniFile(FileInfo fi)
        {
            string[] findList = new string[]
            {
                "MOVE",
                "ROT",
                "FAR",
                "FORWARD_KEY",
                "BACKWARD_KEY",
                "LEFT_KEY",
                "RIGHT_KEY",
                "UP_KEY",
                "DOWN_KEY",
                "MOUSE_WHEEL_FLAG",
                "ALL_DEBUG",
                "DEBUG"
            };
            Dictionary<string, string> dict = new Dictionary<string, string>()
            {
                {"MOVE", "0.01"},
                {"ROT", "3.0"},
                {"FAR", "1000.0"},
                {"FORWARD_KEY", keyConfigList[0]},
                {"BACKWARD_KEY", keyConfigList[1]},
                {"LEFT_KEY", keyConfigList[2]},
                {"RIGHT_KEY", keyConfigList[3]},
                {"UP_KEY", keyConfigList[4]},
                {"DOWN_KEY", keyConfigList[5]},
                {"MOUSE_WHEEL_FLAG", keyConfigList[6]},
                {"ALL_DEBUG", "0"},
                {"DEBUG", "0"}
            };

            if (!fi.Exists)
            {
                foreach (string label in findList)
                {
                    WriteIniFile(fi, label, dict[label]);
                }
            }
            string txtLine = "";
            using (StreamReader sr = new StreamReader(fi.OpenRead(), Encoding.UTF8)){
                txtLine = sr.ReadToEnd();
            }

            string[] separator = new string[]
            {
                "\r",
                "\n"
            };
            string[] array = txtLine.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);
            bool moveFindFlag = false;
            bool rotFindFlag = false;
            bool farFindFlag = false;
            bool allDebugFlag = false;
            bool debugFlag = false;
            bool forwardFindFlag = false;
            bool backwardFindFlag = false;
            bool leftFindFlag = false;
            bool rightFindFlag = false;
            bool upFindFlag = false;
            bool downFindFlag = false;
            bool mouseWheelFindFlag = false;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Contains("="))
                {
                    string[] separator2 = new string[]
                    {
                        "="
                    };
                    string[] collection = array[i].Split(separator2, System.StringSplitOptions.RemoveEmptyEntries);
                    if (collection.Length > 1)
                    {
                        if (collection[0].Equals("MOVE"))
                        {
                            moveFindFlag = true;
                            ChangeCamMoveText(collection[1]);
                        }
                        else if (collection[0].Equals("ROT"))
                        {
                            rotFindFlag = true;
                            ChangeCamRotText(collection[1]);
                        }
                        else if (collection[0].Equals("FAR"))
                        {
                            farFindFlag = true;
                            ChangeCamFarText(collection[1]);
                        }
                        else if (collection[0].Equals("FORWARD_KEY"))
                        {
                            forwardFindFlag = true;
                            try
                            {
                                Input.GetKey(collection[1]);
                                keyConfigList[0] = collection[1];
                            }
                            catch (System.Exception)
                            {
                                mMain.DebugError("認識できないキー(" + collection[1] + ")です。" + collection[0] + "をデフォルトキーに設定します");
                            }
                        }
                        else if (collection[0].Equals("BACKWARD_KEY"))
                        {
                            backwardFindFlag = true;
                            try
                            {
                                Input.GetKey(collection[1]);
                                keyConfigList[1] = collection[1];
                            }
                            catch (System.Exception)
                            {
                                mMain.DebugError("認識できないキー(" + collection[1] + ")です。" + collection[0] + "をデフォルトキーに設定します");
                            }
                        }
                        else if (collection[0].Equals("LEFT_KEY"))
                        {
                            leftFindFlag = true;
                            try
                            {
                                Input.GetKey(collection[1]);
                                keyConfigList[2] = collection[1];
                            }
                            catch (System.Exception)
                            {
                                mMain.DebugError("認識できないキー(" + collection[1] + ")です。" + collection[0] + "をデフォルトキーに設定します");
                            }
                        }
                        else if (collection[0].Equals("RIGHT_KEY"))
                        {
                            rightFindFlag = true;
                            try
                            {
                                Input.GetKey(collection[1]);
                                keyConfigList[3] = collection[1];
                            }
                            catch (System.Exception)
                            {
                                mMain.DebugError("認識できないキー(" + collection[1] + ")です。" + collection[0] + "をデフォルトキーに設定します");
                            }
                        }
                        else if (collection[0].Equals("UP_KEY"))
                        {
                            upFindFlag = true;
                            try
                            {
                                Input.GetKey(collection[1]);
                                keyConfigList[4] = collection[1];
                            }
                            catch (System.Exception)
                            {
                                mMain.DebugError("認識できないキー(" + collection[1] + ")です。" + collection[0] + "をデフォルトキーに設定します");
                            }
                        }
                        else if (collection[0].Equals("DOWN_KEY"))
                        {
                            downFindFlag = true;
                            try
                            {
                                Input.GetKey(collection[1]);
                                keyConfigList[5] = collection[1];
                            }
                            catch (System.Exception)
                            {
                                mMain.DebugError("認識できないキー(" + collection[1] + ")です。" + collection[0] + "をデフォルトキーに設定します");
                            }
                        }
                        else if (collection[0].Equals("MOUSE_WHEEL_FLAG"))
                        {
                            mouseWheelFindFlag = true;
                            keyConfigList[6] = collection[1];
                        }
                        else if (collection[0].Equals("ALL_DEBUG"))
                        {
                            allDebugFlag = true;
                            mMain.isAllDebug = System.Convert.ToBoolean(System.Convert.ToInt32(collection[1]));
                        }
                        else if (collection[0].Equals("DEBUG"))
                        {
                            debugFlag = true;
                            mMain.isDebug = System.Convert.ToBoolean(System.Convert.ToInt32(collection[1]));
                        }
                    }
                }
            }

            if (!moveFindFlag)
            {
                WriteIniFile(fi, "MOVE", dict["MOVE"]);
                ChangeCamMoveText(dict["MOVE"]);
            }
            if (!rotFindFlag)
            {
                WriteIniFile(fi, "ROT", dict["ROT"]);
                ChangeCamRotText(dict["ROT"]);
            }
            if (!farFindFlag)
            {
                WriteIniFile(fi, "FAR", dict["FAR"]);
                ChangeCamFarText(dict["FAR"]);
            }
            if (!forwardFindFlag)
            {
                WriteIniFile(fi, "FORWARD_KEY", dict["FORWARD_KEY"]);
            }
            if (!backwardFindFlag)
            {
                WriteIniFile(fi, "BACKWARD_KEY", dict["BACKWARD_KEY"]);
            }
            if (!leftFindFlag)
            {
                WriteIniFile(fi, "LEFT_KEY", dict["LEFT_KEY"]);
            }
            if (!rightFindFlag)
            {
                WriteIniFile(fi, "RIGHT_KEY", dict["RIGHT_KEY"]);
            }
            if (!upFindFlag)
            {
                WriteIniFile(fi, "UP_KEY", dict["UP_KEY"]);
            }
            if (!downFindFlag)
            {
                WriteIniFile(fi, "DOWN_KEY", dict["DOWN_KEY"]);
            }
            if (!mouseWheelFindFlag)
            {
                WriteIniFile(fi, "MOUSE_WHEEL_FLAG", dict["MOUSE_WHEEL_FLAG"]);
            }
            if (!allDebugFlag)
            {
                WriteIniFile(fi, "ALL_DEBUG", dict["ALL_DEBUG"]);
                mMain.isAllDebug = System.Convert.ToBoolean(System.Convert.ToInt32(dict["ALL_DEBUG"]));
            }
            if (!debugFlag)
            {
                WriteIniFile(fi, "DEBUG", dict["DEBUG"]);
                mMain.isDebug = System.Convert.ToBoolean(System.Convert.ToInt32(dict["DEBUG"]));
            }
        }

        public float GetCamMoveText()
        {
            try
            {
                float distance = System.Single.Parse(camMoveInputField.text);
                return distance;
            }
            catch (System.Exception)
            {
                mMain.DebugError("数字で変換できません：" + camMoveInputField.text);
            }
            return 0.01f;
        }

        public float GetCamRotText()
        {
            try
            {
                float rot = System.Single.Parse(camRotInputField.text);
                return rot;
            }
            catch (System.Exception)
            {
                mMain.DebugError("数字で変換できません：" + camRotInputField.text);
            }
            return 3.0f;
        }

        public void ChangeCamMoveText(string text)
        {
            camMoveInputField.text = text;
        }

        public void ChangeCamRotText(string text)
        {
            camRotInputField.text = text;
        }

        public void ChangeCamFarText(string text)
        {
            camFarInputField.text = text;
            UpdateCamFar(text);
        }

        public void UpdateCamFar(string text)
        {
            try
            {
                float far = System.Single.Parse(text);
                mCamera.farClipPlane = far;
            }
            catch (System.Exception)
            {

            }
        }
    }
}