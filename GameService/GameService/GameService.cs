using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Windows;

namespace GameService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
          ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class GameServiceClass : IGameService
    {
        Dictionary<string, ICallback> avilableClinets = new Dictionary<string, ICallback>();
        Dictionary<string, GameZone> games = new Dictionary<string, GameZone>();
        public void Disconnect(string player)
        {
            //remove from avilable clinet
            avilableClinets.Remove(player);
            //if is exit from game remove the game
            if (this.games.ContainsKey(player))
                this.games.Remove(player);
            //notify all other client that is disconnected
            foreach (var callBack in avilableClinets.Values)
            {
                Thread updateOtherPlayerThread = new Thread(() =>
                {
                    callBack.OtherPlayerDisconnected(player);
                }
              );
                updateOtherPlayerThread.Start();
            }
        }

        public void Register(string name, string pass)
        {
            using (var ctx = new fourinrowDB_RoniShoseov_EilonOsherContext())
            {
                var IsExists = (from u in ctx.Users
                                where u.UserName == name
                                select u).FirstOrDefault();
                if (IsExists != null)
                {
                    ConnectedFault userNameTaken = new ConnectedFault
                    {
                        Details = name +" is taken, please pick another."
                    };
                    throw new FaultException<ConnectedFault>(userNameTaken);
                }
                User newUser = new User
                {
                    UserName = name,
                    HashedPassword = pass,
                    NumOfGames = 0,
                    NumOfLosses = 0,
                    NumOfWins = 0,
                    Points = 0
                };
                ctx.Users.Add(newUser);
                ctx.SaveChanges();

                ICallback regCallback = OperationContext.Current.GetCallbackChannel<ICallback>();
                updateAllClinetToUpdateList(name);
                avilableClinets.Add(name, regCallback);
               
            }
        }

        private bool userExist(string name)
        {

            using (var ctx = new fourinrowDB_RoniShoseov_EilonOsherContext())
            {
                var IsExists = (from u in ctx.Users
                                where u.UserName == name
                                select u).FirstOrDefault();
                if (IsExists != null)
                    return true;
            }
            return false;
        }

        private void updateAllClinetToUpdateList(string name)
        {
            foreach (var callBack in avilableClinets.Values)
            {
                Thread updateOtherPlayerThread = new Thread(() =>
                {
                    callBack.OtherPlayerSignIn(name);
                }
               );
                updateOtherPlayerThread.Start();
            }
        }

        public MoveResult ReportMove(int col, string player, Point p)
        {
            return games[player].VerifyMove(col, player, p);
        }

        public Dictionary<string, ICallback> GetAvliableClients(string user)
        {
            var ret = new Dictionary<string, ICallback>(avilableClinets);
            ret.Remove(user);
            return ret;
        }

        public bool StartGame(string by, string player)
        {
            ICallback c = this.avilableClinets[player];
            bool result = c.ConfirmGame(by);
            return result;
        }

        public void StartGameBetweenPlayers(string p1, string p2)
        {
            this.avilableClinets[p2].StartGameUser(p1);
            GameZone gameZone = new GameZone(p1, p2, this.avilableClinets[p1], this.avilableClinets[p2]);
            games.Add(p1, gameZone);
            games.Add(p2, gameZone);
            using (var ctx = new fourinrowDB_RoniShoseov_EilonOsherContext())
            {
                DateTime localTime = DateTime.Now;

                SingleGame newGame = new SingleGame
                {
                    Date = localTime,
                    Player1_UserName = p1,
                    Player2_UserName = p2,
                    GamePoint = 0,
                    Status = true
                };
                ctx.SingleGames.Add(newGame);
                ctx.SaveChanges();
            }
            updateAllOtherUserToUpdateList(p1, p2);
        }

        private void updateAllOtherUserToUpdateList(string p1, string p2)
        {
            foreach (var pair in avilableClinets)
            {
                if (!pair.Key.Equals(p1) && !pair.Key.Equals(p2))
                    pair.Value.OtherPlayerStartedGame(p1, p2);
            }
        }

        public Dictionary<string, ICallback> GetAvliableClientsForUser(string user)
        {
            Dictionary<string, ICallback> temp = new Dictionary<string, ICallback>(this.avilableClinets);
            temp.Remove(user);
            temp = filterUserThatPlaying(temp);
            return temp;
        }

        private Dictionary<string, ICallback> filterUserThatPlaying(Dictionary<string, ICallback> list)
        {
            Dictionary<string, ICallback> temp = new Dictionary<string, ICallback>();
            foreach (var key in list.Keys)
            {
                if (!games.ContainsKey(key))
                {
                    temp.Add(key, list[key]);
                }
            }
            return temp;
        }

        public void PlayerRetrunToList(string player)
        {
            if (this.avilableClinets[player] != null)
                this.avilableClinets.Remove(player);
            this.updateAllClinetToUpdateList(player);
        }

        public void SingIn(string user, string pass)
        {
            if (avilableClinets.ContainsKey(user))
            {
                ConnectedFault userExsists = new ConnectedFault
                {

                    Details = "User name " + user + " already exists. Try something else"
                };
                throw new FaultException<ConnectedFault>(userExsists);
            }
            using (var ctx = new fourinrowDB_RoniShoseov_EilonOsherContext())
            {
                var findUser = (from u in ctx.Users
                                where u.UserName == user
                                select u).FirstOrDefault();
                if (findUser == null)
                {
                    UnregisteredUser userNotExsists = new UnregisteredUser
                    {
                        Details = user + "does not exist in the database. Please register."
                    };
                    throw new FaultException<UnregisteredUser>(userNotExsists);
                    
                }

                else if (pass != findUser.HashedPassword)
                {
                    WrongPassword userWrongPassword = new WrongPassword
                    {
                        Details = "Wrong password entered, please try again"
                    };
                    throw new FaultException<WrongPassword>(userWrongPassword);
                }
                else
                {
                    ICallback singIncallback = OperationContext.Current.GetCallbackChannel<ICallback>();
                    avilableClinets.Add(user, singIncallback);
                    updateAllClinetToUpdateList(user);



                }
            }
        }

        private bool isValidUser(string user, string pass)
        {
            //need to implnet check in data base if is cuurect
            return false;
        }

        public List<string> createPlayerData()
        {
            using (var ctx = new fourinrowDB_RoniShoseov_EilonOsherContext())
            {
                var userData = (from u in ctx.Users
                                select u.UserName).ToList();
                return userData;
            }
        }

        public List<string> gameDataBetween(string Player1, string Player2)
        {
            using (var ctx = new fourinrowDB_RoniShoseov_EilonOsherContext())
            {
                List<string> dataBetween = new List<string>();
                var match = (from g in ctx.SingleGames
                                    where (((g.Player1_UserName == Player1 && g.Player2_UserName == Player2)
                                            || (g.Player1_UserName == Player2 && g.Player2_UserName == Player1))
                                            && g.Status == false)
                                    select g).ToList();
                if (match.Count == 0) return dataBetween;
                dataBetween.Add(percentageOfWins(match, Player1).ToString());
                dataBetween.Add(percentageOfWins(match, Player2).ToString());
                foreach (var game in match)
                {
                    dataBetween.Add(
                        $"Game ID: {game.Id.ToString()}\n" +
                        $"Date: {game.Date.ToString()}\n" +
                        $"Players: {game.Player1_UserName} vs. {game.Player2_UserName}\n" +
                        $"♛{game.Winner} WON! With {game.GamePoint.ToString()} points!\n" +
                        $"▁ ▂ ღ(¯`◕‿◕´¯)(¯`◕‿◕´¯)ღ ▂ ▁"
                    );
                }
                return dataBetween;
            }
        }


        private double percentageOfWins(List<SingleGame> list, string player)
        {
            int wins = 0;
            foreach (var game in list)
            {
                if (game.Winner == player) wins++;
            }
            return ((double)wins / list.Count()) * 100;
        }



        public List<string> liveGamesList()
        {
            using (var ctx = new fourinrowDB_RoniShoseov_EilonOsherContext())
            {
                List<string> liveGamesData = new List<string>();
                var LiveGames = (from g in ctx.SingleGames
                                 where g.Status == true
                                 select g).ToList();
                if (LiveGames.Count == 0) return liveGamesData;
                foreach (var game in LiveGames)
                {
                    liveGamesData.Add(
                        $"Start Time: {game.Date.ToString()}\n" +
                        $"Players: {game.Player1_UserName} vs. {game.Player2_UserName}\n"
                        );
                }
                return liveGamesData;
            }
        }

        public List<string> gamesHistory()
        {
            List<string> gamesHistory = new List<string>();
            using (var ctx = new fourinrowDB_RoniShoseov_EilonOsherContext())
            {
                var GamesHistory = (from g in ctx.SingleGames
                                where g.GamePoint != 0 && g.Status == false
                                select g).ToList();

                foreach (var game in GamesHistory)
                {
                    gamesHistory.Add(
                        $"Game ID: {game.Id.ToString()}\n" +
                        $"Date: {game.Date.ToString()}\n" +
                        $"Players: {game.Player1_UserName} vs. {game.Player2_UserName}\n" +
                        $"♛{game.Winner} WON! With {game.GamePoint.ToString()} points!\n" +
                        $"▁ ▂ ღ(¯`◕‿◕´¯)(¯`◕‿◕´¯)ღ ▂ ▁"
                        );
                }
                return gamesHistory;
            }

        }

        public Dictionary<string, string> userData(string name)
        {
            using (var ctx = new fourinrowDB_RoniShoseov_EilonOsherContext())
            {
                Dictionary<string, string> userData = new Dictionary<string, string>();
                var userStats = (from u in ctx.Users
                                 where u.UserName == name
                                 select u).FirstOrDefault();
                if (userStats == null) return userData;
                userData.Add("User", userStats.UserName);
                userData.Add("Games", userStats.NumOfGames.ToString());
                userData.Add("Wins", userStats.NumOfWins.ToString());
                userData.Add("Losses", userStats.NumOfLosses.ToString());
                userData.Add("Points", userStats.Points.ToString());
                return userData;
            }
        }

        private List<string> resultToString(List<User> list, string sortBy)
        {
            List<string> listStr = new List<string>();
            switch (sortBy)
            {
                case "Number of Games":
                    foreach (var user in list)
                    {
                        listStr.Add($"{user.UserName} ; Games: {user.NumOfGames} ; Wins: {user.NumOfWins} ; Loses: {user.NumOfLosses} ; Points: {user.Points}");
                    }
                    return listStr;
                case "Number of Wins":
                    foreach (var user in list)
                    {
                        listStr.Add($"{user.UserName} ; Wins: {user.NumOfWins} ; Games: {user.NumOfGames} ; Loses: {user.NumOfLosses} ; Points: {user.Points}");
                    }
                    return listStr;
                case "Number of Losses":
                    foreach (var user in list)
                    {
                        listStr.Add($"{user.UserName} ; Loses: {user.NumOfLosses} ; Games: {user.NumOfGames} ; Wins: {user.NumOfWins} ; Points: {user.Points}");
                    }
                    return listStr;
                case "Number of Points":
                    foreach (var user in list)
                    {
                        listStr.Add($"{user.UserName} ; Points: {user.Points} ; Games: {user.NumOfGames} ; Wins: {user.NumOfWins} ; Loses: {user.NumOfLosses}");
                    }
                    return listStr;
                default:
                    return null;
            }
        }

        public List<string> getSortedList(string sortBy)
        {
            using (var ctx = new fourinrowDB_RoniShoseov_EilonOsherContext())
            {
                switch (sortBy)
                {
                    case "Username":
                        var byUsername = (from u in ctx.Users
                                      orderby u.UserName descending
                                      select u.UserName).ToList();
                        return byUsername;
                    case "Number of Games":
                        var byNumOfGames = (from u in ctx.Users
                                       orderby u.NumOfGames descending
                                       select u).ToList();
                        return resultToString(byNumOfGames, sortBy);
                    case "Number of Wins":
                        var byNumOfWins = (from u in ctx.Users
                                      orderby u.NumOfWins descending
                                      select u).ToList();
                        return resultToString(byNumOfWins, sortBy);
                    case "Number of Losses":
                        var byNumOfLooses = (from u in ctx.Users
                                        orderby u.NumOfLosses descending
                                        select u).ToList();
                        return resultToString(byNumOfLooses, sortBy);
                    case "Number of Points":
                        var byNumOfPoints = (from u in ctx.Users
                                        orderby u.Points descending
                                        select u).ToList();
                        return resultToString(byNumOfPoints, sortBy);
                    default:
                        return null;
                }
            }
        }


    }
}
