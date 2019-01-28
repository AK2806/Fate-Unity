using System.Collections;
using System.Collections.Generic;
using GameCore.Network;
using GameCore;
using GameCore.BattleScene;
using GameService.Util;
using GameService.CharacterData;

namespace GameService.ServerProxy {
	public class BattleSceneProxy : ServerComponentProxy {

		public BattleSceneProxy(Connection connection) :
			base(connection) {

		}

		public override void MessageReceived(Message message) {

		}
	}
}
