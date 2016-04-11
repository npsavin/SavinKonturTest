using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SavinKonturTest.Exception;


namespace SavinKonturTest
{
    class Savin_Nikita
    {
        static void Main(string[] args)
        {
        }
    }
}
#region Model

namespace SavinKonturTest.Model
{

    internal class Card
    {

        public Card(int rank, char color)
        {
            Rank = rank;
            Color = color;
            InformationAboutColor = false;
            InformationAboutRank = false;
        }

       
        public bool InformationAboutColor { get; private set; }
        public bool InformationAboutRank { get; private set; }

        public void SetInformationAboutColor(char color, Card card)
        {
            if (color == card.Color)
            {
                card.InformationAboutColor = true;
            }
            throw new EndThisGameException("The user gave incorrect information");
        }
        public void SetInformationAboutRank(int rank, Card card)
        {
            if (rank == card.Rank)
            {
                card.InformationAboutRank = true;
            }
            throw new EndThisGameException("The user gave incorrect information");
        }

        private int _rank;
        public int Rank
        {
            get { return _rank; }
            set
            {
                if (value <= 5 && value >= 1)
                {
                    _rank = value;
                }
                else
                {
                    throw new InvaliidCardException("Incorrect rank of card " + value);
                }
            }
        }

        private char _color;

        public char Color
        {
            get { return _color; }
            set
            {
                if (value == 'R' || value == 'G' || value == 'Y' || value == 'B' || value == 'W')
                {
                    _color = value;
                }
                else
                {
                    throw new InvaliidCardException("Incorrect color of card " + value);
                }
            }
        }
    }

    internal class Deck
    {
        public Deck(List<Card> deckCard)
        {
            if (deckCard.Count >= 11)
            {
                DeckCard = deckCard;
            }
            else
            {
                throw new InvaliidCardException("Incorrect count of card in deck " + deckCard.Count);
            }
        }

        private List<Card> DeckCard { get; set; }

        public Card GetOneCard()
        {
            var cardForReturn = DeckCard.FirstOrDefault();
            if (cardForReturn != null)
            {
                DeckCard.Remove(cardForReturn);
                return cardForReturn;
            }
            throw new EndThisGameException("Deck is empty");
        }
    }

    internal class Player
    {
        public Player(List<Card> cardsInHand)
        {
            CardsInHand = cardsInHand;
        }

        public List<Card> CardsInHand { get; private set; }

        public void TakeCardInDeck(Card card)
        {
            CardsInHand.Add(card);
        }

        public Card PlaceCardOnTable(int count)
        {
            try
            {
                var cardForReturn = CardsInHand[count];
                CardsInHand.Remove(cardForReturn);
                return cardForReturn;
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new EndThisGameException("This user has played an incorrect card");
            }
        }
    }

    internal class Table
    {
        public List<Card> CardsOnTable { get; set; } 
    }
}


#endregion

#region Exception
namespace SavinKonturTest.Exception
{
   public class InvaliidCardException : System.Exception
    {
        public InvaliidCardException(string message)
            : base(message)
        { }
    }
   public class EndThisGameException : System.Exception
   {
       public EndThisGameException(string message)
           : base(message)
       { }
   }

}
#endregion