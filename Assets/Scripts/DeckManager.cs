using UnityEngine;
using System.Collections.Generic;

public class DeckManager : MonoBehaviour
{
    public List<CardData> allCards = new List<CardData>(); 
    public List<CardData> drawPile = new List<CardData>();
    public List<CardData> hand = new List<CardData>();
    public List<CardData> discardPile = new List<CardData>();

    public int handSize = 5;

    void Start()
    {
        InitializeDeck();
        DrawHand();
        
        
        HandManager handManager = FindFirstObjectByType<HandManager>();
        if (handManager != null)
            handManager.DisplayHand();
    }

    void InitializeDeck()
    {
        drawPile.Clear();

        foreach (CardData card in allCards)
        {
            int quantity = GetCardQuantity(card);
            for (int i = 0; i < quantity; i++)
                drawPile.Add(card);
        }

        ShuffleDeck(drawPile);
        Debug.Log("Deck initialized with " + drawPile.Count + " cards");
    }

    int GetCardQuantity(CardData card)
    {
        switch (card.cardType)
        {
            case CardType.Support: return 1; // First Aid Kit x1
            case CardType.Utility: return 3; // Extended Mag x3
            case CardType.Elemental: return 3; // each elemental x3
            case CardType.Basic: return 6; // Quick Shot x6
            case CardType.Buff: return 3; // buff cards x3
            case CardType.Tradeoff: return 2; // tradeoff cards x2
            default: return 2;
        }
    }

    void ShuffleDeck(List<CardData> deck)
    {
        for (int i = 0; i < deck.Count; i++)
        {
            CardData temp = deck[i];
            int randomIndex = Random.Range(i, deck.Count);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    public void DrawHand()
    {
        for (int i = 0; i < handSize; i++)
        {
            DrawCard();
        }
    }

    public void DrawCard()
    {
        if (drawPile.Count == 0)
        {
            ReshuffleDiscard();
        }

        if (drawPile.Count > 0)
        {
            CardData card = drawPile[0];
            drawPile.RemoveAt(0);
            hand.Add(card);
            Debug.Log("Drew card: " + card.cardName);
        }
    }

    void ReshuffleDiscard()
    {
        Debug.Log("Reshuffling discard pile!");
        drawPile = new List<CardData>(discardPile);
        discardPile.Clear();
        ShuffleDeck(drawPile);
    }

    public void DiscardHand()
    {
        discardPile.AddRange(hand);
        hand.Clear();
    }
}