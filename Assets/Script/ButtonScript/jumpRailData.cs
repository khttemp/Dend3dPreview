using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityButton = UnityEngine.UI.Button;

using MainClass;
using JointMdlClass;
using CameraMgrClass;
using GetObjectLabelClass;

public class jumpRailData : MonoBehaviour
{
    GameObject dialogObject = null;
    UnityButton button;
    UnityButton openRailButton;
    Transform CanvasTr;
    Main mMain;
    CameraMgr mCameraMgr;

    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<UnityButton>();
        button.onClick.AddListener(jumpRailDataFunc);
        button.interactable = false;

        CanvasTr = GameObject.Find("Canvas").transform;
        openRailButton = CanvasTr.Find("openRailButton").GetComponent<UnityButton>();
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
        string dialogPath = "dialogPrefab/inputDialogPanel";
        try
        {
            dialogObject = (UnityEngine.Object.Instantiate(Resources.Load(dialogPath)) as GameObject);
            dialogObject.name = dialogObject.name.Replace("(Clone)", "");
            dialogObject.transform.SetParent(CanvasTr, false);
            dialogObject.transform.localScale = Vector3.one;
            dialogObject.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
            dialogObject.transform.Find("OkButton").GetComponent<UnityButton>().onClick.AddListener(ClickOkButton);
            dialogObject.transform.Find("CancelButton").GetComponent<UnityButton>().onClick.AddListener(ClickCancelButton);

            InputField inputField = dialogObject.transform.Find("InputField").GetComponent<InputField>();
            inputField.onEndEdit.AddListener(EnterPressed);

            mCameraMgr.moveFlag = false;
            button.interactable = false;
            openRailButton.interactable = false;
        }
        catch(System.Exception)
        {

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
        Text errorText = dialogObject.transform.Find("ErrorText").GetComponent<Text>();
        InputField inputField = dialogObject.transform.Find("InputField").GetComponent<InputField>();
        string value = inputField.transform.Find("Text").GetComponent<Text>().text;
        try
        {
            if (!"".Equals(value))
            {
                int intVal;
                try
                {
                    intVal = int.Parse(value);
                    if (intVal < 0 || intVal >= mMain.mRailMgr.railObjList.Count)
                    {
                        errorText.text = "存在しないレール番号(" + intVal + ")";
                        return;
                    }
                    if ((mMain.mStageTblMgr.RailList[intVal].flg & (1U << 31)) > 0)
                    {
                        errorText.text = "無効レール番号(" + intVal + ")";
                        return;
                    }
                    errorText.text = "";
                }
                catch (System.Exception)
                {
                    errorText.text = "数字で入力してください";
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
                openRailButton.interactable = true;

                GameObject railObj = mMain.mRailMgr.railObjList[intVal];
                JointMdl railObjJointMdl = railObj.GetComponent<JointMdl>();

                Vector3 curPos;
                Vector3 curRot;
                if (railObjJointMdl.BaseJoint == null)
                {
                    curPos = railObjJointMdl.JointList[0].transform.position;
                    curRot = railObjJointMdl.JointList[0].transform.eulerAngles;
                }
                else
                {
                    curPos = railObjJointMdl.BaseJoint.transform.position;
                    curRot = railObjJointMdl.BaseJoint.transform.eulerAngles;
                }
                curRot.z = 0;
                mCameraMgr.mainCamObj.transform.position = curPos;
                mCameraMgr.mainCamObj.transform.eulerAngles = curRot;

                mCameraMgr.mainCamObj.transform.position += mCameraMgr.mainCamObj.transform.up * 0.1f;
                mCameraMgr.mainCamObj.transform.position -= mCameraMgr.mainCamObj.transform.forward * 0.4f;

                int search_rail_index = mMain.mRailMgr.search_rail_index;
                if (search_rail_index != intVal)
                {
                    if (search_rail_index != -1)
                    {
                        GameObject delObj = mMain.mRailMgr.railObjList[search_rail_index];
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
                }
                mMain.mRailMgr.search_rail_index = intVal;
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log("予想外のエラー！");
            Debug.Log(ex.ToString());
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
        openRailButton.interactable = true;
    }
}
