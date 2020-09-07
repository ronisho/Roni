using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Windows;

namespace GameService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService" in both code and config file together.
    [ServiceContract(CallbackContract = typeof(ICallback))]
    public interface IGameService
    {
        [OperationContract]
        [FaultContract(typeof(ConnectedFault))]
        void Register(string user,string pass);

        [OperationContract]
        [FaultContract(typeof(ConnectedFault))]
        [FaultContract(typeof(UnregisteredUser))]
        [FaultContract(typeof(WrongPassword))]
        void SingIn(string user, string pass);

        [OperationContract]
        MoveResult ReportMove(int location, string player, Point p);

        [OperationContract]
        void Disconnect(string player);

        [OperationContract]
        bool StartGame(string by,string player);

        [OperationContract]
        void PlayerRetrunToList(string player);

        [OperationContract]
        void StartGameBetweenPlayers(string p1, string p2);

        [OperationContract]
        Dictionary<string, ICallback> GetAvliableClients(string user);

        [OperationContract]
        Dictionary<string, ICallback> GetAvliableClientsForUser(string user);


    }

    public interface ICallback
    {
        [OperationContract(IsOneWay = true)]
        void OtherPlayerDisconnected(string name);
        [OperationContract(IsOneWay = true)]
        void OtherPlayerStartedGame(string user1, string user2);

        [OperationContract(IsOneWay = true)]
        void OtherPlayerMoved(MoveResult moveResult, int row, int col, Point p);

        [OperationContract(IsOneWay = true)]
        void StartGameUser(string p1);

        [OperationContract(IsOneWay = true)]
        void OtherPlayerSignIn(string name);

        [OperationContract]
        bool ConfirmGame(string userToGame);

    }
}
