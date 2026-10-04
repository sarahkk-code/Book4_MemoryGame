using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.Networking;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Cards")]
    public GameObject cardPrefab;
    public Transform cardGrid;

    [Header("UI")]
    public TMP_Text playerNameText;
    public TMP_Text timerText;

    [Header("Pause")]
    public GameObject pausePanel;

    private int numberOfColors;
    private int gameTime;
    private float timeRemaining;

    private MemoryCard firstCard;
    private MemoryCard secondCard;

    private bool checkingMatch = false;
    private bool gameOver = false;
    private bool isPaused = false;

    private int matchedPairs = 0;

    // PHP file in MAMP
    private string saveMemoryURL =
        "http://localhost/book4/saveMemory.php";

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        numberOfColors =
            PlayerPrefs.GetInt("NumberOfColors", 3);

        gameTime =
            PlayerPrefs.GetInt("GameTime", 60);

        timeRemaining = gameTime;

        string playerName =
            PlayerPrefs.GetString("PlayerName", "Player");

        if (playerNameText != null)
        {
            playerNameText.text =
                "Player: " + playerName;
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        CreateCards();

        UpdateTimerText();
    }

    private void Update()
    {
        // ESC = Pause / Resume
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }

        if (gameOver || isPaused)
        {
            return;
        }

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0)
        {
            timeRemaining = 0;

            UpdateTimerText();

            EndGame(false);

            return;
        }

        UpdateTimerText();
    }

    // =====================================================
    // CREATE AND SHUFFLE CARDS
    // =====================================================

    private void CreateCards()
    {
        List<int> cardValues = new List<int>();

        // Two cards for every color
        for (int i = 0; i < numberOfColors; i++)
        {
            cardValues.Add(i);
            cardValues.Add(i);
        }

        // Shuffle the cards
        for (int i = cardValues.Count - 1; i > 0; i--)
        {
            int randomIndex =
                Random.Range(0, i + 1);

            int temp = cardValues[i];

            cardValues[i] =
                cardValues[randomIndex];

            cardValues[randomIndex] =
                temp;
        }

        // Create shuffled cards
        foreach (int value in cardValues)
        {
            GameObject cardObject =
                Instantiate(cardPrefab, cardGrid);

            MemoryCard card =
                cardObject.GetComponent<MemoryCard>();

            if (card != null)
            {
                card.SetValue(value);
            }
        }
    }

    // =====================================================
    // CARD CLICK
    // =====================================================

    public void CardClicked(MemoryCard card)
    {
        if (gameOver ||
            isPaused ||
            checkingMatch)
        {
            return;
        }

        if (card == null ||
            card.IsMatched)
        {
            return;
        }

        // FIRST CARD
        if (firstCard == null)
        {
            firstCard = card;

            firstCard.RevealCard();

            return;
        }

        // Don't click the same card twice
        if (card == firstCard)
        {
            return;
        }

        // SECOND CARD
        secondCard = card;

        secondCard.RevealCard();

        StartCoroutine(CheckMatch());
    }

    // =====================================================
    // CHECK TWO CARDS
    // =====================================================

    private IEnumerator CheckMatch()
    {
        checkingMatch = true;

        // Let player see both colors
        yield return new WaitForSeconds(0.7f);

        if (firstCard.cardValue ==
            secondCard.cardValue)
        {
            // CORRECT MATCH
            Debug.Log("MATCH!");

            firstCard.SetMatched();
            secondCard.SetMatched();

            matchedPairs++;

            // ALL PAIRS MATCHED
            if (matchedPairs >= numberOfColors)
            {
                EndGame(true);

                yield break;
            }
        }
        else
        {
            // WRONG MATCH
            Debug.Log("Not a match.");

            firstCard.HideCard();
            secondCard.HideCard();
        }

        firstCard = null;
        secondCard = null;

        checkingMatch = false;
    }

    // =====================================================
    // PAUSE / RESUME
    // =====================================================

    private void TogglePause()
    {
        if (gameOver)
        {
            return;
        }

        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        if (gameOver)
        {
            return;
        }

        isPaused = true;

        Time.timeScale = 0f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        Debug.Log("GAME PAUSED");
    }

    public void ResumeGame()
    {
        isPaused = false;

        Time.timeScale = 1f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Debug.Log("GAME RESUMED");
    }

    // =====================================================
    // EXIT BEFORE TIME IS UP
    // =====================================================

    public void ExitGame()
    {
        Time.timeScale = 1f;

        isPaused = false;

        PlayerPrefs.SetString(
            "GameResult",
            "Game Exited"
        );

        PlayerPrefs.Save();

        SceneManager.LoadScene("Exit");
    }

    // =====================================================
    // END GAME
    // =====================================================

    private void EndGame(bool completed)
    {
        Time.timeScale = 1f;

        if (gameOver)
        {
            return;
        }

        gameOver = true;

        // Calculate how much time the player actually used
        float elapsedTime =
            gameTime - timeRemaining;

        if (elapsedTime < 0)
        {
            elapsedTime = 0;
        }

        // Save information for Exit scene
        PlayerPrefs.SetInt(
            "GameCompleted",
            completed ? 1 : 0
        );

        PlayerPrefs.SetInt(
            "FinalTime",
            Mathf.CeilToInt(elapsedTime)
        );

        PlayerPrefs.SetInt(
            "FinalColors",
            numberOfColors
        );

        PlayerPrefs.SetString(
            "GameResult",
            completed
                ? "You Won!"
                : "Time's Up!"
        );

        PlayerPrefs.Save();

        if (completed)
        {
            Debug.Log(
                "GAME COMPLETE! Time: " +
                elapsedTime +
                " seconds"
            );

            // SAVE TO DATABASE
            StartCoroutine(
                SaveMemoryToDatabase(elapsedTime)
            );
        }
        else
        {
            Debug.Log("TIME'S UP!");

            // Do NOT save a failed game
            SceneManager.LoadScene("Exit");
        }
    }

    // =====================================================
    // SAVE COMPLETED GAME TO DATABASE
    // =====================================================

    private IEnumerator SaveMemoryToDatabase(
        float elapsedTime)
    {
        string playerName =
            PlayerPrefs.GetString(
                "PlayerName",
                "Player"
            );

        WWWForm form = new WWWForm();

        form.AddField(
            "name",
            playerName
        );

        form.AddField(
            "colors",
            numberOfColors
        );

        form.AddField(
            "seconds",
            elapsedTime.ToString(
                System.Globalization.CultureInfo.InvariantCulture
            )
        );

        using (UnityWebRequest request =
               UnityWebRequest.Post(
                   saveMemoryURL,
                   form))
        {
            yield return request.SendWebRequest();

            if (request.result ==
                UnityWebRequest.Result.Success)
            {
                Debug.Log(
                    "DATABASE SAVE SUCCESS: " +
                    request.downloadHandler.text
                );
            }
            else
            {
                Debug.LogError(
                    "DATABASE SAVE FAILED: " +
                    request.error
                );
            }
        }

        // Go to Exit after database save attempt
        SceneManager.LoadScene("Exit");
    }

    // =====================================================
    // TIMER DISPLAY
    // =====================================================

    private void UpdateTimerText()
    {
        if (timerText != null)
        {
            timerText.text =
                "Time: " +
                Mathf.CeilToInt(timeRemaining);
        }
    }

    // =====================================================
    // CLEANUP
    // =====================================================

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}