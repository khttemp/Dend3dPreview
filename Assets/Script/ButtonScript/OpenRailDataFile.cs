using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityButton = UnityEngine.UI.Button;

using MainClass;
using FileReadMgrClass;

public class OpenRailDataFile : MonoBehaviour
{
    UnityButton button;
    Main mMain;
    FileReadMgr mFileReadMgr;

    void Start()
    {
        button = GetComponent<UnityButton>();
        button.onClick.AddListener(openFileDialogFunc);
        mMain = FindMainClass();
        mFileReadMgr = new FileReadMgr();
    }

    void openFileDialogFunc()
    {
        mFileReadMgr.Read(mMain, true, true);
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
}