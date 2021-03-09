using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPanelManager : MonoBehaviour
{
    public CanvasGroup buttonsCanvasGroup;
    public CanvasGroup panelContentsCanvasGroup;
    public Animator panelAnimator;

    public void OpenPanel()
    {
        // let the settings panel be interactable
        panelContentsCanvasGroup.interactable = true;
        panelContentsCanvasGroup.blocksRaycasts = true;

        // block additional button inputs to the buttons behind the panel
        buttonsCanvasGroup.interactable = false;

        // play the animation for the settings panel
        panelAnimator.SetBool("isOpened", true);
    }

    public void ClosePanel()
    {
        // prevent the settings panel from being interacted
        panelContentsCanvasGroup.interactable = false;
        panelContentsCanvasGroup.blocksRaycasts = false;

        // allow button inputs to the buttons behind the panel
        buttonsCanvasGroup.interactable = true;

        // play the animation for the settings panel
        panelAnimator.SetBool("isOpened", false);
    }
}
