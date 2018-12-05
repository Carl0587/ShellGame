﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreLogic;
using System;
using HoloToolkit.Unity.InputModule;
using System.Text;

public class GameController : MonoBehaviour
{

    [SerializeField]
    private int numberOfStrikes = 3;

    [SerializeField]
    private GameObject[] itemContainers;

    private ShellGameLogic coreLogic;

    private static GameController instance;

    private int score = 0;

    private TextMesh strikeText;
    private TextMesh scoreText;
    private TextMesh highScoreText;

    private StringBuilder strikeTextStringBuilder = new StringBuilder(6);

    public static GameController Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }
            else
            {
                GameObject go = GameObject.FindGameObjectWithTag(Res.GameController);
                instance = go.GetComponent<GameController>();

                return instance;
            }
        }
    }

    private void Awake()
    {
        coreLogic = new ShellGameLogic(itemContainers.Length, numberOfStrikes);
    }


    // Use this for initialization
    void Start()
    {
        FindTextObjects();

        HookupCoreLogicEvents();

        HighScore.LoadHighScore();

        UpdateHighScore();

        StartTurn();

    }

    private void FindTextObjects()
    {
        strikeText = GameObject.FindGameObjectWithTag(Res.StrikeText).GetComponent<TextMesh>();
        scoreText = GameObject.FindGameObjectWithTag(Res.ScoreText).GetComponent<TextMesh>();
        highScoreText = GameObject.FindGameObjectWithTag(Res.HighScoreText).GetComponent<TextMesh>();
    }

    private void StartTurn()
    {
        PrepareItemsForTurn();

        HideStrikeText();

        // Animate items 

        // Play animation sounds

        // Reset items
        coreLogic.ResetItems();

    }

    private void PrepareItemsForTurn()
    {
        for (int i = 0; i < itemContainers.Length; i++)
        {
            itemContainers[i].GetComponent<MeshRenderer>().enabled = true;
            itemContainers[i].transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
        }
    }

    private void HookupCoreLogicEvents()
    {
        coreLogic.StartTurn += CoreLogic_StartTurn;

        coreLogic.SelectedItem += CoreLogic_SelectedItem;
        coreLogic.CheckingItem += CoreLogic_CheckingItem;
        coreLogic.MatchNotMade += CoreLogic_MatchNotMade;
        coreLogic.MatchMade += CoreLogic_MatchMade;

        coreLogic.ItemReset += CoreLogic_ItemReset;
        coreLogic.ResetComplete += CoreLogic_ResetComplete;

        coreLogic.GameOver += CoreLogic_GameOver;
    }

    private void CoreLogic_StartTurn(object sender, EventArgs e)
    {
        Debug.Log("Start Turn");
        Invoke(nameof(StartTurn), 1.2f); // delay to allow for animations
    }

    private void CoreLogic_ItemReset(object sender, EventArgs e)
    {
        Debug.Log("Item Reset");

        // Restore input
        if (!InputManager.Instance.IsInputEnabled)
        {
            InputManager.Instance.PopInputDisable();
        }
    }

    private void CoreLogic_GameOver(object sender, EventArgs e)
    {
        Debug.Log($"GAME OVER. Score: {score} Strikes: {coreLogic.Strikes}");

        HighScore.Value = score;
        HighScore.SaveHighScore();

        score = 0;
        UpdateScore();

        strikeTextStringBuilder.Clear();
    }

    private void CoreLogic_ResetComplete(object sender, EventArgs e)
    {
        Debug.Log("Reset Complete");
    }

    private void CoreLogic_SelectedItem(object sender, ItemEventArgs e)
    {
        Debug.Log($"Selected Item: {e.Id}");

        // Disable input
        InputManager.Instance.PushInputDisable();

        // Show the pea / insert item
        itemContainers[e.Id].transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
    }

    private void CoreLogic_CheckingItem(object sender, ItemEventArgs e)
    {
        Debug.Log($"Checking Item: {e.Id}");
        itemContainers[e.Id].GetComponent<MeshRenderer>().enabled = false;
    }

    private void CoreLogic_MatchNotMade(object sender, NoMatchEventArgs e)
    {
        Debug.Log($"No Match Made. IsStrike: {e.IsStrike}");

        if (e.IsStrike)
        {
            ShowStrikeText();
        }
    }

    private void CoreLogic_MatchMade(object sender, MatchEventArgs e)
    {
        score += e.Score;

        HighScore.Value = score;
        UpdateHighScore();

        UpdateScore();
        Debug.Log($"Match Made. Id: {e.Id} Score: {e.Score} Total Score: {score}");
    }

    private void UpdateScore()
    {
        scoreText.text = $"{Res.Score}{score}";
    }

    private void UpdateHighScore()
    {
        highScoreText.text = $"{Res.HighScore}{HighScore.Value}";
    }

    private void ShowStrikeText()
    {
        strikeTextStringBuilder.Append(Res.StrikeX);

        strikeText.text = strikeTextStringBuilder.ToString();

        strikeText.gameObject.SetActive(true);
    }

    private void HideStrikeText()
    {
        strikeText.gameObject.SetActive(false);
    }

    public void CheckForItem(int itemId)
    {
        Debug.Log($"Check for pea: {itemId}");
        coreLogic.CheckForItem(itemId);
    }

    private void OnDestroy()
    {
        coreLogic.StartTurn -= CoreLogic_StartTurn;

        coreLogic.SelectedItem -= CoreLogic_SelectedItem;
        coreLogic.CheckingItem -= CoreLogic_CheckingItem;
        coreLogic.MatchNotMade -= CoreLogic_MatchNotMade;
        coreLogic.MatchMade -= CoreLogic_MatchMade;

        coreLogic.ItemReset -= CoreLogic_ItemReset;
        coreLogic.ResetComplete -= CoreLogic_ResetComplete;

        coreLogic.GameOver -= CoreLogic_GameOver;
    }
}