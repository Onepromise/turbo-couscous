using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Deck.cs - Manages a player's deck of cards
/// 
/// WHY: Separating deck logic makes it easy to:
/// - Shuffle cards fairly
/// - Draw cards
/// - Track remaining cards
/// - Potentially add "reshuffle graveyard" mechanics later
/// 
/// This uses the Fisher-Yates shuffle algorithm for fair randomization
/// </summary>

namespace Chegg
{
    [System.Serializable]
    public class Deck
    {
        // ==================== PROPERTIES ====================
        
        /// <summary>
        /// Cards still in the deck (not yet drawn)
        /// </summary>
        private List<MinionType> remainingCards = new List<MinionType>();
        
        /// <summary>
        /// Cards that have been drawn (could be used for graveyard mechanics)
        /// </summary>
        private List<MinionType> drawnCards = new List<MinionType>();
        
        /// <summary>
        /// How many cards are left to draw?
        /// </summary>
        public int RemainingCards => remainingCards.Count;
        
        /// <summary>
        /// Is the deck empty?
        /// </summary>
        public bool IsEmpty => remainingCards.Count == 0;
        
        // ==================== INITIALIZATION ====================
        
        /// <summary>
        /// Set up the deck with a list of cards and shuffle them
        /// </summary>
        /// <param name="cardList">List of 15 minion types for the deck</param>
        public void Initialize(List<MinionType> cardList)
        {
            // Copy the card list (don't modify the original)
            remainingCards = new List<MinionType>(cardList);
            drawnCards.Clear();
            
            // Shuffle the deck
            Shuffle();
            
            Debug.Log($"Deck initialized with {remainingCards.Count} cards");
        }
        
        /// <summary>
        /// Shuffle the remaining cards using Fisher-Yates algorithm
        /// This ensures a fair, unbiased shuffle
        /// </summary>
        public void Shuffle()
        {
            // Fisher-Yates shuffle algorithm
            // WHY: This is proven to give every possible permutation equal probability
            
            int n = remainingCards.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                
                // Swap elements at positions k and n
                MinionType temp = remainingCards[k];
                remainingCards[k] = remainingCards[n];
                remainingCards[n] = temp;
            }
            
            Debug.Log("Deck shuffled");
        }
        
        // ==================== CARD DRAWING ====================
        
        /// <summary>
        /// Draw the top card from the deck
        /// Returns the card type, or null if deck is empty
        /// </summary>
        public MinionType? Draw()
        {
            if (IsEmpty)
            {
                Debug.LogWarning("Tried to draw from empty deck!");
                return null;
            }
            
            // Take the first card
            MinionType card = remainingCards[0];
            remainingCards.RemoveAt(0);
            
            // Add to drawn pile
            drawnCards.Add(card);
            
            Debug.Log($"Drew {card} - {remainingCards.Count} cards remaining");
            
            return card;
        }
        
        /// <summary>
        /// Draw multiple cards at once
        /// Used for starting hand (draw 3 cards)
        /// </summary>
        public List<MinionType> DrawMultiple(int count)
        {
            List<MinionType> drawn = new List<MinionType>();
            
            for (int i = 0; i < count; i++)
            {
                MinionType? card = Draw();
                if (card.HasValue)
                {
                    drawn.Add(card.Value);
                }
                else
                {
                    break; // Stop if deck is empty
                }
            }
            
            return drawn;
        }
        
        // ==================== UTILITY ====================
        
        /// <summary>
        /// Peek at the top card without drawing it
        /// Useful for UI or special abilities
        /// </summary>
        public MinionType? PeekTop()
        {
            if (IsEmpty)
                return null;
            
            return remainingCards[0];
        }
        
        /// <summary>
        /// Get a copy of all remaining cards (for UI display)
        /// </summary>
        public List<MinionType> GetRemainingCards()
        {
            return new List<MinionType>(remainingCards);
        }
        
        /// <summary>
        /// Reshuffle the graveyard back into the deck
        /// Could be useful for special abilities or long games
        /// </summary>
        public void ReshuffleGraveyard()
        {
            remainingCards.AddRange(drawnCards);
            drawnCards.Clear();
            Shuffle();
            
            Debug.Log("Graveyard reshuffled into deck");
        }
    }
}