using GameService.Util;
using GameCore;
using System;
using System.Collections.Generic;

namespace GameService.CharacterData {
	public sealed class Stunt : IIdentifiable {
		private readonly int _id;
		private string _name = "";
		private string _description = "";

		public Stunt(int id) {
			_id = id;
		}

		public int ID { get { return _id; } }
		public string Name { get { return _name; } set { _name = value; } }
		public string Description { get { return _description; } set { _description = value; } }
	}
}
