using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CameraDirector : MonoBehaviour
{
    private GameObject UI_viewMode;
    private GameObject targetVehicle;
    private string vehicleID_str;
    private Camera main_cam;

    void Start()
    {
        main_cam =  this.GetComponent<Camera>();
        UI_viewMode = GameObject.Find("ViewMode");
    }

    void Update()
    {
        bool IsEntered = getKeyboardNumberInput();
        bool IsExited = Input.GetKeyDown(KeyCode.Escape);
        
        if (IsEntered)
        {
            try{
                string targetVehicleName = "vehicle_no." +
                                        int.Parse(vehicleID_str) +
                                        "_real";
                targetVehicle = GameObject.Find(targetVehicleName);
                setShoulderView(targetVehicle);
                setShoulderView_viewmodeUI(vehicleID_str);
            }
            catch (System.Exception)
            {
                // Debug.Log(e.ToString());
            }
            vehicleID_str = null;
        }

        if (IsExited)
        {
            setOverView();
        }
    }

    void setShoulderView(GameObject targetVehicle)
    {
        this.transform.SetParent(targetVehicle.transform);
        this.transform.localPosition = new Vector3(0, 3, -6);
        this.transform.localRotation = UnityEngine.Quaternion.Euler(
            15, 0, 0);
    }

    void setShoulderView_viewmodeUI(string vehicleID_str)
    {
        int vehicleID = int.Parse(vehicleID_str);
        UI_viewMode.GetComponent<TextMeshProUGUI>().text = "<Vehicle " + vehicleID + ">";
    }

    void setOverView()
    {
        this.transform.parent = null;
        this.transform.localPosition = new Vector3(150, 350, 158);
        this.transform.localRotation = UnityEngine.Quaternion.Euler(
            90, 0, 0);
        UI_viewMode.GetComponent<TextMeshProUGUI>().text = "Overview";
    }

    bool getKeyboardNumberInput()
    {
        if (Input.GetKeyDown(KeyCode.KeypadEnter) ||
            Input.GetKeyDown(KeyCode.Return))
        { return true; }

        else
        {
            if (Input.GetKeyDown(KeyCode.Keypad0) ||
                Input.GetKeyDown("0"))
            { vehicleID_str += "0"; }
            if (Input.GetKeyDown(KeyCode.Keypad1) ||
                Input.GetKeyDown("1"))
            { vehicleID_str += "1"; }
            if (Input.GetKeyDown(KeyCode.Keypad2) ||
                Input.GetKeyDown("2"))
            { vehicleID_str += "2"; }
            if (Input.GetKeyDown(KeyCode.Keypad3) ||
                Input.GetKeyDown("3"))
            { vehicleID_str += "3"; }
            if (Input.GetKeyDown(KeyCode.Keypad4) ||
                Input.GetKeyDown("4"))
            { vehicleID_str += "4"; }
            if (Input.GetKeyDown(KeyCode.Keypad5) ||
                Input.GetKeyDown("5"))
            { vehicleID_str += "5"; }
            if (Input.GetKeyDown(KeyCode.Keypad6) ||
                Input.GetKeyDown("6"))
            { vehicleID_str += "6"; }
            if (Input.GetKeyDown(KeyCode.Keypad7) ||
                Input.GetKeyDown("7"))
            { vehicleID_str += "7"; }
            if (Input.GetKeyDown(KeyCode.Keypad8) ||
                Input.GetKeyDown("8"))
            { vehicleID_str += "8"; }
            if (Input.GetKeyDown(KeyCode.Keypad9) ||
                Input.GetKeyDown("9"))
            { vehicleID_str += "9"; }
        return false;
        }
    }
}
