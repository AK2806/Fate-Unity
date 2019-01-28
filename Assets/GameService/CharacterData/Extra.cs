using GameService.Util;
using System;

namespace GameService.CharacterData {
	public sealed class Extra : IIdentifiable {
		private readonly int _id;
		private readonly Character _item;
		private bool _isLongRangeWeapon;
		private bool _isVehicle;

		public Extra(int id, Character item) {
			_id = id;
			_item = item;
		}

		public int ID { get { return _id; } }
		public string Name { get { return _item.Name; } set { _item.Name = value; } }
		public string Description { get { return _item.Description; } set { _item.Description = value; } }
		public Character Item { get { return _item;} }
		public bool IsLongRangeWeapon { get { return _isLongRangeWeapon; } set { _isLongRangeWeapon = value; } }
		public bool IsVehicle { get { return _isVehicle; } set { _isVehicle = value; } }
	}
}
