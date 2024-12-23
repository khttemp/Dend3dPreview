using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.UI;
using UnityButton = UnityEngine.UI.Button;
using System.IO;

using MainClass;
using JointMdlClass;
using RailListClass;
using CameraMgrClass;
using GetObjectLabelClass;

public class editRailData : MonoBehaviour
{
    GameObject dialogObject = null;
    UnityButton button;
    Transform CanvasTr;
    Transform DefaultPanel;
    Transform Panel;
    Toggle camMoveToggle;
    Toggle editModeToggle;
    Main mMain;
    CameraMgr mCameraMgr;
    UnityButton OkButton;

    int editRailIndex;
    float floatDirX;
    float floatDirY;
    float floatDirZ;
    float floatPer;

    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<UnityButton>();
        button.onClick.AddListener(editRailDataFunc);

        CanvasTr = GameObject.Find("Canvas").transform;
        DefaultPanel = CanvasTr.Find("DefaultPanel").transform;
        mMain = FindMainClass();
        mCameraMgr = FindCameraMgrClass();
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

    CameraMgr FindCameraMgrClass()
    {
        GameObject mainCamObj = GameObject.FindGameObjectWithTag("MainCamera");
        return mainCamObj.GetComponent<CameraMgr>();
    }

    void editRailDataFunc()
    {
        string dialogPath = "dialogPrefab/editRailPanel";
        try
        {
            DefaultPanel.gameObject.SetActive(false);
            dialogObject = (UnityEngine.Object.Instantiate(Resources.Load(dialogPath)) as GameObject);
            dialogObject.name = dialogObject.name.Replace("(Clone)", "");
            dialogObject.transform.SetParent(CanvasTr, false);
            dialogObject.transform.localScale = Vector3.one;
            Panel = dialogObject.transform.Find("Panel").GetComponent<Transform>();
            OkButton = Panel.transform.Find("OkButton").GetComponent<UnityButton>();
            OkButton.onClick.AddListener(ClickOkButton);
            Panel.transform.Find("CancelButton").GetComponent<UnityButton>().onClick.AddListener(ClickCancelButton);

            editRailIndex = mMain.mRailMgr.search_rail_index;
            rail_list r = mMain.mStageTblMgr.RailList[editRailIndex];

            Text curRailNumLabel = Panel.transform.Find("curRailNumLabel").GetComponent<Text>();
            curRailNumLabel.text = editRailIndex.ToString();
            Text fileNameLabel = Panel.transform.Find("FileNameLabel").GetComponent<Text>();
            string fileNameText = "";
            string fileExt = Path.GetExtension(mMain.openFilename).ToLower();
            if (".txt".Equals(fileExt))
            {
                fileNameText = "【テキストファイル】\n" + Path.GetFileName(mMain.openFilename);
            }
            fileNameLabel.text = fileNameText;

            camMoveToggle = Panel.transform.Find("camMoveToggle").GetComponent<Toggle>();
            camMoveToggle.onValueChanged.AddListener(SetCamMoveFlag);
            editModeToggle = Panel.transform.Find("editModeToggle").GetComponent<Toggle>();
            editModeToggle.isOn = mMain.editModeFlag;
            editModeToggle.onValueChanged.AddListener(SetEditMode);

            InputField railDirXInputField = Panel.transform.Find("railDirXInputField").GetComponent<InputField>();
            railDirXInputField.text = r.dir.x.ToString();
            railDirXInputField.onEndEdit.AddListener(EnterPressed);
            railDirXInputField.onValueChanged.AddListener(railDirChangeFunc);
            InputField railDirYInputField = Panel.transform.Find("railDirYInputField").GetComponent<InputField>();
            railDirYInputField.text = r.dir.y.ToString();
            railDirYInputField.onEndEdit.AddListener(EnterPressed);
            railDirYInputField.onValueChanged.AddListener(railDirChangeFunc);
            InputField railDirZInputField = Panel.transform.Find("railDirZInputField").GetComponent<InputField>();
            railDirZInputField.text = r.dir.z.ToString();
            railDirZInputField.onEndEdit.AddListener(EnterPressed);
            railDirZInputField.onValueChanged.AddListener(railDirChangeFunc);
            InputField railPerInputField = Panel.transform.Find("railPerInputField").GetComponent<InputField>();
            railPerInputField.text = r.per.ToString();
            railPerInputField.onEndEdit.AddListener(EnterPressed);
            railPerInputField.onValueChanged.AddListener(railDirChangeFunc);

            railDirXInputField.Select();
        }
        catch(System.Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    void EnterPressed(string _)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            ClickOkButton();
        }
    }

    void SetCamMoveFlag(bool state)
    {
        mCameraMgr.moveFlag = !state;
    }

    void SetEditMode(bool state)
    {
        mMain.editModeFlag = state;
    }

    void railDirChangeFunc(string text)
    {
        InputField railDirXInputField = Panel.transform.Find("railDirXInputField").GetComponent<InputField>();
        InputField railDirYInputField = Panel.transform.Find("railDirYInputField").GetComponent<InputField>();
        InputField railDirZInputField = Panel.transform.Find("railDirZInputField").GetComponent<InputField>();
        InputField railPerInputField = Panel.transform.Find("railPerInputField").GetComponent<InputField>();
        string railDirX = railDirXInputField.text;
        string railDirY = railDirYInputField.text;
        string railDirZ = railDirZInputField.text;
        string railPer = railPerInputField.text;
        if ("".Equals(railDirX) || "".Equals(railDirY) || "".Equals(railDirZ) || "".Equals(railPer))
        {
            OkButton.interactable = false;
            return;
        }
        try
        {
            floatDirX = float.Parse(railDirX);
            floatDirY = float.Parse(railDirY);
            floatDirZ = float.Parse(railDirZ);
            floatPer = float.Parse(railPer);
        }
        catch (System.Exception)
        {
            OkButton.interactable = false;
            return;
        }
        if (floatPer == 0)
        {
            OkButton.interactable = false;
            return;
        }
        OkButton.interactable = true;
        GameObject railObj = mMain.mRailMgr.railObjList[editRailIndex];
        JointMdl railObjJointMdl = railObj.GetComponent<JointMdl>();
        railObjJointMdl.JointDir = new Vector3(floatDirX, floatDirY, floatDirZ);
        railObjJointMdl.LengthPer = floatPer;
        railObjJointMdl.UpdateJointDir();
    }

    void ClickOkButton()
    {
        try
        {
            string fileExt = Path.GetExtension(mMain.openFilename).ToLower();
            string filePath;
            string newFilePath;
            string reloadFilePath;
            if (".txt".Equals(fileExt))
            {
                string fileContent = File.ReadAllText(mMain.openFilename);
                string[] separator = new string[]
                {
                    "\r\n"
                };
                string[] originArray = fileContent.Split(separator, System.StringSplitOptions.None);
                int railDataTxtIndex = mMain.mStageTblMgr.getRailDataTxtIndex(mMain.mRailMgr.search_rail_index, fileContent, originArray, mMain);
                if (railDataTxtIndex == -1)
                {
                    MessageBox.Show("テキストの書込失敗！\nエラーを確認してください", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                string originRailData = originArray[railDataTxtIndex];
                string[] separator2 = new string[]
                {
                    "\r",
                    "\t"
                };
                string[] collection = originRailData.Split(separator2, System.StringSplitOptions.RemoveEmptyEntries);
                collection[6] = floatDirX.ToString();
                collection[7] = floatDirY.ToString();
                collection[8] = floatDirZ.ToString();
                collection[11] = floatPer.ToString();
                originArray[railDataTxtIndex] = string.Join("\t", collection);
                filePath = mMain.openFilename;
                if (mMain.editModeFlag)
                {
                    string txtFileName = Path.GetFileNameWithoutExtension(mMain.openFilename);
                    string txtFileExt = Path.GetExtension(mMain.openFilename);
                    newFilePath = Path.Combine(Path.GetDirectoryName(mMain.openFilename), txtFileName + "_new" + txtFileExt);
                    File.WriteAllLines(newFilePath, originArray);
                    reloadFilePath = newFilePath;
                }
                else
                {
                    File.WriteAllLines(filePath, originArray);
                    reloadFilePath = filePath;
                }

                foreach (Transform child in dialogObject.transform)
                {
                    UnityEngine.Object.Destroy(child.gameObject);
                }
                UnityEngine.Object.Destroy(dialogObject);
                dialogObject = null;
                DefaultPanel.gameObject.SetActive(true);
                mCameraMgr.moveFlag = true;

                using (var fileStream = File.Open(reloadFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        fileContent = reader.ReadToEnd();
                    }
                    bool result = mMain.mStageTblMgr.Open(fileContent, mMain);
                    if (!result)
                    {
                        MessageBox.Show("再読込失敗！\nエラーを確認してください", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        mMain.SetDrawModel(mMain.railFlag, mMain.ambFlag, true);
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            MessageBox.Show("予想外のエラー！\nエラーを確認してください", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Debug.Log(ex.ToString());
        }
    }

    void ClickCancelButton()
    {
        rail_list r = mMain.mStageTblMgr.RailList[editRailIndex];
        GameObject railObj = mMain.mRailMgr.railObjList[editRailIndex];
        JointMdl railObjJointMdl = railObj.GetComponent<JointMdl>();
        railObjJointMdl.JointDir = r.dir;
        railObjJointMdl.LengthPer = r.per;
        railObjJointMdl.UpdateJointDir();

        foreach (Transform child in dialogObject.transform)
        {
            UnityEngine.Object.Destroy(child.gameObject);
        }
        UnityEngine.Object.Destroy(dialogObject);
        dialogObject = null;
        DefaultPanel.gameObject.SetActive(true);
        mCameraMgr.moveFlag = true;
    }
}
