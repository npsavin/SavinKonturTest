using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SavinKonturTest.Exception;
using SavinKonturTest.Model;


namespace SavinKonturTest
{
        class Program
        {
            public static void Main(string[] args)
            {
                StartGame();
            }

            public static void StartNewGame()
            {
                StartGame();
            }

            private static void StartGame()
            {
                var viewModel = new ViewModel.ViewModel();
                viewModel.StartGame();
            }
        }
}


namespace SavinKonturTest.ViewModel
{
   
    class ViewModel
    {
        public static int Score { get; private set; }
        public static int Turn { get; private set; }
        public static Deck Deck { get; private set; }
        public static Player Player1 { get; private set; }
        public static Player Player2 { get; private set; }
        public static List<Player> PlayerList { get; set; } 
        public static Table Table { get; private set; }
        
        public void StartGame()
        {
            var collectionForStart = View.View.StartGame(null);
            Score = 0;
            Turn = 0;
            Deck = CreateDeck(collectionForStart.RequestText.Split(' '));
            Player1 = CreatePlayer(Deck, true);
            Player2 = CreatePlayer(Deck, false);
            PlayerList = new List<Player>(2) {Player1, Player2};
            Table = new Table();
            var firstTurn = View.View.GetAboutTurn(MakeInformationString(false, 0).RequestText);
            MakeTurn(firstTurn);
        }

        private static void MakeTurn(View.View.Request requestAboutTurn)
        {
            ChangeTurn();
            var finish = false;
            if (requestAboutTurn.RequestType == "Play")
            {
                MakeTurn(MakeInformationString(finish, 0));
            }
            if (requestAboutTurn.RequestType == "Color")
            {
                MakeTurn(MakeInformationString(finish, 0));
            }
            if (requestAboutTurn.RequestType == "Rank")
            {
                MakeTurn(MakeInformationString(finish, 0));
            }
            if (requestAboutTurn.RequestType == "Drop")
            {
                MakeTurn(MakeInformationString(false, 0));
            }
            else
            {
                Program.StartNewGame();
            }
        }

        private static View.View.Request MakeInformationString(bool finished, int risk)
        {
            var sb = new StringBuilder();
            sb.Append("Turn: ");
            sb.Append(Turn);
            sb.Append(", Score: ");
            sb.Append(Score);
            sb.Append(", Finished: ");
            sb.Append(finished.ToString());
            sb.AppendLine();
            sb.Append("Current player:");
            var currentPlayer = PlayerList.Single(x => x.Turn);
            sb.Append(MakeStringAboutCards(currentPlayer.CardsInHand));
            sb.AppendLine();
            sb.Append("Next player:");
            var nextPlayer = PlayerList.Single(x => !x.Turn);
            sb.Append(MakeStringAboutCards(nextPlayer.CardsInHand));
            sb.AppendLine();
            sb.Append("Table:");
            sb.Append(MakeStringAboutCards(Table.CardsOnTable));
            if (0 == risk)
            {
                return View.View.GetAboutTurn(sb.ToString());
            }
            else
            {
                var newSb = new StringBuilder();
                newSb.Append("Turn: ");
                newSb.Append(Turn);
                newSb.Append(", cards: ");
                newSb.Append(Score);
                newSb.Append(", with risk: ");
                newSb.Append(risk);
                newSb.AppendLine();
                newSb.Append(sb);
                return View.View.GetAboutTurn(newSb.ToString());
            }
                
            
            
        }

        private static string MakeStringAboutCards(List<Card> cards)
        {
            var sb = new StringBuilder();
            foreach (var card in cards)
            {
                sb.Append(" ");
                sb.Append(card.Color);
                sb.Append(card.Rank);
            }
            return sb.ToString();
        }

        private static void ChangeTurn()
        {
            Player1.Turn = !Player1.Turn;
            Player2.Turn = !Player2.Turn;
            Turn++;
        }
        private static Player CreatePlayer(Deck deck, bool turn)
        {
            return new Player(deck.GetFiveCardInStartGame(), turn);
        }

        private static Deck CreateDeck(string[] cardsForDeck)
        {
            var deckCards = new List<Card>();
            foreach (var cards in cardsForDeck)
            {
                if (2 != cards.Length || !char.IsDigit(cards[1]) || !char.IsLetter(cards[0]))
                {
                    View.View.DelegateForRepeatString handler = View.View.StartGame;

                    CreateDeck(View.View.InvalidInput(handler).RequestText.Split(' '));
                }
                try
                {
                    deckCards.Add(new Card(cards[0], (int)char.GetNumericValue(cards[1])));
                }
                catch (InvaliidCardException e)
                {
                    View.View.DelegateForRepeatString handler = View.View.StartGame;
                    CreateDeck(View.View.InvalidInput(handler, e.Message).RequestText.Split(' '));
                }
                
            }
            var objectDecp = new object();
            try
            {
                objectDecp = new Deck(deckCards);
            }
            catch (InvaliidCardException e)
            {
                View.View.DelegateForRepeatString handler = View.View.StartGame;
                CreateDeck(View.View.InvalidInput(handler, e.Message).RequestText.Split(' '));
            }
            return objectDecp as Deck;
        }

        private void EndThisGame()
        {
            View.View.EndGame("");
            Program.StartNewGame();
        }
    }

}
namespace SavinKonturTest.Model
{
    internal class Card
    {
        public Card(char color, int rank)
        {
            Color = color;
            Rank = rank;
            PlayerHaveInformationAboutColor = false;
            PlayerHaveInformationAboutRank = false;
        }

       
        public bool PlayerHaveInformationAboutColor { get; set; }
        public bool PlayerHaveInformationAboutRank { get; set; }

        private int _rank;
        public int Rank
        {
            get { return _rank; }
            set
            {
                if (5 >= value && 0 <= value)
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
                if ('R' == value ||'G' == value  || 'Y' == value ||'B' == value  || 'W' == value)
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
        private const int CountOfCardInHand = 5;
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

        public List<Card> DeckCard { get; private set; }

        public Card GetOneCard()
        {
            var cardForReturn = DeckCard.FirstOrDefault();
            if (null != cardForReturn)
            {
                DeckCard.Remove(cardForReturn);
                return cardForReturn;
            }
            throw new EndThisGameException("Deck is empty");
        }

        public List<Card> GetFiveCardInStartGame()
        {
            var cardsForReturn = new List<Card>(CountOfCardInHand);
            for (var i = 0; i < CountOfCardInHand; i++)
            {
                cardsForReturn.Add(DeckCard.FirstOrDefault());
                if (null == cardsForReturn.Last())
                {
                    throw new InvaliidCardException("Deck is empty");
                }
                DeckCard.Remove(DeckCard.FirstOrDefault());
            }
            return cardsForReturn;
        }
    }

    internal class Player
    {
        public List<Card> CardsInHand { get; private set; }
        public bool Turn { get; set; }
        public Player(List<Card> cardsInHand, bool turn)
        {
            Turn = turn;
            CardsInHand = cardsInHand;
        }

        

        public void TakeCardInDeck(Card card)
        {
            CardsInHand.Add(card);
        }

        public class InformationAboutCard
        {
            public InformationAboutCard(int count, char? color = null, int? rank = null)
            {
                Count = count;
                Color = color;
                Rank = rank;
            }

            public int Count { get; private set; }
            public char? Color { get; private set; }
            public int? Rank { get; private set; }
        }
        public void GetInformationAboutCards(List<InformationAboutCard> information)
        {
            var informationAboutCard = information.FirstOrDefault();
            if (null == informationAboutCard)
            {
                throw new InvaliidCardException("Information is empty");
            }
            if (null == informationAboutCard.Color)
            {
                if (null == informationAboutCard.Rank)
                {
                    throw new InvaliidCardException("Information is empty");
                }
                foreach (var inf in information)
                {
                    if (CardsInHand[inf.Count].Rank == inf.Rank)
                    {
                        CardsInHand[inf.Count].PlayerHaveInformationAboutRank = true;
                    }
                    else
                    {
                        throw new EndThisGameException("A player has given incorrect information about the card rank");
                    }
                }
            }
            if (null == informationAboutCard.Rank)
            {
                if (null == informationAboutCard.Color)
                {
                    throw new InvaliidCardException("Information is empty");
                }
                foreach (var inf in information)
                {
                    if (CardsInHand[inf.Count].Color == inf.Color)
                    {
                        CardsInHand[inf.Count].PlayerHaveInformationAboutColor = true;
                    }
                    else
                    {
                        throw new EndThisGameException("A player has given incorrect information about the card color");
                    }
                }
            }
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
                throw new InvaliidCardException("This user has played an incorrect card");
            }
        }

       
    }

    internal class Table
    {
        private const int MinCountOfCardsOnTable = 5;
        private const int MaxCountOfCardsOnTable = 25;
        public List<Card> CardsOnTable { get; private set; }
        private int _countOfCardsOnTheTable = MinCountOfCardsOnTable;
        private readonly List<Card> _defaultCard = new List<Card>(){new Card('R', 0), new Card('G', 0), new Card('B', 0), new Card('W', 0), new Card('Y', 0)}; 
        public Table()
        {
            CardsOnTable = _defaultCard;
        }

        public void PlaceСardOnTable(Card card)
        {
            var cardOnTable = CardsOnTable.Single(x => x.Color == card.Color);
            if (1 == card.Rank - cardOnTable.Rank)
            {
                var count = CardsOnTable.IndexOf(cardOnTable);
                CardsOnTable[count] = card;
                _countOfCardsOnTheTable++;
                if (MaxCountOfCardsOnTable == _countOfCardsOnTheTable)
                {
                    throw new EndThisGameException("On the table can not be folded more cards");
                }
            }
            else
            {
                throw new EndThisGameException("The player played a wrong card");
            }
        }
    }
}

namespace SavinKonturTest.View
{
    class View
    {
        private const string StartString = "Start new game with deck ";
        private const string PlayCard = "Play card ";
        private const string TellColor = "Tell color ";
        private const string TellRank = "Tell rank ";
        private const string DropCard = "Drop card ";
        public delegate Request DelegateForRepeatString(string stringForGame);

        public class Request
        {
            public string RequestText { get; private set;}
            public string RequestType { get; private set;}

            public Request(string requestText, string requestType)
            {
                RequestText = requestText;
                RequestType = requestType;
            }
        }
        public static Request StartGame(string stringForStartGame)
        {
            DelegateForRepeatString repeatCallStartGame = StartGame;
            if (null == stringForStartGame)
            {
                stringForStartGame = Console.ReadLine();
            }
            if (null != stringForStartGame && stringForStartGame.StartsWith(StartString))
            {
                return new Request(stringForStartGame.Substring(StartString.Length), "Start");
            }
            return InvalidInput(repeatCallStartGame);
            
        }

        public static void EndGame(string stringForEnd)
        {
            Console.WriteLine(stringForEnd);
        }

        public static Request GetAboutTurn(string startInformationAboutGame)
        {
            DelegateForRepeatString repeatCallThisMethod = GetAboutTurn;
            Console.WriteLine(startInformationAboutGame);
            var informationAboutNexTurn = Console.ReadLine();
            if (null == informationAboutNexTurn)
            {
                return InvalidInput(repeatCallThisMethod);
            }
            if (informationAboutNexTurn.StartsWith(PlayCard))
            {
                return new Request(informationAboutNexTurn.Substring(PlayCard.Length), "Play");
            }
            if (informationAboutNexTurn.StartsWith(TellColor))
            {
                return new Request(informationAboutNexTurn.Substring(TellColor.Length), "Color");
            }
            if (informationAboutNexTurn.StartsWith(TellRank))
            {
                return new Request(informationAboutNexTurn.Substring(TellRank.Length), "Rank");
            }
            if (informationAboutNexTurn.StartsWith(DropCard))
            {
                return new Request(informationAboutNexTurn.Substring(DropCard.Length), "Drop");
            }
            return InvalidInput(repeatCallThisMethod);
        }
       
        public static Request InvalidInput(DelegateForRepeatString callingDelegate, string exceptionMessage)
        {
            Console.WriteLine(exceptionMessage);
            Console.WriteLine("Repeat data entry");
            var ruquestForReturn = callingDelegate(Console.ReadLine());
            return ruquestForReturn;
        }
        public static Request InvalidInput(DelegateForRepeatString callingDelegate)
        {
            Console.WriteLine("Invalid input. Repeat data entry");
            var requestForReturn = callingDelegate(Console.ReadLine());
            return requestForReturn;
        }
    }
     
}

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
   public class InvalidInputException : System.Exception
   {
       public InvalidInputException(string message)
           : base(message)
       { }
   }

}
