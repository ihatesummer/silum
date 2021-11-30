using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class StartupSceneController : MonoBehaviour
{
    public GameObject startButton;
    public GameObject loadingPopup;
    public AudioSource popupSound;
    private bool bPopup;

    void Start()
    {
        loadingPopup = GameObject.Find("Popup_Loading");
        startButton = GameObject.Find("Start Button");
        popupSound = GetComponent<AudioSource>();
        bPopup = false;
    }

    void Update()
    {
        if(!bPopup && Input.GetMouseButtonUp(0)){
            popupSound.Play();
            startButton.GetComponent<StartButtonController>().ChangeAnimationState("Click");
            loadingPopup.GetComponent<PopupController>().ChangeAnimationState("VerticalPopup");
            bPopup = true;
        }
    }
}
