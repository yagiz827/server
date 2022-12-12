using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace server
{
    public class Game
    {
        private List<Player> Players;
        private Socket ServerSocket;
        private Quiz Quiz;
        private Thread AcceptConnectionsThread;
        private bool IsServerListening;
        private bool IsGameEnding;
        private bool IsGameStarted;
        private RichTextBox LogTextBox;
        private int numberOfQuestionsTobeAskeed;
        public bool Isclicked=false;

        public Game(RichTextBox logTextBox)
        {
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Players = new List<Player>();
            IsServerListening = false;
            IsGameEnding = false;
            LogTextBox = logTextBox;
        }

        private void Log(string message)
        {
            LogTextBox.AppendText(message + "\n");
        }

        public void AcceptConnections()
        {
            while (IsServerListening)
            {
                var clientSocket = ServerSocket.Accept();
                if (!ShouldAcceptConnection())
                {
                    Log("2 players are already connected, won't accept connection");
                    clientSocket.Close();
                }
                else
                {
                    Log("A new client is connected. Starting listening...");
                    IsGameEnding = false;
                    var thread = new Thread(() => StartToListenNewClient(clientSocket));
                    thread.Start();
                }


            }
        }

        private void StartToListenNewClient(Socket clientSocket)
        {
            Player player = null;
            while (IsSocketConnected(clientSocket) && !IsGameEnding)
            {
                if (clientSocket.Available > 0)
                {
                    var buffer = new byte[64];
                    clientSocket.Receive(buffer, 0);
                    var incomingMessage = Encoding.UTF8.GetString(buffer);
                    incomingMessage = incomingMessage.Substring(0, incomingMessage.IndexOf("\0"));
                    var messageSegments = incomingMessage.Split(',');
                    var messageType = messageSegments[0];
                    if (messageType == "Name")
                    {
                        var name = messageSegments[1];
                        if (ShouldAcceptConnection())
                        {
                            player = AddPlayer(name, clientSocket);
                            if (player != null)
                            {
                                Log("Player " + name + " connected.");
                                if (ShouldGameStart(Isclicked))
                                {
                                    
                                }
                                else
                                {
                                    SendWaitingForSecondPlayerMessage();
                                }
                            }
                            else
                            {
                                SendNameNotUniqueMessage(clientSocket);
                            }
                        }
                        else
                        {
                            clientSocket.Close();
                            Log("There are already 200 players. Player " + name + " will be disconnected");
                        }

                    }
                    else if (player != null && messageType == "Answer" && IsGameStarted)
                    {
                        var answer = int.Parse(messageSegments[1]);
                        if (ShouldAcceptAnswer(player))
                        {
                            Log("Player " + player.Name + " responded with: " + answer);
                            player.AddAnswer(answer);
                        }
                        if (IsTurnEnded())
                        {
                            var trueAnswer = Quiz.GetAnswer();

                            UpdateScores(trueAnswer);
                            PrintScoreBoard(true, trueAnswer);
                            DeleteAnswers();
                            
                            if (!Quiz.IsQuizFinished())
                            {
                                AskQuestion();

                            }
                            else
                            {
                                DeclareWinner();
                                ResetGame(numberOfQuestionsTobeAskeed);
                            }
                        }
                    }
                }
            }
            if (player != null)
            {
                PlayerDisconnected(player);
                IsGameEnding = true;
            }
        }

        private void PlayerDisconnected(Player player)
        {
            var message = player.Name + " disconnected";
            var scr= player.Name;
            Log(message);
            Log(scr);
            Players.Remove(player);

            if (Players.Count == 1)
            {
                Players[0].SendMessage(message);
                PrintScoreBoard(false, 0);
                Players[0].SendMessage("Player " + scr + " has 0 points.");
              
                DeclareWinner();
            }
            ResetGame(numberOfQuestionsTobeAskeed);
        }

        private bool IsSocketConnected(Socket socket)
        {
            try
            {
                if (!socket.Connected)
                    return false;

                bool part1 = socket.Poll(1000, SelectMode.SelectRead);
                bool part2 = (socket.Available == 0);
                return !(part1 && part2);
            }
            catch
            {
                return false;
            }
        }
        private void DeclareWinner()
        {
            string message = "";
            var maxPoint = Players.Select(x => x.Score).Max();
            var winners = Players.Where(x => x.Score == maxPoint).ToList();
            if (winners.Count == 2)
            {
                message = "Game ended with a tie.";
            }
            else
            {
                message = "Player " + winners.First().Name + " has won!";
            }
            Log(message);
            SendMessageToAllPlayers(message);
        }

        private void SendMessageToAllPlayers(string message)
        {
            foreach (var player in Players)
            {
                player.SendMessage(message);
            }
        }

        public void ResetGame(int NumberofQues)
        {
            IsGameEnding = true;

            foreach (var player in Players)
            {
                player.CloseConnection();
            }
            Players.Clear();
            
            Quiz.ResetQuiz(NumberofQues);


        }

        private void UpdateScores(int trueAnswer)
        {
            var distancesToTrueAnswer = new List<Tuple<Player, int>>();
            foreach (var player in Players)
            {
                var distance = Math.Abs(trueAnswer - player.Answers[player.Answers.Count - 1]);
                distancesToTrueAnswer.Add(new Tuple<Player, int>(player, distance));
            }
            var distancesOrdered = distancesToTrueAnswer.OrderByDescending(x => x.Item2);
            var maxPoint = distancesOrdered.Min(x => x.Item2);
            var winners = distancesOrdered.Where(x => x.Item2 == maxPoint).Select(x => x.Item1).ToList();
            var numberOfWinners = winners.Count;
            var pointsPerPlayer = 1.0m / numberOfWinners;
            foreach (var player in winners)
            {
                player.AddPoints(pointsPerPlayer);
            }
            var message = "";
            message += "True answer to last question is " + trueAnswer + ".\n";
            if (numberOfWinners == 1)
            {
                
                var roundWinner = winners[0];
                message += "player " + winners[0].Name + " won the round" + ".\n";
                Log(message);
            }
            else
            {
                message += "There were multiple winners this round "+".\n";
                message += "Players that won the round are " + ".\n";
                foreach (var player in winners)
                {
                    message += player.Name+" ";
                }
                Log(message);
            }
            SortPlayersByScore();

        }

        private void SortPlayersByScore()
        {
            Players = Players.OrderByDescending(x => x.Score).ToList();
        }
        private void DeleteAnswers()
        {
            foreach (var player in Players)
            {
                player.Answers.Clear();
            }
        }

        private void PrintScoreBoard(bool takeAnswerIntoConsideration, int trueAnswer)
        {
            var message = "";
            
            if (takeAnswerIntoConsideration)
            {
      
                foreach (var player in Players)
                {
                    message += "Player " + player.Name+ " responded: " + player.Answers.Last() + ".\n";

                }
                
                
            }

            message += "Current points as following:\n";
            foreach (var player in Players)
            {
                message += player.GetScoreMessage();
            }
            foreach (var player in Players)
            {
                player.SendMessage(message);
            }
            Log(message);

        }

        private bool IsTurnEnded()
        {
            foreach (var player in Players)
            {
                if (player.Answers.Count == 0)
                {
                    return false;
                }
            }
            return true;
        }

        private bool ShouldAcceptAnswer(Player responder)
        {
            int maxAnswerCountAmongPlayers = 0;
            foreach (var player in Players)
            {
                if (responder != player && maxAnswerCountAmongPlayers < player.Answers.Count)
                {
                    maxAnswerCountAmongPlayers = player.Answers.Count;
                }
            }
            return responder.Answers.Count <= maxAnswerCountAmongPlayers;
        }

        private void AskQuestion()
        {
            var question = Quiz.GetQuestion();
            foreach (var player in Players)
            {
                player.AskQuestion(question);
            }
            Log("Asked the following question: " + question);

        }

        private bool ShouldAcceptConnection()
        {
            var shouldAcceptConnection = Players.Count < 200;
            return shouldAcceptConnection;
        }

        public bool StartServer(string fileName, int numberOfQuestionsToBeAsked, int port)
        {
            if (numberOfQuestionsToBeAsked > 0 && port > 0)
            {
                numberOfQuestionsTobeAskeed= numberOfQuestionsToBeAsked;
                Quiz = new Quiz(fileName, numberOfQuestionsToBeAsked);
                var endPoint = new IPEndPoint(IPAddress.Any, port);
                ServerSocket.Bind(endPoint);
                ServerSocket.Listen(3);
                IsServerListening = true;
                AcceptConnectionsThread = new Thread(AcceptConnections);
                AcceptConnectionsThread.Start();
                return true;
            }
            else
            {
                return false;
            }
        }



        private Player AddPlayer(string playerName, Socket playerSocket)
        {
            if (CheckIfPlayerNameIsUnique(playerName))
            {
                var player = new Player(playerName, playerSocket);
                Players.Add(player);
                return player;
            }
            else
            {
                return null;
            }
        }

        private bool CheckIfPlayerNameIsUnique(string playerName)
        {
            foreach (var player in Players)
            {
                var isNameEqual = player.Name == playerName;
                if (isNameEqual)
                {
                    return false;
                }
            }
            return true;
        }

        public void SendNameNotUniqueMessage(Socket socket)
        {
            var message = "Your name is already used. Try connecting with different name again";
            Log("Player with already used name connected. Going to close connection.");
            var messageBuffer = Encoding.Default.GetBytes(message);
            socket.Send(messageBuffer);
            socket.Close();
        }

        public bool ShouldGameStart(bool Isclicked)
        {
            var playerCount = Players.Count;
            if (playerCount>=1 && Isclicked)
            {
                SendGameStartingMessage();
                AskQuestion();
                IsGameStarted = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void SendGameStartingMessage()
        {
            foreach (var player in Players)
            {
                player.SendGameStartingMessage();
            }
            Log("Game starting...");
        }
        private void SendWaitingForSecondPlayerMessage()
        {
            foreach (var player in Players)
            {
                player.SendWaitingForSecondPlayerMessage();
            }
            Log("Waiting for second player...");
        }
    }
}
