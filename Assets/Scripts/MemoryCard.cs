using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MemoryCard : MonoBehaviour
{
    public int cardValue;

    private Button button;
    private Image cardImage;
    private TextMeshProUGUI cardText;

    public bool IsMatched { get; private set; }

    private void Awake()
    {
        button = GetComponent<Button>();
        cardImage = GetComponent<Image>();
        cardText = GetComponentInChildren<TextMeshProUGUI>();

        button.onClick.AddListener(FlipCard);
    }

    public void SetValue(int value)
    {
        cardValue = value;

        IsMatched = false;

        HideCard();

        button.interactable = true;
    }

    public void FlipCard()
    {
        if (IsMatched)
        {
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CardClicked(this);
        }
    }

    // SHOW COLOR
    public void RevealCard()
    {
        if (cardImage != null)
        {
            cardImage.color = GetCardColor();
        }

        if (cardText != null)
        {
            cardText.text = "";
        }
    }

    // HIDE COLOR
    public void HideCard()
    {
        if (IsMatched)
        {
            return;
        }

        if (cardImage != null)
        {
            cardImage.color = Color.white;
        }

        if (cardText != null)
        {
            cardText.text = "?";
        }
    }

    // CORRECT MATCH
    public void SetMatched()
    {
        IsMatched = true;

        // KEEP THE COLOR SHOWING
        if (cardImage != null)
        {
            cardImage.color = GetCardColor();
        }

        if (cardText != null)
        {
            cardText.text = "";
        }

        // Prevent clicking it again
        if (button != null)
        {
            button.interactable = false;
        }
    }

    private Color GetCardColor()
    {
        switch (cardValue)
        {
            case 0:
                return Color.red;

            case 1:
                return Color.blue;

            case 2:
                return Color.green;

            case 3:
                return Color.yellow;

            case 4:
                return new Color(0.6f, 0.2f, 0.8f);

            case 5:
                return new Color(1f, 0.5f, 0f);

            case 6:
                return new Color(1f, 0.4f, 0.7f);

            case 7:
                return Color.cyan;

            case 8:
                return Color.white;

            case 9:
                return Color.black;

            default:
                return Color.gray;
        }
    }
}