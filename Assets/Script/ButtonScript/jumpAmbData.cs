using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityButton = UnityEngine.UI.Button;
using System.IO;

using MainClass;
using JointMdlClass;
using CameraMgrClass;
using GetObjectLabelClass;

public class jumpAmbData : MonoBehaviour
{
    GameObject dialogObject = null;
    UnityButton button;
    UnityButton editRailButton;
    UnityButton editAmbButton;
    Transform CanvasTr;
    Transform DefaultPanel;
    Transform Panel;
    Main mMain;
    CameraMgr mCameraMgr;

    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<UnityButton>();
        button.onClick.AddListener(jumpRailDataFunc);
        button.interactable = false;

        CanvasTr = GameObject.Find("Canvas").transform;
        DefaultPanel = CanvasTr.Find("DefaultPanel").transform;
        editRailButton = DefaultPanel.Find("editRailButton").GetComponent<UnityButton>();
        editAmbButton = DefaultPanel.Find("editAmbButton").GetComponent<UnityButton>();
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

    void jumpRailDataFunc()
    {
        string dialogPath = "dialogPrefab/inputAmbDialogPanel";
        try
        {
            dialogObject = (UnityEngine.Object.Instantiate(Resources.Load(dialogPath)) as GameObject);
            dialogObject.name = dialogObject.name.Replace("(Clone)", "");
            dialogObject.transform.SetParent(DefaultPanel, false);
            dialogObject.transform.localScale = Vector3.one;
            Panel = dialogObject.transform.Find("Panel").GetComponent<Transform>();
            Panel.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
            Panel.transform.Find("OkButton").GetComponent<UnityButton>().onClick.AddListener(ClickOkButton);
            Panel.transform.Find("CancelButton").GetComponent<UnityButton>().onClick.AddListener(ClickCancelButton);

            InputField inputField = Panel.transform.Find("InputField").GetComponent<InputField>();
            inputField.onEndEdit.AddListener(EnterPressed);
            InputField inputField2 = Panel.transform.Find("InputField2").GetComponent<InputField>();
            inputField2.onEndEdit.AddListener(EnterPressed);

            mCameraMgr.moveFlag = false;
            button.interactable = false;

            inputField.Select();
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

    void ClickOkButton()
    {
        Text errorText = Panel.transform.Find("ErrorText").GetComponent<Text>();
        InputField inputField = Panel.transform.Find("InputField").GetComponent<InputField>();
        InputField inputField2 = Panel.transform.Find("InputField2").GetComponent<InputField>();
        string ambValue = inputField.transform.Find("Text").GetComponent<Text>().text;
        string ambIndex = inputField2.transform.Find("Text").GetComponent<Text>().text;
        try
        {
            if (!"".Equals(ambValue))
            {
                if ("".Equals(ambIndex))
                {
                    errorText.text = "index番号を入力してください";
                    return;
                }
                int intVal;
                try
                {
                    intVal = int.Parse(ambValue);
                    if (intVal < 0 || intVal >= mMain.mAMBMgr.ambObjList.Length)
                    {
                        errorText.text = "存在しないAMB番号(" + intVal + ")";
                        return;
                    }
                    if (mMain.mAMBMgr.ambObjList[intVal] == null)
                    {
                        errorText.text = "描画されてないAMB番号(" + intVal + ")";
                        return;
                    }
                    errorText.text = "";
                }
                catch (System.Exception)
                {
                    errorText.text = "AMB番号を数字で入力してください";
                    return;
                }

                int intIndex;
                try
                {
                    intIndex = int.Parse(ambIndex);
                    if (intIndex < 0)
                    {
                        errorText.text = "index番号は0以上です(" + intIndex + ")";
                        return;
                    }
                    if (intIndex >= mMain.mAMBMgr.ambObjList[intVal].Count)
                    {
                        errorText.text = "描画されてないAMBモデル(" + intVal + ", " + intIndex + ")";
                        return;
                    }
                    errorText.text = "";
                }
                catch (System.Exception)
                {
                    errorText.text = "AMBのindexを数字で入力してください";
                    return;
                }

                foreach (Transform child in dialogObject.transform)
                {
                    UnityEngine.Object.Destroy(child.gameObject);
                }
                UnityEngine.Object.Destroy(dialogObject);
                dialogObject = null;
                mCameraMgr.moveFlag = true;
                button.interactable = true;

                GameObject ambObj = mMain.mAMBMgr.ambObjList[intVal][intIndex];
                JointMdl ambObjJointMdl = ambObj.GetComponent<JointMdl>();

                Vector3 curPos;
                Vector3 curRot;
                if (ambObjJointMdl.BaseJoint == null)
                {
                    curPos = ambObjJointMdl.JointList[0].transform.position;
                    curRot = ambObjJointMdl.JointList[0].transform.eulerAngles;
                }
                else
                {
                    curPos = ambObjJointMdl.BaseJoint.transform.position;
                    curRot = ambObjJointMdl.BaseJoint.transform.eulerAngles;
                }
                curRot.z = 0;
                mCameraMgr.mainCamObj.transform.position = curPos;
                mCameraMgr.mainCamObj.transform.eulerAngles = curRot;

                mCameraMgr.mainCamObj.transform.position += mCameraMgr.mainCamObj.transform.up * 0.1f;
                mCameraMgr.mainCamObj.transform.position -= mCameraMgr.mainCamObj.transform.forward * 0.4f;

                mMain.InitRailObjOutlineAndLabel();
                mMain.InitAmbObjOutlineAndLabel();
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
                mMain.mAMBMgr.search_amb_no = intVal;
                mMain.mAMBMgr.search_amb_index = intIndex;

                GameObject editRailButtonObj = editRailButton.gameObject;
                editRailButtonObj.SetActive(false);
                GameObject editAmbButtonObj = editAmbButton.gameObject;
                editAmbButtonObj.SetActive(false);
                string fileExt = Path.GetExtension(mMain.openFilename).ToLower();
                if (".txt".Equals(fileExt))
                {
                    editAmbButtonObj.SetActive(true);
                    Text editAmbButtonText = editAmbButton.transform.Find("Text").GetComponent<Text>();
                    editAmbButtonText.text = "AMB No.[" + intVal + "-" + intIndex + "] の修正";
                }
            }
        }
        catch (System.Exception ex)
        {
            mMain.DebugError("予想外のエラー");
            mMain.DebugError(ex.ToString());
        }
    }

    void ClickCancelButton()
    {
        foreach (Transform child in dialogObject.transform)
        {
            UnityEngine.Object.Destroy(child.gameObject);
        }
        UnityEngine.Object.Destroy(dialogObject);
        dialogObject = null;
        mCameraMgr.moveFlag = true;
        button.interactable = true;
    }
}
