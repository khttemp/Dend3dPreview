using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using MainClass;

public class editRailDialogTab : MonoBehaviour
{
    Transform CanvasTr;
    Transform EditRailPanel;
    Transform Panel;
    Selectable railDirXSelectable;
    Selectable railPerSelectable;
    Main mMain;

    void Start()
    {
        CanvasTr = GameObject.Find("Canvas").transform;
        EditRailPanel = CanvasTr.Find("editRailPanel").transform;
        Panel = EditRailPanel.transform.Find("Panel").GetComponent<Transform>();
        railDirXSelectable = Panel.transform.Find("railDirXInputField").GetComponent<Selectable>();
        railPerSelectable = Panel.transform.Find("railPerInputField").GetComponent<Selectable>();
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
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
                    next = current.FindSelectableOnUp();
                    if (next == null)
                    {
                        next = railPerSelectable;
                    }
                } else {
                    next = current.FindSelectableOnDown();
                    if (next == null)
                    {
                        next = railDirXSelectable;
                    }
                }
            }
            
            if (next != null)  {
                next.Select();
            }
        }
    }
}