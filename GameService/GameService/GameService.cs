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
        private int gameID = 0;
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
                        Details = $"{name} is taken, please pick another."
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
                avilableClinets.Add(name, regCallback);
                updateAllClinetToUpdateList(name);
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
            GameZone gameZone = new GameZone(p1, p2, this.avilableClinets[p1], this.avilableClinets[p2], ++gameID);
            using (var ctx = new fourinrowDB_RoniShoseov_EilonOsherContext())
            {
                DateTime localTime = DateTime.Now;

                SingleGame newGame = new SingleGame
                {
                    Id = gameID,
                    Date = localTime,
                    Player1_Name = p1,
                    Player2_Name = p2,
                    GamePoint = 0,
                    Status = true
                };
                ctx.SingleGames.Add(newGame);
                ctx.SaveChanges();

                GameZone gm = new GameZone(p1, p2, avilableClinets[p1], avilableClinets[p2], gameID);
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
            MessageBox.Show("Roni 1");

            using (var ctx = new fourinrowDB_RoniShoseov_EilonOsherContext())
            {
                var findUser = (from u in ctx.Users
                                where u.UserName == user
                                select u).FirstOrDefault();
                if (findUser == null)
                {
                    UnregisteredUser userNotExsists = new UnregisteredUser
                    {
                        Details = "{user} does not exist in the database. Please register."
                    };
                    throw new FaultException<UnregisteredUser>(userNotExsists);
                    
                }

                else if (pass != findUser.HashedPassword)
                {
                    WrongPassword userWrongPassword = new WrongPassword
                    {
                        Details = "Wrong password entered, please try again"
                    };
                    MessageBox.Show("Roni2");
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


    }
}
