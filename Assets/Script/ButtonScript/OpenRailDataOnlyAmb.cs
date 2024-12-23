﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityButton = UnityEngine.UI.Button;

using MainClass;
using FileReadMgrClass;

public class OpenRailDataOnlyAmb : MonoBehaviour
{
    UnityButton button;
    Main mMain;

    void Start()
    {
        button = GetComponent<UnityButton>();
        button.onClick.AddListener(openRailDataOnlyAmbFunc);
        button.interactable = false;
        mMain = FindMainClass();
    }

    void openRailDataOnlyAmbFunc()
    {
        mMain.mFileReadMgr.Read(mMain, false, true);
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
