using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PreferencesManager : MonoBehaviour
{
    public TMP_InputField playerNameInput;
    public TMP_Dropdown numberOfColorsDropdown;
    public Slider timeSlider;
    public TMP_Text timeLabel;

    private void Start()
    {
        timeSlider.minValue = 10;
        timeSlider.maxValue = 120;
        timeSlider.wholeNumbers = true;

        playerNameInput.text = PlayerPrefs.GetString("PlayerName", "");

        int savedColors = PlayerPrefs.GetInt("NumberOfColors", 3);
        numberOfColorsDropdown.value = savedColors - 1;

        int savedTime = PlayerPrefs.GetInt("GameTime", 60);
        timeSlider.value = savedTime;

        UpdateTimeLabel();
    }

    // THIS is the function Unity needs to see
    public void OnTimeChanged(float value)
    {
        if (timeLabel != null)
        {
            timeLabel.text = "Time to Complete: " +
                             Mathf.RoundToInt(value) +
                             " seconds";
        }
    }

    public void StartGame()
    {
        PlayerPrefs.SetString(
            "PlayerName",
            playerNameInput.text
        );

        int colors = 3;

        if (numberOfColorsDropdown.options.Count > 0)
        {
            string selectedText =
                numberOfColorsDropdown.options[numberOfColorsDropdown.value].text;

            selectedText = selectedText.Replace(" Colors", "");
            selectedText = selectedText.Replace(" Color", "");

            int.TryParse(selectedText, out colors);
        }

        PlayerPrefs.SetInt("NumberOfColors", colors);

        int gameTime = Mathf.RoundToInt(timeSlider.value);

        PlayerPrefs.SetInt(
            "GameTime",
            gameTime
        );

        PlayerPrefs.Save();

        SceneManager.LoadScene("Game");
    }

    private void UpdateTimeLabel()
    {
        if (timeLabel != null)
        {
            timeLabel.text = "Time to Complete: " +
                             Mathf.RoundToInt(timeSlider.value) +
                             " seconds";
        }
    }
}