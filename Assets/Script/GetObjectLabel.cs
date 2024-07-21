using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using JointMdlClass;
using RailMdlClass;

namespace GetObjectLabelClass
{
    public class GetObjectLabel : MonoBehaviour
    {
        private Camera mCamera;
        private Color mBackColor = new Color(0,0,0,64);
        private GUIStyle mStyle = null;
        string mText1;
        private Vector2 mSize1;

        Canvas canvas;
        Transform CanvasTr;
        Toggle railCheckToggle;
        Toggle railLabelToggle;

        private void Start()
        {
            int FontSize = 12;
            bool Bold = false;

            mCamera = Camera.main;

            mStyle = new GUIStyle();
            Texture2D whiteTex = new Texture2D(1, 1);
            whiteTex.SetPixel(0, 0, Color.white);
            mStyle.normal.background = whiteTex;
            mStyle.normal.textColor = Color.white ;
            mStyle.fontSize = FontSize;
            mStyle.fontStyle = Bold ? FontStyle.Bold : FontStyle.Normal;
            mStyle.fixedWidth = 0;
            mStyle.fixedHeight = 0;
            mStyle.padding = new RectOffset(6, 6, 5, 5);
            mStyle.wordWrap = true;
            mStyle.alignment = TextAnchor.UpperLeft;

            canvas = FindCanvasClass();
            CanvasTr = canvas.transform;
            railCheckToggle = CanvasTr.Find("railCheckToggle").GetComponent<Toggle>();
            railLabelToggle = CanvasTr.Find("railLabelToggle").GetComponent<Toggle>();
        }

        Canvas FindCanvasClass()
        {
            var findObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var findObject in findObjects)
            {
                if (findObject.name == "Canvas") {
                    return findObject.GetComponent<Canvas>();
                }
            }
            return null;
        }

        void OnGUI()
        {
            if (mCamera == null) {
                return;
            }
            if (!railLabelToggle.isOn) {
                return;
            }

            JointMdl jointMdl = gameObject.GetComponent<JointMdl>();
            Transform[] jointList = jointMdl.JointList;
            Transform trans1 = jointList[1];

            RailMdl railMdl = gameObject.GetComponent<RailMdl>();
            mText1 = "レールNo." + railMdl.railNum.ToString();
            if (railMdl.railCheck)
            {
                mText1 += "\nNextレール：[";
                for (int i = 0; i < railMdl.nextRail.Count; i++)
                {
                    List<int[]> nextRailList = railMdl.nextRail[i];
                    for (int j = 0; j < nextRailList.Count; j++)
                    {
                        mText1 += "(" + nextRailList[j][0].ToString() + ", " + nextRailList[j][1].ToString() + ")";
                        if (j < nextRailList.Count - 1)
                        {
                            mText1 += ", ";
                        }
                    }
                    if (i < railMdl.nextRail.Count - 1)
                    {
                        mText1 += " / ";
                    }
                }
                mText1 += "]\n";
                mText1 += "Prevレール：[";
                for (int i = 0; i < railMdl.prevRail.Count; i++)
                {
                    List<int[]> prevRailList = railMdl.prevRail[i];
                    for (int j = 0; j < prevRailList.Count; j++)
                    {
                        mText1 += "(" + prevRailList[j][0].ToString() + ", " + prevRailList[j][1].ToString() + ")";
                        if (j < prevRailList.Count - 1)
                        {
                            mText1 += ", ";
                        }
                    }
                    if (i < railMdl.prevRail.Count - 1)
                    {
                        mText1 += " / ";
                    }
                }
                mText1 += "]";
            }
            Transform rail0Trans = jointMdl.JointList[0];
            Transform railLastTrans = jointMdl.LastTrans;
            mText1 += "\nDIR  0番:" + rail0Trans.rotation.eulerAngles.x.ToString() + ", " + rail0Trans.rotation.eulerAngles.y.ToString() + ", " + rail0Trans.rotation.eulerAngles.z.ToString();
            mText1 += "\nDIR 末番:" + railLastTrans.rotation.eulerAngles.x.ToString() + ", " + railLastTrans.rotation.eulerAngles.y.ToString() + ", " + railLastTrans.rotation.eulerAngles.z.ToString();
            mSize1 = mStyle.CalcSize(new GUIContent(mText1));

            Vector2 guiPosition = mCamera.WorldToScreenPoint(trans1.position);
            guiPosition.y = Screen.height - guiPosition.y;
            GUI.backgroundColor = mBackColor;
            if (!string.IsNullOrEmpty(mText1)) GUI.Label(new Rect(guiPosition.x, guiPosition.y, mSize1.x, mSize1.y), mText1, mStyle);
        }
    }
}