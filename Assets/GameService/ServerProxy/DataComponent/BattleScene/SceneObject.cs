using System.Collections;
using System.Collections.Generic;
using GameCore.Network;
using GameCore;
using GameCore.BattleScene;
using GameService.Util;
using GameService.CharacterData;

namespace GameService.ServerProxy.DataComponent.BattleScene {
    public sealed class SceneObject : IIdentifiable {
        private readonly int _id;
        private readonly Character _characterRef;

        public int ID { get { return _id; } }
        public Character CharacterRef { get { return _characterRef; } }

        public SceneObject(int id, Character character) {
            _id = id;
            _characterRef = character;
        }

    }
}