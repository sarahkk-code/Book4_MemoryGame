using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using TMPro;

public class ExitManager : MonoBehaviour
{
    [Header("Current Game Result")]
    public TMP_Text resultsText;

    [Header("Database Results")]
    public TMP_Text databaseResultsText;

    private void Start()
    {
        string playerName = PlayerPrefs.GetString("PlayerName", "Player");
        int colors = PlayerPrefs.GetInt("FinalColors", 0);
        int finalTime = PlayerPrefs.GetInt("FinalTime", 0);
        string gameResult = PlayerPrefs.GetString("GameResult", "");

        // Current game result
        if (resultsText != null)
        {
            resultsText.text =
                "Player: " + playerName +
                "\nColors: " + colors +
                "\nTime: " + finalTime + " seconds" +
                "\n" + gameResult;
        }

        // Get previous results from database
        StartCoroutine(GetDatabaseResults());
    }

    private IEnumerator GetDatabaseResults()
    {
        string url = "http://localhost/book4/getMemories.php";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                if (databaseResultsText != null)
                {
                    databaseResultsText.text =
                        "Could not load previous results.";
                }

                Debug.LogError("Database Error: " + request.error);
                yield break;
            }

            string data = request.downloadHandler.text;

            if (databaseResultsText == null)
            {
                yield break;
            }

            if (string.IsNullOrWhiteSpace(data) ||
                data.Trim() == "NO_RESULTS")
            {
                databaseResultsText.text = "No previous results.";
                yield break;
            }

            string[] rows = data.Trim().Split('\n');

            string formattedResults = "Previous Results\n\n";

            foreach (string row in rows)
            {
                string[] values = row.Trim().Split('|');

                if (values.Length >= 3)
                {
                    string name = values[0];
                    string colors = values[1];

                    if (float.TryParse(values[2], out float seconds))
                    {
                        formattedResults +=
                            name + "    " +
                            colors + " colors    " +
                            seconds.ToString("F2") +
                            " seconds\n";
                    }
                }
            }

            databaseResultsText.text = formattedResults;
        }
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("Preferences");
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;

        Application.Quit();
    }
}