using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityButton = UnityEngine.UI.Button;

using MainClass;
using JointMdlClass;
using CameraMgrClass;

public class jumpRailData : MonoBehaviour
{
    GameObject dialogObject = null;
    UnityButton button;
    UnityButton openRailButton;
    Transform CanvasTr;
    Main mMain;
    CameraMgr mCameraMgr;
    Vector3 adjustVector = new Vector3(0.1f, 0.15f, -0.4f);
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

    // Update is called once per frame
    void Update()
    {
        
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
                int intVal = int.Parse(value);
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

                Vector3 curRot;
                if (railObjJointMdl.BaseJoint == null)
                {
                    mCameraMgr.mainCamObj.transform.position = railObjJointMdl.JointList[0].transform.position;
                    curRot = railObjJointMdl.JointList[0].transform.eulerAngles;
                }
                else
                {
                    mCameraMgr.mainCamObj.transform.position = railObjJointMdl.BaseJoint.transform.position;
                    curRot = railObjJointMdl.BaseJoint.transform.eulerAngles;
                }
                curRot.z = 0;
                mCameraMgr.mainCamObj.transform.eulerAngles = curRot;
            }
        }
        catch (System.Exception)
        {
            errorText.text = "数字で入力してください";
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
