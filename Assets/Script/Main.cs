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

namespace MainClass
{
    public class Main : MonoBehaviour
    {
        public StageTblMgr mStageTblMgr = new StageTblMgr();
        public RailMgr mRailMgr = new RailMgr();
        public AMBMgr mAMBMgr = new AMBMgr();
        public string defaultPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);

        Transform CanvasTr;
        CameraMgr mCameraMgr;
        UnityButton openRailOnlyAmbButton;
        UnityButton jumpRailButton;
        Toggle cameraToggle;

        Vector3 initPosVector = new Vector3(0.1f, 0.15f, -0.4f);
        Vector3 initRotVector = new Vector3(0f, 0f, 0f);
        
        StreamWriter sw;
        string datetimePattern = "yyyy-MM-dd HH:mm:ss";

        public void Start()
        {
            mCameraMgr = FindCameraMgrClass();

            CanvasTr = GameObject.Find("Canvas").transform;
            openRailOnlyAmbButton = CanvasTr.Find("openRailOnlyAmbButton").GetComponent<UnityButton>();
            jumpRailButton = CanvasTr.Find("jumpRailButton").GetComponent<UnityButton>();
            cameraToggle = CanvasTr.Find("cameraToggle").GetComponent<Toggle>();

            SetDeactiveButton();
        }

        CameraMgr FindCameraMgrClass()
        {
            GameObject mainCamObj = GameObject.FindGameObjectWithTag("MainCamera");
            return mainCamObj.GetComponent<CameraMgr>();
        }

        public void SetActiveButton()
        {
            openRailOnlyAmbButton.interactable = true;
            jumpRailButton.interactable = true;
        }

        public void SetDeactiveButton()
        {
            openRailOnlyAmbButton.interactable = false;
            jumpRailButton.interactable = false;
        }

        public void SetDrawModel(bool railFlag, bool ambFlag)
        {
            if (railFlag)
            {
                mRailMgr.SetRailData(this);
                if (mRailMgr.isError)
                {
                    MessageBox.Show("レール配置失敗！\nエラーを確認してください", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            if (ambFlag)
            {
                mAMBMgr.SetAmbData(this);
                if (mAMBMgr.isError)
                {
                    MessageBox.Show("AMB一部 配置失敗！\nエラーを確認してください", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                mAMBMgr.RemoveAMB();
            }
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

        public void DebugError(string message)
        {
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
        }

        public void DebugWarning(string message)
        {
            try 
            {
                string ERROR_PATH = Path.Combine(System.Windows.Forms.Application.StartupPath, "warning.log");
                sw = new StreamWriter(ERROR_PATH, true, Encoding.UTF8);
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
        }
    }
}