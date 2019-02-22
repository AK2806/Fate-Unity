using System;
using System.Collections;
using System.Collections.Generic;
using GameService.CharacterData;
using GameService.Util;
using GameCore;
using GameCore.Network;
using GameCore.Network.ServerMessages;
using GameCore.Network.ClientMessages;

namespace GameService.ServerProxy {
    public abstract class ServerComponentProxy : IMessageReceiver {
		private readonly Connection _connectionRef;
        protected Connection ConnectionRef { get { return _connectionRef; } }

		public ServerComponentProxy(Connection connection) {
			_connectionRef = connection;
		}

		public abstract void MessageReceived(Message message);
    }

    public sealed class GameServerProxy : IMessageReceiver {
        private static readonly GameServerProxy _instance = new GameServerProxy();
        public static GameServerProxy Instance { get { return _instance; } }

        private readonly IdentifiedObjectList<User> _users = new IdentifiedObjectList<User>();
        private User _self = null;

        private readonly NetworkfConnection _connection;
        private readonly CharacterDataProxy _characterData;
        private readonly StorySceneProxy _storyScene;
        private readonly BattleSceneProxy _battleScene;

        

        public User Self { get { return _self; } }
        public StorySceneProxy StoryScene { get { return _storyScene; } }
        public BattleSceneProxy BattleScene { get { return _battleScene; } }

        public User FindUser(int id) {
            User ret;
            if (!_users.TryGetValue(id, out ret)) return null;
            else return ret;
        }

        private GameServerProxy() {
            _connection = new NetworkfConnection();
            _characterData = new CharacterDataProxy(_connection);
            _storyScene = new StorySceneProxy(_connection);
            _battleScene = new BattleSceneProxy(_connection);
            _connection.AddMessageReceiver(SyncUsersInfoMessage.MESSAGE_TYPE, this);
        }

        public bool EstablishConnection(string ip, byte[] verificationCode) {
            try {
                var ns = Networkf.NetworkHelper.StartClient(ip);
                var nsi = new NSInitializer(ns);
                bool ret = nsi.ClientInit(verificationCode, _connection);
                if (!ret) ns.TeardownService();
                return ret;
            } catch (Exception e) {
                Logger.WriteLine(e.Message);
                return false;
            }
        }

	    public void InitSyncData() {
		    _connection.SendMessage(new ClientOnlineMessage());
	    }

        public void MessageReceived(Message message) {
            switch (message.MessageType) {
                case SyncUsersInfoMessage.MESSAGE_TYPE: {
                        var msg = (SyncUsersInfoMessage)message;
                        for (int i = 0; i < msg.usersID.Length; ++i) {
                            int userID = msg.usersID[i].value;
                            int characterID = msg.charactersID[i].value;
                            var user = new User(userID, msg.usersInfo[i], characterID);
                            _users.Add(user);
                        }
                        int clientUserID = msg.clientUserID.value;
                        _self = _users[clientUserID];

                    }
                    break;
                case UserOnlineMessage.MESSAGE_TYPE: {
                        var msg = (UserOnlineMessage)message;
                        
                    }
                    break;
                case UserOfflineMessage.MESSAGE_TYPE: {

                    }
                    break;
                case UserLoadingMessage.MESSAGE_TYPE: {

                    }
                    break;
                case UserLoadCompleteMessage.MESSAGE_TYPE: {

                    }
                    break;
                case LoadAssetsMessage.MESSAGE_TYPE: {
                        var msg = (LoadAssetsMessage)message;

                    }
                    break;
            }
        }

        public void ExecuteServerCommands() {
            _connection.DispatchReceivedMessages();
        }
    }

}
