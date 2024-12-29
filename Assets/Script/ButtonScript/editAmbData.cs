using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.UI;
using UnityButton = UnityEngine.UI.Button;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.Util;

using MainClass;
using JointMdlClass;
using AmbDataClass;
using AmbMdlClass;
using CameraMgrClass;
using GetObjectLabelClass;

public class editAmbData : MonoBehaviour
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
    XSSFWorkbook xssWorkbook;
    ISheet iSheet;

    float floatPosX;
    float floatPosY;
    float floatPosZ;
    float floatRotX;
    float floatRotY;
    float floatRotZ;
    float floatDirX;
    float floatDirY;
    float floatDirZ;
    float floatPer;
    float floatKasenchuPer;

    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<UnityButton>();
        button.onClick.AddListener(editAmbDataFunc);

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

    void editAmbDataFunc()
    {
        string dialogPath = "dialogPrefab/editAmbPanel";
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

            int editAmbNo = mMain.mAMBMgr.search_amb_no;
            int editAmbIndex = mMain.mAMBMgr.search_amb_index;
            amb_data amb = mMain.mStageTblMgr.AmbList[editAmbNo].datalist[editAmbIndex];
            GameObject ambObj = mMain.mAMBMgr.ambObjList[editAmbNo][editAmbIndex];
            JointMdl ambObjJointMdl = ambObj.GetComponent<JointMdl>();

            Text curAmbNumLabel = Panel.transform.Find("curAmbNumLabel").GetComponent<Text>();
            curAmbNumLabel.text = editAmbNo.ToString() + "-" + editAmbIndex.ToString();
            Text fileNameLabel = Panel.transform.Find("FileNameLabel").GetComponent<Text>();
            string fileNameText = "";
            string fileExt = Path.GetExtension(mMain.openFilename).ToLower();
            if (".txt".Equals(fileExt))
            {
                fileNameText = "【テキストファイル】\n" + Path.GetFileName(mMain.openFilename);
            }
            else if (".xlsx".Equals(fileExt))
            {
                fileNameText = "【エクセルファイル】\n" + Path.GetFileName(mMain.openFilename);
                using (var fileStream = File.Open(mMain.openFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    xssWorkbook = new XSSFWorkbook(fileStream);
                    iSheet = xssWorkbook.GetSheet("AMB情報");
                    if (iSheet != null)
                    {
                        fileNameText += "\nの「AMB情報」シート";
                    }
                    else
                    {
                        iSheet = xssWorkbook.GetSheetAt(0);
                        if (iSheet != null)
                        {
                            fileNameText += "\nの「1番目」シート";
                        }
                    }
                }
            }
            fileNameLabel.text = fileNameText;

            camMoveToggle = Panel.transform.Find("camMoveToggle").GetComponent<Toggle>();
            camMoveToggle.onValueChanged.AddListener(SetCamMoveFlag);
            editModeToggle = Panel.transform.Find("editModeToggle").GetComponent<Toggle>();
            editModeToggle.isOn = mMain.editModeFlag;
            editModeToggle.onValueChanged.AddListener(SetEditMode);

            InputField ambPosXInputField = Panel.transform.Find("ambPosXInputField").GetComponent<InputField>();
            ambPosXInputField.text = amb.offsetpos.x.ToString();
            floatPosX = amb.offsetpos.x;
            ambPosXInputField.onEndEdit.AddListener(EnterPressed);
            ambPosXInputField.onValueChanged.AddListener(ambChangeFunc);
            InputField ambPosYInputField = Panel.transform.Find("ambPosYInputField").GetComponent<InputField>();
            ambPosYInputField.text = amb.offsetpos.y.ToString();
            floatPosY = amb.offsetpos.y;
            ambPosYInputField.onEndEdit.AddListener(EnterPressed);
            ambPosYInputField.onValueChanged.AddListener(ambChangeFunc);
            InputField ambPosZInputField = Panel.transform.Find("ambPosZInputField").GetComponent<InputField>();
            ambPosZInputField.text = amb.offsetpos.z.ToString();
            floatPosZ = amb.offsetpos.z;
            ambPosZInputField.onEndEdit.AddListener(EnterPressed);
            ambPosZInputField.onValueChanged.AddListener(ambChangeFunc);

            InputField ambRotXInputField = Panel.transform.Find("ambRotXInputField").GetComponent<InputField>();
            ambRotXInputField.text = amb.dir.x.ToString();
            floatRotX = amb.dir.x;
            ambRotXInputField.onEndEdit.AddListener(EnterPressed);
            ambRotXInputField.onValueChanged.AddListener(ambChangeFunc);
            InputField ambRotYInputField = Panel.transform.Find("ambRotYInputField").GetComponent<InputField>();
            ambRotYInputField.text = amb.dir.y.ToString();
            floatRotY = amb.dir.y;
            ambRotYInputField.onEndEdit.AddListener(EnterPressed);
            ambRotYInputField.onValueChanged.AddListener(ambChangeFunc);
            InputField ambRotZInputField = Panel.transform.Find("ambRotZInputField").GetComponent<InputField>();
            ambRotZInputField.text = amb.dir.z.ToString();
            floatRotZ = amb.dir.z;
            ambRotZInputField.onEndEdit.AddListener(EnterPressed);
            ambRotZInputField.onValueChanged.AddListener(ambChangeFunc);

            InputField ambDirXInputField = Panel.transform.Find("ambDirXInputField").GetComponent<InputField>();
            ambDirXInputField.text = amb.joint_dir.x.ToString();
            floatDirX = amb.joint_dir.x;
            ambDirXInputField.onEndEdit.AddListener(EnterPressed);
            ambDirXInputField.onValueChanged.AddListener(ambChangeFunc);
            InputField ambDirYInputField = Panel.transform.Find("ambDirYInputField").GetComponent<InputField>();
            ambDirYInputField.text = amb.joint_dir.y.ToString();
            floatDirY = amb.joint_dir.y;
            ambDirYInputField.onEndEdit.AddListener(EnterPressed);
            ambDirYInputField.onValueChanged.AddListener(ambChangeFunc);
            InputField ambDirZInputField = Panel.transform.Find("ambDirZInputField").GetComponent<InputField>();
            ambDirZInputField.text = amb.joint_dir.z.ToString();
            floatDirZ = amb.joint_dir.z;
            ambDirZInputField.onEndEdit.AddListener(EnterPressed);
            ambDirZInputField.onValueChanged.AddListener(ambChangeFunc);

            InputField ambPerInputField = Panel.transform.Find("ambPerInputField").GetComponent<InputField>();
            ambPerInputField.text = amb.per.ToString();
            floatPer = amb.per;
            ambPerInputField.onEndEdit.AddListener(EnterPressed);
            ambPerInputField.onValueChanged.AddListener(ambChangeFunc);

            InputField ambKasenchuPerInputField = Panel.transform.Find("ambKasenchuPerInputField").GetComponent<InputField>();
            ambKasenchuPerInputField.text = amb.size_per.ToString();
            floatKasenchuPer = amb.size_per;
            if (mMain.mAMBMgr.keyList.Contains(ambObjJointMdl.Name.ToUpper()))
            {
                ambKasenchuPerInputField.onEndEdit.AddListener(EnterPressed);
                ambKasenchuPerInputField.onValueChanged.AddListener(ambChangeFunc);
            }
            else
            {
                ambKasenchuPerInputField.interactable = false;
            }

            ambPosXInputField.Select();
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

    void ambChangeFunc(string text)
    {
        InputField ambPosXInputField = Panel.transform.Find("ambPosXInputField").GetComponent<InputField>();
        InputField ambPosYInputField = Panel.transform.Find("ambPosYInputField").GetComponent<InputField>();
        InputField ambPosZInputField = Panel.transform.Find("ambPosZInputField").GetComponent<InputField>();
        InputField ambRotXInputField = Panel.transform.Find("ambRotXInputField").GetComponent<InputField>();
        InputField ambRotYInputField = Panel.transform.Find("ambRotYInputField").GetComponent<InputField>();
        InputField ambRotZInputField = Panel.transform.Find("ambRotZInputField").GetComponent<InputField>();
        InputField ambDirXInputField = Panel.transform.Find("ambDirXInputField").GetComponent<InputField>();
        InputField ambDirYInputField = Panel.transform.Find("ambDirYInputField").GetComponent<InputField>();
        InputField ambDirZInputField = Panel.transform.Find("ambDirZInputField").GetComponent<InputField>();
        InputField ambPerInputField = Panel.transform.Find("ambPerInputField").GetComponent<InputField>();
        InputField ambKasenchuPerInputField = Panel.transform.Find("ambKasenchuPerInputField").GetComponent<InputField>();
        string ambPosX = ambPosXInputField.text;
        string ambPosY = ambPosYInputField.text;
        string ambPosZ = ambPosZInputField.text;
        string ambRotX = ambRotXInputField.text;
        string ambRotY = ambRotYInputField.text;
        string ambRotZ = ambRotZInputField.text;
        string ambDirX = ambDirXInputField.text;
        string ambDirY = ambDirYInputField.text;
        string ambDirZ = ambDirZInputField.text;
        string ambPer = ambPerInputField.text;
        string ambKasenchuPer = ambKasenchuPerInputField.text;
        if ("".Equals(ambPosX) || "".Equals(ambPosY) || "".Equals(ambPosZ) 
            || "".Equals(ambRotX) || "".Equals(ambRotY) || "".Equals(ambRotZ)
            || "".Equals(ambDirX) || "".Equals(ambDirY) || "".Equals(ambDirZ)
            || "".Equals(ambPer) || "".Equals(ambKasenchuPer))
        {
            OkButton.interactable = false;
            return;
        }
        try
        {
            floatPosX = float.Parse(ambPosX);
            floatPosY = float.Parse(ambPosY);
            floatPosZ = float.Parse(ambPosZ);
            floatRotX = float.Parse(ambRotX);
            floatRotY = float.Parse(ambRotY);
            floatRotZ = float.Parse(ambRotZ);
            floatDirX = float.Parse(ambDirX);
            floatDirY = float.Parse(ambDirY);
            floatDirZ = float.Parse(ambDirZ);
            floatPer = float.Parse(ambPer);
            floatKasenchuPer = float.Parse(ambKasenchuPer);
        }
        catch (System.Exception)
        {
            OkButton.interactable = false;
            return;
        }
        if (floatPer == 0 || floatKasenchuPer == 0)
        {
            OkButton.interactable = false;
            return;
        }
        OkButton.interactable = true;
        int editAmbNo = mMain.mAMBMgr.search_amb_no;
        int editAmbIndex = mMain.mAMBMgr.search_amb_index;
        GameObject ambObj = mMain.mAMBMgr.ambObjList[editAmbNo][editAmbIndex];
        JointMdl ambObjJointMdl = ambObj.GetComponent<JointMdl>();
        ambObjJointMdl.BasePos = new Vector3(floatPosX, floatPosY, floatPosZ);
        ambObjJointMdl.BaseRot = new Vector3(floatRotX, floatRotY, floatRotZ);
        ambObjJointMdl.JointDir = new Vector3(floatDirX, floatDirY, floatDirZ);
        ambObjJointMdl.LengthPer = floatPer;

        if (mMain.mAMBMgr.keyList.Contains(ambObjJointMdl.Name.ToUpper()))
        {
            AmbMdl ambMdl = ambObj.GetComponent<AmbMdl>();
            ambMdl.KasenChuScale = floatKasenchuPer;
            mMain.mAMBMgr.SetScale(ambObj, ambMdl.KasenChuScale, ambObjJointMdl.Name.ToUpper(), mMain.mAMBMgr.size_per_dict[ambObjJointMdl.Name.ToUpper()]);
        }
        ambObjJointMdl.UpdateOffsetPos();
        ambObjJointMdl.UpdateBaseRot(true);
        ambObjJointMdl.UpdateJointDir();
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
                    "\n"
                };
                string[] originArray = fileContent.Split(separator, System.StringSplitOptions.None);
                for (int i = 0; i < originArray.Length; i++)
                {
                    originArray[i] = originArray[i].Trim('\r');
                }

                int ambDataTxtIndex = mMain.mStageTblMgr.getAmbDataTxtIndex(mMain.mAMBMgr.search_amb_no, fileContent, originArray, mMain);
                if (ambDataTxtIndex == -1)
                {
                    MessageBox.Show("テキストの書込失敗！\nエラーを確認してください", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                string originAmbData = originArray[ambDataTxtIndex];
                string[] separator2 = new string[]
                {
                    "\r",
                    "\t"
                };
                string[] collection = originAmbData.Split(separator2, System.StringSplitOptions.RemoveEmptyEntries);
                collection[6 + mMain.mAMBMgr.search_amb_index * 13] = floatPosX.ToString();
                collection[7 + mMain.mAMBMgr.search_amb_index * 13] = floatPosY.ToString();
                collection[8 + mMain.mAMBMgr.search_amb_index * 13] = floatPosZ.ToString();
                collection[9 + mMain.mAMBMgr.search_amb_index * 13] = floatRotX.ToString();
                collection[10 + mMain.mAMBMgr.search_amb_index * 13] = floatRotY.ToString();
                collection[11 + mMain.mAMBMgr.search_amb_index * 13] = floatRotZ.ToString();
                collection[12 + mMain.mAMBMgr.search_amb_index * 13] = floatDirX.ToString();
                collection[13 + mMain.mAMBMgr.search_amb_index * 13] = floatDirY.ToString();
                collection[14 + mMain.mAMBMgr.search_amb_index * 13] = floatDirZ.ToString();
                collection[15 + mMain.mAMBMgr.search_amb_index * 13] = floatPer.ToString();
                collection[16 + mMain.mAMBMgr.search_amb_index * 13] = floatKasenchuPer.ToString();

                originArray[ambDataTxtIndex] = string.Join("\t", collection);
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
                    mMain.mFileReadMgr.StagedataRead(mMain, fileContent, false, true, true);
                }
            }
            else if (".xlsx".Equals(fileExt))
            {
                if (mMain.excelAmbNewLineFlag)
                {
                    int rowNum = mMain.mFileReadMgr.excelAmbIndexMap[mMain.mAMBMgr.search_amb_no];
                    var row = iSheet.GetRow(rowNum + mMain.mAMBMgr.search_amb_index);
                    row.GetCell(6).SetCellValue(floatPosX);
                    row.GetCell(7).SetCellValue(floatPosY);
                    row.GetCell(8).SetCellValue(floatPosZ);
                    row.GetCell(9).SetCellValue(floatRotX);
                    row.GetCell(10).SetCellValue(floatRotY);
                    row.GetCell(11).SetCellValue(floatRotZ);
                    row.GetCell(12).SetCellValue(floatDirX);
                    row.GetCell(13).SetCellValue(floatDirY);
                    row.GetCell(14).SetCellValue(floatDirZ);
                    row.GetCell(15).SetCellValue(floatPer);
                    row.GetCell(16).SetCellValue(floatKasenchuPer);
                }
                else
                {
                    var row = iSheet.GetRow(mMain.excelAmbFirstRowNum + mMain.mAMBMgr.search_amb_no);
                    row.GetCell(6 + mMain.mAMBMgr.search_amb_index * 13).SetCellValue(floatPosX);
                    row.GetCell(7 + mMain.mAMBMgr.search_amb_index * 13).SetCellValue(floatPosY);
                    row.GetCell(8 + mMain.mAMBMgr.search_amb_index * 13).SetCellValue(floatPosZ);
                    row.GetCell(9 + mMain.mAMBMgr.search_amb_index * 13).SetCellValue(floatRotX);
                    row.GetCell(10 + mMain.mAMBMgr.search_amb_index * 13).SetCellValue(floatRotY);
                    row.GetCell(11 + mMain.mAMBMgr.search_amb_index * 13).SetCellValue(floatRotZ);
                    row.GetCell(12 + mMain.mAMBMgr.search_amb_index * 13).SetCellValue(floatDirX);
                    row.GetCell(13 + mMain.mAMBMgr.search_amb_index * 13).SetCellValue(floatDirY);
                    row.GetCell(14 + mMain.mAMBMgr.search_amb_index * 13).SetCellValue(floatDirZ);
                    row.GetCell(15 + mMain.mAMBMgr.search_amb_index * 13).SetCellValue(floatPer);
                    row.GetCell(16 + mMain.mAMBMgr.search_amb_index * 13).SetCellValue(floatKasenchuPer);
                }

                filePath = mMain.openFilename;
                if (mMain.editModeFlag)
                {
                    string excelFileName = Path.GetFileNameWithoutExtension(mMain.openFilename);
                    string excelFileExt = Path.GetExtension(mMain.openFilename);
                    newFilePath = Path.Combine(Path.GetDirectoryName(mMain.openFilename), excelFileName + "_new" + excelFileExt);
                    reloadFilePath = newFilePath;
                }
                else
                {
                    reloadFilePath = filePath;
                }

                try
                {
                    using(var fs = new FileStream(reloadFilePath, FileMode.Create, FileAccess.Write))
                    {
                        xssWorkbook.Write(fs);
                    }
                }
                catch (System.IO.IOException)
                {
                    MessageBox.Show("書込みエラー！\n権限問題の可能性があります", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("書込み中、予想外のエラー！\nエラーを確認してください", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Debug.Log(ex.ToString());
                    return;
                }

                foreach (Transform child in dialogObject.transform)
                {
                    UnityEngine.Object.Destroy(child.gameObject);
                }
                UnityEngine.Object.Destroy(dialogObject);
                dialogObject = null;
                DefaultPanel.gameObject.SetActive(true);
                mCameraMgr.moveFlag = true;

                mMain.mFileReadMgr.xlsxRead(mMain, reloadFilePath, false, true, true);
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
        int editAmbNo = mMain.mAMBMgr.search_amb_no;
        int editAmbIndex = mMain.mAMBMgr.search_amb_index;
        amb_data amb = mMain.mStageTblMgr.AmbList[editAmbNo].datalist[editAmbIndex];
        GameObject ambObj = mMain.mAMBMgr.ambObjList[editAmbNo][editAmbIndex];
        JointMdl ambObjJointMdl = ambObj.GetComponent<JointMdl>();
        ambObjJointMdl.BasePos = amb.offsetpos;
        ambObjJointMdl.BaseRot = amb.dir;
        ambObjJointMdl.JointDir = amb.joint_dir;
        ambObjJointMdl.LengthPer = amb.per;

        if (mMain.mAMBMgr.keyList.Contains(ambObjJointMdl.Name.ToUpper()))
        {
            AmbMdl ambMdl = ambObj.GetComponent<AmbMdl>();
            ambMdl.KasenChuScale = amb.size_per;
            mMain.mAMBMgr.SetScale(ambObj, ambMdl.KasenChuScale, ambObjJointMdl.Name.ToUpper(), mMain.mAMBMgr.size_per_dict[ambObjJointMdl.Name.ToUpper()]);
        }
        ambObjJointMdl.UpdateOffsetPos();
        ambObjJointMdl.UpdateBaseRot(true);
        ambObjJointMdl.UpdateJointDir();

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
