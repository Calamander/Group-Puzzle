using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelEngine : MonoBehaviour
{
    public float jump_speed = 25f;
    public float move_speedup = 100f;
    public float dash_speed = 100f;
    public float dash_timeout = 5f;
    public float max_velocity = 100f;
    public int group_size;
    public int simultaneous_max_group = 0;
    public static LevelEngine current;
    public GameObject menu;
    public Text goals;
    public Text end_text;
    public Button continue_button;
    public Level level;
    public static UnityEvent OnRestart = new UnityEvent();
    private void Awake()
    {
        Time.timeScale = 1;
        level = GameData.levels[SceneManager.GetActiveScene().name];
        current = this;
        Unit.init = false;
    }
    // Start is called before the first frame update
    void Start()
    {
        Unit.OnUnitDied.AddListener(OnUnitDied);
        menu.SetActive(false);
        level.Reset(group_size);
        UpdateGoalsPanel();
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("escape"))
        {
            Time.timeScale = menu.activeSelf? 1:0;
            menu.SetActive(!menu.activeSelf);
        }
        if (level.OnUpdate())
            UpdateGoalsPanel();
    }
    public void OnClickContinue()
    {
        Time.timeScale = 1;
        menu.SetActive(false);
    }
    public void OnClickRestart()
    {
        var spawns = GameObject.FindGameObjectsWithTag("Respawn");
        OnRestart.Invoke();
        end_text.gameObject.SetActive(false);
        continue_button.gameObject.SetActive(true);
        level.Reset(group_size);
        Time.timeScale = 1;
        menu.gameObject.SetActive(false);
    }
    public void OnClickChangeBehavior()
    {
        GameData.to_load = GameData.MENU.PROGRAM;
        SceneManager.LoadScene("Scenes/MainMenu");
    }
    public void OnClickChangeLevel()
    {
        GameData.to_load = GameData.MENU.LEVEL;
        SceneManager.LoadScene("Scenes/MainMenu");
    }
    public void OnClickExit()
    {
        Application.Quit();
    }
    public void OnUnitDied()
    {
        if (level.OnUnitDied())
            UpdateGoalsPanel();
    }
    public void OnUnitFinished()
    {
        if (level.OnUnitSaved())
            UpdateGoalsPanel();
    }
    public void UpdateGoalsPanel()
    {
        var primary_already_finished = level.primary.finished;
        string[] text = level.primary.GetText();
        goals.text = $"<size=24><color=#00ff00>{(level.primary.finished ? "★" : "☆")}</color></size> " + (text.Length == 1 ? $"<color=#00ff00>{text[0]}</color>" : (text.Length == 2? $"<color=#ff0000>{string.Join(" ", text)}</color>": string.Join(" ", text)));
        foreach (var goal in level.optional)
        {
            text = goal.GetText();
            goals.text += $"\n<size=24><color=#0088ff>{(goal.finished ? "★" : "☆")}</color></size> " + (text.Length == 1 ? $"<color=#0088ff>{text[0]}</color>" : (text.Length == 2? $"<color=#ff0000>{string.Join(" ", text)}</color>" : string.Join(" ", text)));
        }
        if (level.primary.failed)
            TheEnd(false, false);
        else if (!primary_already_finished && level.primary.finished)
            TheEnd(true);
    }
    public void TheEnd(bool victory = true, bool resumable = true)
    {
        Time.timeScale = 0;
        end_text.text = victory ? "Victory!" : "Defeat :(";
        end_text.gameObject.SetActive(true);
        if (!resumable)
            continue_button.gameObject.SetActive(false);
        menu.gameObject.SetActive(true);
    }
}
