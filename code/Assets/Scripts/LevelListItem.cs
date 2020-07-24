using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelListItem : MonoBehaviour
{
    public LevelMenu level_menu;
    public Button button;
    public Text text;
    public Level level;
    // Start is called before the first frame update
    void Start()
    {
        button.onClick.AddListener(() => level_menu.OnClickLevel(level.id));
        button.GetComponentInChildren<Text>().text = "Level " + level.id + ": " + level.name;
        text.text = level.name + $" <size=30><color=#00ff00>{(level.primary.finished?'★':'☆')}</color></size>";
        foreach(var goal in level.optional)
            text.text += $" <size=30><color=#0088ff>{(goal.finished ? '★' : '☆')}</color></size>";
        if (level.id != 1 && GameData.levels["Level"+(level.id-1).ToString("D2")].primary.finished == false)
        {
            /// Level progression one by one
            //button.interactable = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
