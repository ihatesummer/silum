using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupController : MonoBehaviour
{
    private Animator animator;
    private string currentState;
    public Text message;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        message = GameObject.Find("Message").GetComponent<Text>();
        message.text = "Loading...";
    }

    public void ChangeAnimationState(string newState)
    {
        if (currentState == newState) return;
        animator.Play(newState);
        currentState = newState;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
