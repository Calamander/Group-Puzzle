using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelMenu : MonoBehaviour
{
    public GameObject start_menu;
    public ProgramMenu program_menu;
    public GameObject list;
    public GameObject list_item;
    // Start is called before the first frame update
    void Start()
    {
        foreach (var key in GameData.levels.Keys)
        {
            GameObject new_item = Instantiate(list_item);
            LevelListItem new_item_script = new_item.GetComponent<LevelListItem>();
            new_item_script.level_menu = this;
            new_item_script.level = GameData.levels[key];
            new_item.transform.SetParent(list.transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnClickLevel(int level)
    {
        GameData.level = level;
        gameObject.SetActive(false);
        program_menu.gameObject.SetActive(true);
        program_menu.Reset();

    }
    public void OnClickBackToStartMenu()
    {
        gameObject.SetActive(false);
        start_menu.SetActive(true);
    }
}
