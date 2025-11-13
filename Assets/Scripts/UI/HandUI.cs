using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class HandUI : MonoBehaviour
{
    [Header("Settings")]
    public float cardWidth = 140f;
    public float cardHeight = 180f;
    public float cardSpacing = 10f;
    
    private Canvas canvas;
    private GameObject handPanel;
    private List<CardButton> cardButtons = new List<CardButton>();
    
    public System.Action<MinionData> OnCardClicked;
    
    void Awake()
    {
        CreateHandUI();
    }
    
    void CreateHandUI()
    {
        // Create Canvas if not exists
        canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            UnityEngine.UI.CanvasScaler scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        
        // CRITICAL: Ensure EventSystem exists
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("EventSystem created for UI input");
        }
        
        // Create Hand Panel (RIGHT SIDE - VERTICAL)
        handPanel = new GameObject("HandPanel");
        handPanel.transform.SetParent(canvas.transform, false);
        
        RectTransform panelRect = handPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1, 0.5f);  // Right side, centered vertically
        panelRect.anchorMax = new Vector2(1, 0.5f);
        panelRect.pivot = new Vector2(1, 0.5f);
        panelRect.anchoredPosition = new Vector2(-20, 0);  // 20px from right edge
        panelRect.sizeDelta = new Vector2(cardWidth + 20, 800);
        
        // Optional: Add background panel
        Image panelBg = handPanel.AddComponent<Image>();
        panelBg.color = new Color(0.1f, 0.1f, 0.1f, 0.7f);
        
        // Add title
        GameObject titleObj = new GameObject("HandTitle");
        titleObj.transform.SetParent(handPanel.transform, false);
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "YOUR HAND";
        titleText.fontSize = 24;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -10);
        titleRect.sizeDelta = new Vector2(-20, 40);
    }
    
    public void UpdateHand(List<MinionData> hand)
    {
        // Clear existing cards
        foreach (CardButton btn in cardButtons)
        {
            if (btn != null && btn.gameObject != null)
            {
                Destroy(btn.gameObject);
            }
        }
        cardButtons.Clear();
        
        // Create new card buttons (VERTICAL LAYOUT)
        for (int i = 0; i < hand.Count; i++)
        {
            CreateCardButton(hand[i], i, hand.Count);
        }
    }
    
    void CreateCardButton(MinionData cardData, int index, int totalCards)
    {
        // Create card button
        GameObject cardObj = new GameObject($"Card_{cardData.minionName}_{index}");
        cardObj.transform.SetParent(handPanel.transform, false);
        
        // Position cards VERTICALLY from top to bottom
        float startY = -60; // Start below title
        float yPos = startY - (index * (cardHeight + cardSpacing));
        
        RectTransform cardRect = cardObj.AddComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 1);
        cardRect.anchorMax = new Vector2(0.5f, 1);
        cardRect.pivot = new Vector2(0.5f, 1);
        cardRect.anchoredPosition = new Vector2(0, yPos);
        cardRect.sizeDelta = new Vector2(cardWidth, cardHeight);
        
        // Add background image
        Image bgImage = cardObj.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);
        
        // Add outline
        Outline outline = cardObj.AddComponent<Outline>();
        outline.effectColor = Color.cyan;
        outline.effectDistance = new Vector2(2, -2);
        
        // Add button component
        Button button = cardObj.AddComponent<Button>();
        CardButton cardButton = cardObj.AddComponent<CardButton>();
        cardButton.cardData = cardData;
        cardButton.onCardClicked = OnCardClicked;
        
        // Add hover effect
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 0.7f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.5f);
        button.colors = colors;
        
        // Card Name Text
        GameObject nameObj = new GameObject("Name");
        nameObj.transform.SetParent(cardObj.transform, false);
        
        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = cardData.minionName;
        nameText.fontSize = 18;
        nameText.fontStyle = FontStyles.Bold;
        nameText.alignment = TextAlignmentOptions.Top;
        nameText.color = Color.white;
        
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.75f);
        nameRect.anchorMax = new Vector2(1, 0.95f);
        nameRect.offsetMin = new Vector2(5, 0);
        nameRect.offsetMax = new Vector2(-5, -5);
        
        // Mana Cost (BIG and prominent)
        GameObject costObj = new GameObject("ManaCost");
        costObj.transform.SetParent(cardObj.transform, false);
        
        TextMeshProUGUI costText = costObj.AddComponent<TextMeshProUGUI>();
        costText.text = $"{cardData.manaCost}";
        costText.fontSize = 48;
        costText.fontStyle = FontStyles.Bold;
        costText.alignment = TextAlignmentOptions.Center;
        costText.color = Color.cyan;
        
        RectTransform costRect = costObj.GetComponent<RectTransform>();
        costRect.anchorMin = new Vector2(0, 0.5f);
        costRect.anchorMax = new Vector2(1, 0.75f);
        costRect.offsetMin = new Vector2(5, 0);
        costRect.offsetMax = new Vector2(-5, 0);
        
        // Stats Text
        GameObject statsObj = new GameObject("Stats");
        statsObj.transform.SetParent(cardObj.transform, false);
        
        TextMeshProUGUI statsText = statsObj.AddComponent<TextMeshProUGUI>();
        statsText.text = $"ATK: {cardData.attack}\nHP: {cardData.health}\nMove: {cardData.moveRange}";
        statsText.fontSize = 16;
        statsText.alignment = TextAlignmentOptions.Center;
        statsText.color = Color.white;
        
        RectTransform statsRect = statsObj.GetComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0, 0.15f);
        statsRect.anchorMax = new Vector2(1, 0.45f);
        statsRect.offsetMin = new Vector2(5, 0);
        statsRect.offsetMax = new Vector2(-5, 0);
        
        // Type indicator
        GameObject typeObj = new GameObject("Type");
        typeObj.transform.SetParent(cardObj.transform, false);
        
        TextMeshProUGUI typeText = typeObj.AddComponent<TextMeshProUGUI>();
        typeText.text = GetMinionTypeIcon(cardData);
        typeText.fontSize = 14;
        typeText.alignment = TextAlignmentOptions.Center;
        typeText.color = Color.yellow;
        
        RectTransform typeRect = typeObj.GetComponent<RectTransform>();
        typeRect.anchorMin = new Vector2(0, 0);
        typeRect.anchorMax = new Vector2(1, 0.15f);
        typeRect.offsetMin = new Vector2(5, 5);
        typeRect.offsetMax = new Vector2(-5, 0);
        
        cardButtons.Add(cardButton);
    }
    
    string GetMinionTypeIcon(MinionData data)
    {
        switch (data.minionType)
        {
            case CHEGG.MinionType.Villager: return "👑 KING";
            case CHEGG.MinionType.Zombie: return "🧟 Melee";
            case CHEGG.MinionType.Skeleton: return "💀 Ranged";
            case CHEGG.MinionType.Creeper: return "💥 Explode";
            case CHEGG.MinionType.IronGolem: return "🛡️ Tank";
            default: return "⚔️";
        }
    }
}

public class CardButton : MonoBehaviour
{
    public MinionData cardData;
    public System.Action<MinionData> onCardClicked;
    
    public void OnClick()
    {
        Debug.Log($"CARD CLICKED: {cardData.minionName}");
        onCardClicked?.Invoke(cardData);
    }
    
    void Awake()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(OnClick);
        }
    }
}