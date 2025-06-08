using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityButton = UnityEngine.UI.Button;

using CameraMgrClass;
using StageTblMgrClass;
using RailMgrClass;
using AMBMgrClass;
using FileReadMgrClass;
using JointMdlClass;
using GetObjectLabelClass;
using RSRailMgrClass;

namespace MainClass
{
    public class Main : MonoBehaviour
    {
        public StageTblMgr mStageTblMgr = new StageTblMgr();
        public RailMgr mRailMgr;
        public AMBMgr mAMBMgr = new AMBMgr();
        public FileReadMgr mFileReadMgr = new FileReadMgr();
        public RSRailMgr mRSRailMgr;
        public string defaultPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        public string openFilename;
        public bool editModeFlag;
        public bool railFlag;
        public bool ambFlag;
        public int excelRailFirstRowNum;
        public int excelAmbFirstRowNum;
        public bool excelAmbNewLineFlag;
        public int defaultOfdIndex = 1;

        Transform CanvasTr;
        Transform DefaultPanel;
        CameraMgr mCameraMgr;
        UnityButton openRailOnlyAmbButton;
        UnityButton jumpRailButton;
        UnityButton jumpAmbButton;
        UnityButton editRailButton;
        UnityButton editAmbButton;
        Toggle cameraToggle;

        GameObject loadingPanel;
        Text loadingText;

        public int modelDisplayMode = 0;
        public bool isAllDebug;
        public bool isDebug;

        Vector3 initPosVector = new Vector3(0.1f, 0.15f, -0.4f);
        Vector3 initRotVector = new Vector3(0f, 0f, 0f);
        
        StreamWriter sw;
        string datetimePattern = "yyyy-MM-dd HH:mm:ss";

        public void Start()
        {
            mRailMgr = GameObject.Find("RailMgr").GetComponent<RailMgr>();
            mRSRailMgr = GameObject.Find("RSRailMgr").GetComponent<RSRailMgr>();
            mCameraMgr = FindCameraMgrClass();

            CanvasTr = GameObject.Find("Canvas").transform;
            DefaultPanel = CanvasTr.Find("DefaultPanel").transform;
            openRailOnlyAmbButton = DefaultPanel.Find("openRailOnlyAmbButton").GetComponent<UnityButton>();
            jumpRailButton = DefaultPanel.Find("jumpRailButton").GetComponent<UnityButton>();
            jumpAmbButton = DefaultPanel.Find("jumpAmbButton").GetComponent<UnityButton>();
            editModeFlag = true;
            editRailButton = DefaultPanel.Find("editRailButton").GetComponent<UnityButton>();
            editAmbButton = DefaultPanel.Find("editAmbButton").GetComponent<UnityButton>();
            cameraToggle = DefaultPanel.Find("cameraToggle").GetComponent<Toggle>();

            string loadingPanelPath = "dialogPrefab/LoadingPanel";
            loadingPanel = (UnityEngine.Object.Instantiate(Resources.Load(loadingPanelPath)) as GameObject);
            loadingPanel.name = loadingPanel.name.Replace("(Clone)", "");
            loadingPanel.transform.SetParent(DefaultPanel, false);
            loadingText = loadingPanel.transform.Find("LoadingText").GetComponent<Text>();

            SetDeactiveAmbReadButton();
        }

        CameraMgr FindCameraMgrClass()
        {
            GameObject mainCamObj = GameObject.FindGameObjectWithTag("MainCamera");
            return mainCamObj.GetComponent<CameraMgr>();
        }

        public void SetActiveAmbReadButton()
        {
            openRailOnlyAmbButton.interactable = true;
        }

        public void SetDeactiveAmbReadButton()
        {
            openRailOnlyAmbButton.interactable = false;
        }

        public void SetActiveJumpRailButton()
        {
            jumpRailButton.interactable = true;
        }

        public void SetDeactiveJumpRailButton()
        {
            jumpRailButton.interactable = false;
        }

        public void SetActiveJumpAmbButton()
        {
            jumpAmbButton.interactable = true;
        }

        public void SetDeactiveJumpAmbButton()
        {
            jumpAmbButton.interactable = false;
        }

        public void SetShowEditRailButton()
        {
            editRailButton.gameObject.SetActive(true);
        }

        public void SetHideEditRailButton()
        {
            editRailButton.gameObject.SetActive(false);
        }

        public void SetShowEditAmbButton()
        {
            editAmbButton.gameObject.SetActive(true);
        }

        public void SetHideEditAmbButton()
        {
            editAmbButton.gameObject.SetActive(false);
        }

        public void InitRailObjOutlineAndLabel()
        {
            if (mRailMgr.search_rail_index != -1)
            {
                try
                {
                    GameObject delObj = mRailMgr.railObjList[mRailMgr.search_rail_index];
                    JointMdl delJointMdl = delObj.GetComponent<JointMdl>();
                    Transform delBaseMdl = delJointMdl.AllTransformChildren[1];
                    if (delBaseMdl.gameObject.GetComponent<Outline>() != null)
                    {
                        GameObject.Destroy(delBaseMdl.gameObject.GetComponent<Outline>());
                    }
                    if (delObj.GetComponent<GetObjectLabel>() != null)
                    {
                        GameObject.Destroy(delObj.GetComponent<GetObjectLabel>());
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(ex.ToString());
                }
                mRailMgr.search_rail_index = -1;
            }
        }

        public void InitAmbObjOutlineAndLabel()
        {
            if (mAMBMgr.search_amb_no != -1 && mAMBMgr.search_amb_index != -1)
            {
                try
                {
                    GameObject delObj = mAMBMgr.ambObjList[mAMBMgr.search_amb_no][mAMBMgr.search_amb_index];
                    JointMdl delJointMdl = delObj.GetComponent<JointMdl>();
                    Transform delBaseMdl = delJointMdl.AllTransformChildren[1];
                    if (delBaseMdl.gameObject.GetComponent<Outline>() != null)
                    {
                        GameObject.Destroy(delBaseMdl.gameObject.GetComponent<Outline>());
                    }
                    if (delObj.GetComponent<GetObjectLabel>() != null)
                    {
                        GameObject.Destroy(delObj.GetComponent<GetObjectLabel>());
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(ex.ToString());
                }
                mAMBMgr.search_amb_no = -1;
                mAMBMgr.search_amb_index = -1;
            }
        }

        public void SetDrawModel(bool railFlag, bool ambFlag, bool reDrawFlag)
        {
            StartCoroutine(_SetDrawModel(railFlag, ambFlag, reDrawFlag));
        }

        public IEnumerator _SetDrawModel(bool railFlag, bool ambFlag, bool reDrawFlag)
        {
            if (railFlag)
            {
                int editRailIndex = mRailMgr.search_rail_index;
                SetPanelText("レール\n読み込み中...");
                yield return null;
                mRailMgr.SetRailData(this);
                if (mRailMgr.isError)
                {
                    MessageBox.Show("レール配置失敗！\nエラーを確認してください", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    if (mRailMgr.isCheckError)
                    {
                        MessageBox.Show("レール繋がり異常！\nログを確認してください", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }

                if (reDrawFlag && editRailIndex != -1)
                {
                    InitAmbObjOutlineAndLabel();
                    InitRailObjOutlineAndLabel();
                    GameObject railObj = mRailMgr.railObjList[editRailIndex];
                    JointMdl jointMdl = railObj.GetComponent<JointMdl>();
                    Transform baseMdl = jointMdl.AllTransformChildren[1];
                    if (baseMdl.gameObject.GetComponent<Outline>() == null)
                    {
                        var outline = baseMdl.gameObject.AddComponent<Outline>();
                        outline.OutlineMode = Outline.Mode.OutlineAll;
                        outline.OutlineColor = Color.yellow;
                        outline.OutlineWidth = 10f;
                    }
                    if (railObj.GetComponent<GetObjectLabel>() == null)
                    {
                        railObj.AddComponent<GetObjectLabel>();
                    }
                    mRailMgr.search_rail_index = editRailIndex;
                }
            }

            if (ambFlag)
            {
                int editAmbNo = mAMBMgr.search_amb_no;
                int editAmbIndex = mAMBMgr.search_amb_index;
                SetPanelText("AMB\n読み込み中...");
                yield return null;
                mAMBMgr.SetAmbData(this);
                if (mAMBMgr.isError)
                {
                    MessageBox.Show("AMB一部 配置失敗！\nエラーを確認してください", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (reDrawFlag && editAmbNo != -1 && editAmbIndex != -1)
                {
                    InitRailObjOutlineAndLabel();
                    InitAmbObjOutlineAndLabel();
                    GameObject ambObj = mAMBMgr.ambObjList[editAmbNo][editAmbIndex];
                    JointMdl jointMdl = ambObj.GetComponent<JointMdl>();
                    Transform baseMdl = jointMdl.AllTransformChildren[1];
                    if (baseMdl.gameObject.GetComponent<Outline>() == null)
                    {
                        var outline = baseMdl.gameObject.AddComponent<Outline>();
                        outline.OutlineMode = Outline.Mode.OutlineAll;
                        outline.OutlineColor = Color.yellow;
                        outline.OutlineWidth = 10f;
                    }
                    if (ambObj.GetComponent<GetObjectLabel>() == null)
                    {
                        ambObj.AddComponent<GetObjectLabel>();
                    }
                    mAMBMgr.search_amb_no = editAmbNo;
                    mAMBMgr.search_amb_index = editAmbIndex;
                }
            }
            else
            {
                mAMBMgr.RemoveAMB();
            }
            if (mRailMgr.railObjList != null && mRailMgr.railObjList.Count > 0)
            {
                SetActiveJumpRailButton();
            }
            else
            {
                SetDeactiveJumpRailButton();
            }
            if (mAMBMgr.drawAmbCount > 0)
            {
                SetActiveJumpAmbButton();
            }
            else
            {
                SetDeactiveJumpAmbButton();
            }
            SetPanelText("");
            yield return null;
            SetInitCamera();
        }

        public void SetInitCamera()
        {
            if (cameraToggle.isOn)
            {
                mCameraMgr.mainCamObj.transform.position = initPosVector;
                mCameraMgr.mainCamObj.transform.rotation = Quaternion.Euler(initRotVector);
            }
            mCameraMgr.moveFlag = true;
        }

        public void SetRSDrawModel()
        {
            StartCoroutine(_SetRSDrawModel());
        }

        public IEnumerator _SetRSDrawModel()
        {
            SetPanelText("レール\n読み込み中...");
            yield return null;
            mRSRailMgr.search_rail_index = -1;
            mRSRailMgr.SetRailData(this);
            SetPanelText("");
            yield return null;
        }

        public void DebugError(string message)
        {
#if UNITY_EDITOR
            sw = new StreamWriter(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "error.log"), true, Encoding.UTF8);
            sw.Write(string.Format("[{0}][ERROR]：", System.DateTime.Now.ToString(datetimePattern)));
            sw.WriteLine(message);
            sw.Close();
#else
            try 
            {
                string ERROR_PATH = Path.Combine(System.Windows.Forms.Application.StartupPath, "error.log");
                sw = new StreamWriter(ERROR_PATH, true, Encoding.UTF8);
                sw.Write(string.Format("[{0}][ERROR]：", System.DateTime.Now.ToString(datetimePattern)));
                sw.WriteLine(message);
                sw.Close();
            }
            catch (System.Exception ex)
            {
                sw = new StreamWriter(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "error.log"), true, Encoding.UTF8);
                sw.Write(string.Format("[{0}][ERROR]：", System.DateTime.Now.ToString(datetimePattern)));
                sw.WriteLine(ex.ToString() + "\n");
                sw.Write(string.Format("[{0}][ERROR]：", System.DateTime.Now.ToString(datetimePattern)));
                sw.WriteLine(message);
                sw.Close();
            }
#endif
        }

        public void DebugWarning(string message)
        {
#if UNITY_EDITOR
                sw = new StreamWriter(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "warning.log"), true, Encoding.UTF8);
                sw.Write(string.Format("[{0}][WARNING]：", System.DateTime.Now.ToString(datetimePattern)));
                sw.WriteLine(message);
                sw.Close();
#else
            try 
            {
                string WARNING_PATH = Path.Combine(System.Windows.Forms.Application.StartupPath, "warning.log");
                sw = new StreamWriter(WARNING_PATH, true, Encoding.UTF8);
                sw.Write(string.Format("[{0}][WARNING]：", System.DateTime.Now.ToString(datetimePattern)));
                sw.WriteLine(message);
                sw.Close();
            }
            catch (System.Exception ex)
            {
                sw = new StreamWriter(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "warning.log"), true, Encoding.UTF8);
                sw.Write(string.Format("[{0}][WARNING]：", System.DateTime.Now.ToString(datetimePattern)));
                sw.WriteLine(ex.ToString() + "\n");
                sw.Write(string.Format("[{0}][WARNING]：", System.DateTime.Now.ToString(datetimePattern)));
                sw.WriteLine(message);
                sw.Close();
            }
#endif
        }

        public void SetPanelText(string text)
        {
            if (text == null || "".Equals(text))
            {
                loadingPanel.SetActive(false);
                loadingText.text = "";
            }
            else
            {
                loadingPanel.SetActive(true);
                loadingText.text = text;
            }
        }
    }
}