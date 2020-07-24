using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartMenu : MonoBehaviour
{
    public GameObject level_menu;
    public GameObject program_menu;
    void Awake()
    {
        switch (GameData.to_load)
        {
            case GameData.MENU.START:
                level_menu.gameObject.SetActive(false);
                program_menu.gameObject.SetActive(false);
                break;
            case GameData.MENU.LEVEL:
                gameObject.SetActive(false);
                level_menu.gameObject.SetActive(true);
                program_menu.gameObject.SetActive(false);
                break;
            case GameData.MENU.PROGRAM:
                gameObject.SetActive(false);
                level_menu.gameObject.SetActive(false);
                program_menu.gameObject.SetActive(true);
                break;
        }
    }
    // Update is called once per frame
    void Update()
    {
    }
    public void OnClickPlay()
    {
        gameObject.SetActive(false);
        level_menu.SetActive(true);
    }
    public void OnClickExit()
    {
        Application.Quit();
    }
}
