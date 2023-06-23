using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectionManager : MonoBehaviour
{
    public string selectableTag = "Selectable";
    public Camera Camera_Environment;

    private Transform _selection; // current selection
    void Update()
    {   
        // if selectable object already selected
        if (_selection != null)
        {
            // if left click once, control the object
            if (Input.GetMouseButtonDown(0))
            {
                Camera_Environment.GetComponent<SmoothMouseLook>().enabled = false;
                _selection.transform.parent.GetComponent<SmoothMouseLook>().enabled = true;
            }
            
            // if click right, cube disappears
            if (Input.GetMouseButtonDown(1))
            {
                _selection.gameObject.SetActive(false);
            }

            // if click right twice, cube reappears
            if (Input.GetMouseButtonUp(1))
            {
                _selection.gameObject.SetActive(true);
            }
            
            // if left click twice, control the camera
            if (Input.GetMouseButtonUp(0))
            {
                _selection.transform.parent.GetComponent<SmoothMouseLook>().enabled = false;
                Camera_Environment.GetComponent<SmoothMouseLook>().enabled = true;
                _selection = null;
            }
        }

        var ray = Camera_Environment.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f)); // ray from Crosshair
        RaycastHit hit;
        // if new object with selectable tag hit, it becomes the current selection
        if (Physics.Raycast(ray, out hit)) 
        {
            var selection = hit.transform;
            if (selection.CompareTag(selectableTag))
            {
                _selection = selection;
            }
        }
    }
}