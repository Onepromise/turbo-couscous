using UnityEngine;
using TMPro;

public class ManaUI : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI player1ManaText;
    public TextMeshProUGUI player2ManaText;
    
    private Canvas canvas;
    
    void Awake()
    {
        CreateUI();
    }
    
    void CreateUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("ManaCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        // Player 1 Mana Display (Bottom Left)
        GameObject p1TextObj = new GameObject("Player1Mana");
        p1TextObj.transform.SetParent(canvasObj.transform, false);
        
        player1ManaText = p1TextObj.AddComponent<TextMeshProUGUI>();
        player1ManaText.fontSize = 36;
        player1ManaText.color = Color.cyan;
        player1ManaText.alignment = TextAlignmentOptions.BottomLeft;
        player1ManaText.text = "P1 Mana: 1/1";
        
        RectTransform p1Rect = p1TextObj.GetComponent<RectTransform>();
        p1Rect.anchorMin = new Vector2(0, 0);
        p1Rect.anchorMax = new Vector2(0, 0);
        p1Rect.pivot = new Vector2(0, 0);
        p1Rect.anchoredPosition = new Vector2(20, 20);
        p1Rect.sizeDelta = new Vector2(300, 50);
        
        // Player 2 Mana Display (Top Left)
        GameObject p2TextObj = new GameObject("Player2Mana");
        p2TextObj.transform.SetParent(canvasObj.transform, false);
        
        player2ManaText = p2TextObj.AddComponent<TextMeshProUGUI>();
        player2ManaText.fontSize = 36;
        player2ManaText.color = Color.red;
        player2ManaText.alignment = TextAlignmentOptions.TopLeft;
        player2ManaText.text = "P2 Mana: 1/1";
        
        RectTransform p2Rect = p2TextObj.GetComponent<RectTransform>();
        p2Rect.anchorMin = new Vector2(0, 1);
        p2Rect.anchorMax = new Vector2(0, 1);
        p2Rect.pivot = new Vector2(0, 1);
        p2Rect.anchoredPosition = new Vector2(20, -20);
        p2Rect.sizeDelta = new Vector2(300, 50);
    }
    
    public void UpdateManaDisplay(int playerID, int currentMana, int maxMana)
    {
        string text = $"P{playerID + 1} Mana: {currentMana}/{maxMana}";
        
        if (playerID == 0)
        {
            player1ManaText.text = text;
        }
        else
        {
            player2ManaText.text = text;
        }
    }
}