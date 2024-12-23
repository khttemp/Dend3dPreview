using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using MainClass;

public class inputAmbDialogTab : MonoBehaviour
{
    Transform CanvasTr;
    Transform DefaultPanel;
    Transform EditRailPanel;
    Transform Panel;
    Selectable inputFieldSelectable;
    Selectable inputField2Selectable;
    Main mMain;

    void Start()
    {
        CanvasTr = GameObject.Find("Canvas").transform;
        DefaultPanel = CanvasTr.Find("DefaultPanel").transform;
        EditRailPanel = DefaultPanel.Find("inputAmbDialogPanel").transform;
        Panel = EditRailPanel.Find("Panel").GetComponent<Transform>();
        inputFieldSelectable = Panel.transform.Find("InputField").GetComponent<Selectable>();
        inputField2Selectable = Panel.transform.Find("InputField2").GetComponent<Selectable>();
        mMain = FindMainClass();
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

    void Update()
    {
        // When TAB is pressed, we should select the next selectable UI element
        if (Input.GetKeyDown(KeyCode.Tab)) {
            Selectable next = null;
            Selectable current = null;

            // Figure out if we have a valid current selected gameobject
            if (EventSystem.current.currentSelectedGameObject != null) {
                // Unity doesn't seem to "deselect" an object that is made inactive
                if (EventSystem.current.currentSelectedGameObject.activeInHierarchy) {
                    current = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();
                }
            }
            
            if (current != null) {
                // When SHIFT is held along with tab, go backwards instead of forwards
                if (current.Equals(inputFieldSelectable))
                {
                    next = inputField2Selectable;
                }
                else if (current.Equals(inputField2Selectable))
                {
                    next = inputFieldSelectable;
                }
            }
            
            if (next != null)  {
                next.Select();
            }
        }
    }
}