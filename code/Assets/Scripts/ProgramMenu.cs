using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ProgramMenu : MonoBehaviour
{
    public GameObject level_menu;
    public Text header;
    public GameObject trigger_prefab;
    public GameObject trigger_list;
    public ScrollRect scroll;
    public Button add_button;
    public bool started = false;
    // Start is called before the first frame update
    void Start()
    {
        started = true;
        header.text = "Level " + GameData.level + ": Set behavior";
        add_button.onClick.AddListener( delegate { OnClickAddTrigger(); } );
        if (GameData.triggers.Count == 0)
            OnClickAddTrigger();
        else
            foreach (Trigger trigger in GameData.triggers)
                OnClickAddTrigger(trigger);
    }
    public void Reset()
    {
        header.text = "Level " + GameData.level + ": Set behavior";
        if (trigger_list.transform.childCount > 1)
        {
            foreach (Transform trigger in trigger_list.transform)
            {
                var tb = trigger.GetComponent<TriggerBlock>();
                if (tb != null)
                    tb.OnClickRemoveTrigger();
            }
        }
        else
            GameData.ResetTriggers();
        if (started)
            OnClickAddTrigger();
    }
    public void OnClickBackToLevelSelection()
    {
        gameObject.SetActive(false);
        level_menu.SetActive(true);
    }
    public void OnClickStart()
    {
        for (int i=GameData.triggers.Count-1; i>=0; i--)
        {
            if (GameData.triggers[i].acts.Count == 0)
            {
                //GameData.triggers.RemoveAt(i); // getting rid of functionless triggers
                // better replace this with action merger of same events with same/no conditions
            }
        }
        SceneManager.LoadScene("Scenes/Level"+ GameData.level.ToString("D2"));
    }
    public void OnClickAddTrigger(Trigger trigger = null)
    {
        GameObject new_item = Instantiate(trigger_prefab);
        TriggerBlock new_item_script = new_item.GetComponent<TriggerBlock>();
        int id = trigger_list.transform.childCount - 1;
        new_item.transform.SetParent(trigger_list.transform);
        new_item_script.program_menu = this;
        new_item.transform.SetSiblingIndex(id);
        if (trigger != null)
            new_item_script.trigger = trigger;
        //Canvas c = gameObject.GetComponent<Canvas>();
        ResetScroll(1f);
    }
    public void ResetScroll(float value, bool checkIfInactive = false)
    {
        Canvas.ForceUpdateCanvases();
        if (!checkIfInactive || !scroll.horizontalScrollbar.IsActive())
            scroll.horizontalNormalizedPosition = value;
        else
            scroll.horizontalNormalizedPosition = 0f;
        //Canvas.ForceUpdateCanvases();
    }
}
