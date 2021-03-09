using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NavigablePage : MonoBehaviour
{
    public List<GameObject> pages;
    public byte startAtPage = 1;

    [Space]
    public Button previousPageButton;
    public Button nextPageButton;

    private byte currentPage = 1;

    // Start is called just before any of the Update methods is called the first time
    private void Start()
    {
        currentPage = startAtPage;

        for (int i = 0; i < pages.Count; i++)
        {
            if (i == startAtPage - 1)
            {
                // only set one page active based on which pages to start at
                pages[i].SetActive(true);
            }
            else
            {
                // the rest of the pages are inactives
                pages[i].SetActive(false);
            }
        }

        if (startAtPage <= 1)
        {
            previousPageButton.interactable = false;
        }

        if (startAtPage > pages.Count)
        {
            nextPageButton.interactable = false;
        }
    }

    public void NextPage()
    {
        currentPage++;

        if (currentPage >= pages.Count)
        {
            nextPageButton.interactable = false;
        }

        if (!previousPageButton.interactable && currentPage <= pages.Count)
        {
            previousPageButton.interactable = true;
        }

        // set the next page to be active and set the previous page to be inactive
        // we minus some amount off the index because arrays start at 0
        pages[currentPage - 1].SetActive(true);
        pages[currentPage - 2].SetActive(false);
    }

    public void PreviousPage()
    {
        currentPage--;

        if (currentPage <= 1)
        {
            previousPageButton.interactable = false;
        }

        if (!nextPageButton.interactable && currentPage > 1)
        {
            nextPageButton.interactable = true;
        }

        // set the previous page to be active and set the next page to be inactive
        pages[currentPage - 1].SetActive(true);
        pages[currentPage].SetActive(false);
    }
}
