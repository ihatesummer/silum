using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class StartupSceneController : MonoBehaviour
{
    private GameObject startButton;
    private GameObject loadingPopup;
    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        loadingPopup = GameObject.Find("Popup_Loading");
        startButton = GameObject.Find("Start Button");
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonUp(0)){
            audioSource.Play();
            startButton.GetComponent<StartButtonController>().ChangeAnimationState("Click");
            loadingPopup.GetComponent<PopupController>().ChangeAnimationState("VerticalPopup");
        }
    }
}
