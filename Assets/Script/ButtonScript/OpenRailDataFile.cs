﻿using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.UI;
using UnityButton = UnityEngine.UI.Button;
using System.IO;

using MainClass;
using AMBMgrClass;
using AmbDataClass;
using JointMdlClass;

public class OpenRailDataFile : MonoBehaviour
{
    UnityButton button;
    Main mMain;

    void Start()
    {
        button = GetComponent<UnityButton>();
        button.onClick.AddListener(openFileDialogFunc);
        mMain = FindMainClass();
    }

    void openFileDialogFunc()
    {
        OpenFileDialog ofd = new OpenFileDialog();
        ofd.InitialDirectory = mMain.defaultPath;
        ofd.Filter =  "txt files (*.txt)|*.txt";
        
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            string filePath = ofd.FileName;
            mMain.defaultPath = Path.GetDirectoryName(filePath);
            string fileContent = string.Empty;
            var fileStream = ofd.OpenFile();
            using (StreamReader reader = new StreamReader(fileStream))
            {
                fileContent = reader.ReadToEnd();
            }
            bool result = mMain.mStageTblMgr.Open(fileContent, mMain);
            if (!result)
            {
                MessageBox.Show("読込失敗！\nエラーを確認してください", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                mMain.SetDeactiveButton();
            }
            else
            {
                mMain.SetActiveButton();
                mMain.SetDrawModel(true, true);
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
}