using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
            var game = new Game();
            game.RecursiveFunctionGame();
        }
    }

    class Game
    {
        ViewModel.ViewModel Vm { get; set; }
        public Game()
        {
            Vm = new ViewModel.ViewModel();
        }

        public void RecursiveFunctionGame()
        {
            while (true)
            {
                var command = View.View.GetAboutTurn();
                var flag = Vm.MakeTurn(command);
                
                
                if (flag)
                {
                    var inform = Vm.MakeInformationString(Vm.Finish, Vm.Risk);
                    View.View.WriteInfoAboutTurn(inform);
                }
                else
                {
                    continue;
                }
            }
        }
    }
}


namespace SavinKonturTest.ViewModel
{

    internal class ViewModel
    {

        public  int Score { get; private set; }
        public  int Turn { get; private set; }
        public  Deck Deck { get; private set; }
        public  Player Player1 { get; private set; }
        public Player Player2 { get; private set; }
        public List<Player> PlayerList { get; set; }
        public  Table Table { get; private set; }
        public bool Finish { get; private set; }
        public int Risk { get; private set; }
        private bool _start = false;

        public bool StartGame(string collectionForStart)
        {
            Score = 0;
            Turn = 0;
            Finish = false;
            Risk = 0;
            Deck = CreateDeck(collectionForStart.Split(' '));
            if (null == Deck)
            {
                return false;
            }
            Player1 = CreatePlayer(Deck, true);
            Player2 = CreatePlayer(Deck, false);
            PlayerList = new List<Player>(2) {Player1, Player2};
            Table = new Table();
            _start = true;
            return true;
        }

        public bool MakeTurn(View.View.Request requestAboutTurn)
        {
            try
            {
                if (requestAboutTurn.RequestType == "Start")
                {
                    var startString = StartGame(requestAboutTurn.RequestText);
                    return startString;
                }
                else if (requestAboutTurn.RequestType == "Play")
                {
                    if (!_start)
                    {
                        View.View.InvalidInput();
                        return false;
                    }
                        return PlayCard(requestAboutTurn.RequestText);
                }
                else if (requestAboutTurn.RequestType == "Color")
                {
                    if (!_start)
                    {
                        View.View.InvalidInput();
                        return false;
                    }
                }
                else if (requestAboutTurn.RequestType == "Rank")
                {
                    if (!_start)
                    {
                        View.View.InvalidInput();
                        return false;
                    }
                    return true;
                }
                else if (requestAboutTurn.RequestType == "Drop")
                {
                    if (!_start)
                    {
                        View.View.InvalidInput();
                        return false;
                    }
                    var cardForDrop =
                        PlayerList.Single(x => x.Turn)
                            .PlaceCardOnTable(Convert.ToInt32(requestAboutTurn.RequestText));
                    PlayerList.Single(x => x.Turn).TakeCardInDeck(cardForDrop);
                    return true;
                }
                else
                {
                    Program.StartNewGame();
                }
            }

            catch (EndThisGameException)
            {

                EndThisGame();
            }
            catch (InvaliidCardException)
            {
                EndThisGame();
            }
            return false;
        }

        public bool PlayCard(string requestAboutTurn)
        {
            var riskFlag=false;
            Turn++;
            try
            {
                var cardForPlay =
                    PlayerList.Single(x => x.Turn)
                        .PlaceCardOnTable(Convert.ToInt32(requestAboutTurn));
                if (false == cardForPlay.PlayerHaveInformationAboutColor)
                {
                    riskFlag = true;
                }
                if (false == cardForPlay.PlayerHaveInformationAboutRank)
                {
                    riskFlag = true;
                }
                
                Table.PlaceСardOnTable(cardForPlay);
                if (riskFlag)
                {
                    Risk++;
                }
                ChangeTurn();
                Score++;
                return true;
            }
            catch (InvaliidCardException e)
            {
                View.View.InvalidInput(e.Message);
                return false;
            }
            catch (EndThisGameException)
            {
                EndThisGame();
            }
            View.View.InvalidInput();
            return false;
        }
        public string MakeInformationString(bool finished, int risk)
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
                    return sb.ToString();
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
                    return newSb.ToString();
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

        private void ChangeTurn()
        {
            Player1.Turn = !Player1.Turn;
            Player2.Turn = !Player2.Turn;
            
        }

        private static Player CreatePlayer(Deck deck, bool turn)
        {
            return new Player(deck.GetFiveCardInStartGame(), turn);
        }

        private Deck CreateDeck(string[] cardsForDeck)
        {
            var deckCards = new List<Card>();
            foreach (var cards in cardsForDeck)
            {
                if (2 != cards.Length || !char.IsDigit(cards[1]) || !char.IsLetter(cards[0]))
                {
                    View.View.InvalidInput();
                    return null;
                }
                try
                {
                    deckCards.Add(new Card(cards[0], (int) char.GetNumericValue(cards[1])));
                }
                catch (InvaliidCardException e)
                {
                    View.View.InvalidInput(e.Message);
                    return null;
                }

            }
            var objectDecp = new object();
            try
            {
                objectDecp = new Deck(deckCards);
            }
            catch (InvaliidCardException e)
            {
                View.View.InvalidInput(e.Message);
                return null;

            }
            return objectDecp as Deck;
        }

        private string MakeFinishString()
        {
            var newSb = new StringBuilder();
            newSb.Append("Turn: ");
            newSb.Append(Turn);
            newSb.Append(", cards: ");
            newSb.Append(Score);
            newSb.Append(", with risk: ");
            newSb.Append(Risk);
            return newSb.ToString();
        }
        private void EndThisGame()
        {
            Finish = true;
            var inform = MakeFinishString();
            View.View.WriteInfoAboutTurn("END!"+inform);
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
                if ('R' == value || 'G' == value || 'Y' == value || 'B' == value || 'W' == value)
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
            if (deckCard.Any(x => 0 == x.Rank))
            {
                throw new InvaliidCardException("Deck can't have card with 0 rank");
            }
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
        private readonly List<Card> _defaultCard = new List<Card>() { new Card('R', 0), new Card('G', 0), new Card('B', 0), new Card('W', 0), new Card('Y', 0) };
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
        public delegate Request DelegateForRepeatString();

        public class Request
        {
            public string RequestText { get; private set; }
            public string RequestType { get; private set; }

            public Request(string requestText, string requestType)
            {
                RequestText = requestText;
                RequestType = requestType;
            }
        }


        public static void EndGame(string stringForEnd)
        {
            Console.WriteLine(stringForEnd);
        }

        public static void WriteInfoAboutTurn(string text)
        {
            Console.WriteLine(text);
        }
        public static Request GetAboutTurn()
        {
            DelegateForRepeatString repeatCallThisMethod = GetAboutTurn;
            var informationAboutNexTurn = Console.ReadLine();
            if (null == informationAboutNexTurn)
            {
                InvalidInput();
                return GetAboutTurn();
            }
            if (informationAboutNexTurn.StartsWith(StartString))
            {
                return new Request(informationAboutNexTurn.Substring(StartString.Length), "Start");
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
            InvalidInput();
            return GetAboutTurn();
        }

        public static void InvalidInput(string exceptionMessage)
        {
            Console.WriteLine(exceptionMessage);
            Console.WriteLine("Repeat data entry");
        }
        public static void InvalidInput()
        {
            Console.WriteLine("Invalid input. Repeat data entry");
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
