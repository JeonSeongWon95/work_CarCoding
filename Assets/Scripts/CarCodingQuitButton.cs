using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CarCodingQuitButton : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private AudioClip touchSound;
    
    private const int quitClick = 7;
    private int currentClick = 0;
    private float clickTimer = 0.0f;
    
    public float idleTimeLimit = 30.0f;
    private float idleTimer = 0.0f;

    private bool isClickBefore = false;

    void Update()
    {
        if (currentClick > 0)
        {
            clickTimer += Time.deltaTime;
            if (clickTimer > 1.0f)
            {
                clickTimer = 0.0f;
                currentClick--;
            }
        }
        
        if (currentClick > quitClick)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            
        }

        if (Input.touchCount > 0 || Input.anyKeyDown)
        {
            idleTimer = 0.0f;

            if (!isClickBefore)
            {
                AudioManager.instance.PlayAudio(AudioClipType.Click, 0.5f);
            }

            isClickBefore = true;
        }
        else
        {
            isClickBefore = false;
        }

        if (CarCodingFlowController.instance != null)
        {
            if (CarCodingFlowController.instance.NowWaitStatus())
            {
                idleTimer = 0.0f;
                return;
            }
        }
        
        idleTimer += Time.deltaTime;

        if (idleTimer >= idleTimeLimit)
        {
            GoToMainMenu();
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        currentClick++;
    }

    void GoToMainMenu()
    {
        idleTimer = 0.0f;
        CarCodingFlowController.instance.ChangeStatus("Main");
    }

}
