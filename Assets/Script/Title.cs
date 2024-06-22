using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class Title : MonoBehaviour
{
//Windowsのみに限定
#if UNITY_STANDALONE_WIN
    [DllImport("user32.dll", EntryPoint = "SetWindowText")]
    public static extern bool SetWindowText(System.IntPtr hwnd, System.String lpString);
    [DllImport("user32.dll", EntryPoint = "FindWindow")]
    public static extern System.IntPtr FindWindow(System.String className, System.String windowName);
    private void Start()
    {
        //Product NameのWindowを探す
        var windowPtr = FindWindow(null, Application.productName);
        //名前をセットする
        SetWindowText(windowPtr, Application.productName + " " + Application.version);
    }
#endif
}
