using GameCore;
using GameService.CharacterData;
using System;

namespace GameService.Util {
	public class User : IIdentifiable, IEquatable<User> {
		private readonly int _id;
		private readonly string _accountID;
		private readonly string _name;
		private readonly Guid _avatar;
		private readonly int _characterID;
		private Character _characterRef = null;
		
		public int ID { get { return _id; } }
		public string AccountID { get { return _accountID; } }
		public string Name { get { return _name; } }
		public Guid Avatar { get { return _avatar; } }
		public Character Character { get { return _characterRef ?? (_characterID != -1 ? _characterRef = CharacterManager.Instance.FindCharacter(_characterID) : null); } }
		public bool IsDM { get { return _characterID == -1; } }

		public User(int id, UserInfo userInfo, int characterID) {
			_id = id;
			_accountID = userInfo.id;
			_name = userInfo.name;
			_avatar = userInfo.avatar;
			if (userInfo.dm) characterID = -1;
			else _characterID = characterID;
		}

		public override bool Equals(object obj) {
			return Equals(obj as User);
		}

		public override int GetHashCode() {
			return _id;
		}

		public bool Equals(User other) {
			return other != null && _id == other._id;
		}
	}
}
