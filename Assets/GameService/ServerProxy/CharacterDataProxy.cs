using System;
using System.Collections;
using System.Collections.Generic;
using Futilef;
using GameService.CharacterData;
using GameCore;
using GameCore.Network;
using GameCore.Network.ServerMessages;
using GameCore.Network.ClientMessages;
using UnityEngine;
using GameService.Util;

namespace GameService.ServerProxy {
    public sealed class CharacterDataProxy : ServerComponentProxy {
        public CharacterDataProxy(Connection connection) : base(connection) {
            connection.AddMessageReceiver(SyncAbilityTypeListMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(CreateCharacterMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(CreateTemporaryCharacterMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(ClearTemporaryCharacterListMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(UpdateCharacterBaseDataMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(UpdateCharacterAbilityDataMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(CharacterAddAspectMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(CharacterRemoveAspectMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(CharacterUpdateAspectDataMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(CharacterClearAspectListMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(CharacterAddConsequenceMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(CharacterRemoveConsequenceMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(CharacterUpdateConsequenceMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(CharacterClearConsequenceListMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(CharacterAddStuntMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(CharacterRemoveStuntMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(CharacterUpdateStuntMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(CharacterClearStuntListMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(CharacterAddExtraMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(CharacterRemoveExtraMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(CharacterUpdateExtraMessage.MESSAGE_TYPE, this);
            connection.AddMessageReceiver(CharacterClearExtraListMessage.MESSAGE_TYPE, this);
        }

        public override void MessageReceived(Message message) {
            switch (message.MessageType) {
                case SyncAbilityTypeListMessage.MESSAGE_TYPE: {
                        var msg = (SyncAbilityTypeListMessage)message;
                        AbilityType.AbilityTypes.Clear();
                        foreach (var typeInfo in msg.abilityTypes) {
                            var abilityType = new AbilityType(typeInfo.id.value, typeInfo.name);
                            AbilityType.AbilityTypes.Add(abilityType.ID, abilityType);
                        }
                    }
                    break;
                case CreateCharacterMessage.MESSAGE_TYPE: {
                        var msg = (CreateCharacterMessage)message;
                        var pool = CharacterManager.Instance.CharacterPool;
                        var character = new Character(msg.id.value, msg.avatar);
                        character.Name = msg.description.name;
                        character.Description = msg.description.text;
                        pool.Add(character);
                    }
                    break;
                case CreateTemporaryCharacterMessage.MESSAGE_TYPE: {
                        var msg = (CreateTemporaryCharacterMessage)message;
                        var pool = CharacterManager.Instance.TemporaryCharacterPool;
                        var character = new Character(msg.id.value, msg.avatar);
                        character.Name = msg.description.name;
                        character.Description = msg.description.text;
                        pool.Add(character);
                    }
                    break;
                case ClearTemporaryCharacterListMessage.MESSAGE_TYPE: {
                        CharacterManager.Instance.TemporaryCharacterPool.Clear();
                    }
                    break;
                case UpdateCharacterBaseDataMessage.MESSAGE_TYPE: {
                        var msg = (UpdateCharacterBaseDataMessage)message;
                        int characterID = msg.characterID.value;
                        var character = CharacterManager.Instance.FindCharacter(characterID);
                        character.Destroyed = msg.destroyed;
                        character.SituationToken = msg.situationToken;
                        character.ClassToken = msg.classToken;
                        character.FatePoint = msg.fatePoint;
                        character.RefreshPoint = msg.refreshPoint;
                        character.PhysicsStress = msg.physicsStress;
                        character.PhysicsStressMax = msg.physicsStressMax;
                        character.PhysicsInvincible = msg.physicsInvincible;
                        character.MentalStress = msg.mentalStress;
                        character.MentalStressMax = msg.mentalStressMax;
                        character.MentalInvincible = msg.mentalInvincible;
                    }
                    break;
                case UpdateCharacterAbilityDataMessage.MESSAGE_TYPE: {
                        var msg = (UpdateCharacterAbilityDataMessage)message;
                        int characterID = msg.characterID.value;
                        var character = CharacterManager.Instance.FindCharacter(characterID);
                        var abilityType = AbilityType.AbilityTypes[msg.typeID.value];
                        character.Abilities[abilityType] = new AbilityData() {
                            customName = msg.customName,
                            level = msg.level,
                            damageMental = msg.damageMental
                        };
                    }
                    break;
                case CharacterAddAspectMessage.MESSAGE_TYPE: {
                        var msg = (CharacterAddAspectMessage)message;
                        int characterID = msg.characterID.value;
                        var character = CharacterManager.Instance.FindCharacter(characterID);
                        var aspect = new Aspect(msg.aspectID.value);
                        aspect.Name = msg.description.name;
                        aspect.Description = msg.description.text;
                        character.Aspects.Add(aspect);
                    }
                    break;
                case CharacterRemoveAspectMessage.MESSAGE_TYPE: {
                        var msg = (CharacterRemoveAspectMessage)message;
                        int characterID = msg.characterID.value;
                        var character = CharacterManager.Instance.FindCharacter(characterID);
                        character.Aspects.Remove(msg.aspectID.value);
                    }
                    break;
                case CharacterUpdateAspectDataMessage.MESSAGE_TYPE: {
                        var msg = (CharacterUpdateAspectDataMessage)message;
                        int characterID = msg.characterID.value;
                        var character = CharacterManager.Instance.FindCharacter(characterID);
                        var aspect = character.Aspects[msg.aspectID.value];
                        aspect.PersistenceType = msg.persistenceType;
                        int benefiterID = msg.benefiterID.value;
                        if (benefiterID == -1) aspect.Benefiter = null;
                        else aspect.Benefiter = CharacterManager.Instance.FindCharacter(benefiterID);
                        aspect.BenefitTimes = msg.benefitTimes;
                    }
                    break;
                case CharacterClearAspectListMessage.MESSAGE_TYPE: {
                        var msg = (CharacterClearAspectListMessage)message;
                        int characterID = msg.characterID.value;
                        var character = CharacterManager.Instance.FindCharacter(characterID);
                        character.Aspects.Clear();
                    }
                    break;
                case CharacterAddConsequenceMessage.MESSAGE_TYPE: {
                        var msg = (CharacterAddConsequenceMessage)message;
                        int characterID = msg.characterID.value;
                        var character = CharacterManager.Instance.FindCharacter(characterID);
                        var consequence = new Consequence(msg.consequenceID.value);
                        consequence.Name = msg.description.name;
                        consequence.Description = msg.description.text;
                        character.Consequences.Add(consequence);
                    }
                    break;
                case CharacterRemoveConsequenceMessage.MESSAGE_TYPE: {
                        var msg = (CharacterRemoveConsequenceMessage)message;
                        int characterID = msg.characterID.value;
                        var character = CharacterManager.Instance.FindCharacter(characterID);
                        character.Consequences.Remove(msg.consequenceID.value);
                    }
                    break;
                case CharacterUpdateConsequenceMessage.MESSAGE_TYPE: {
                        var msg = (CharacterUpdateConsequenceMessage)message;
                        int characterID = msg.characterID.value;
                        var character = CharacterManager.Instance.FindCharacter(characterID);
                        var consequence = character.Consequences[msg.consequenceID.value];
                        consequence.PersistenceType = msg.persistenceType;
                        int benefiterID = msg.benefiterID.value;
                        if (benefiterID == -1) consequence.Benefiter = null;
                        else consequence.Benefiter = CharacterManager.Instance.FindCharacter(benefiterID);
                        consequence.BenefitTimes = msg.benefitTimes;
                        consequence.CounteractLevel = msg.counteractLevel;
                        consequence.ActualDamage = msg.actualDamage;
                        consequence.Mental = msg.mental;
                    }
                    break;
                case CharacterClearConsequenceListMessage.MESSAGE_TYPE: {
                        var msg = (CharacterClearConsequenceListMessage)message;
                        int characterID = msg.characterID.value;
                        var character = CharacterManager.Instance.FindCharacter(characterID);
                        character.Consequences.Clear();
                    }
                    break;
                case CharacterAddStuntMessage.MESSAGE_TYPE: {
                        var msg = (CharacterAddStuntMessage)message;
                        int characterID = msg.characterID.value;
                        var character = CharacterManager.Instance.FindCharacter(characterID);
                        var stunt = new Stunt(msg.stuntID.value);
                        stunt.Name = msg.description.name;
                        stunt.Description = msg.description.text;
                        character.Stunts.Add(stunt);
                    }
                    break;
                case CharacterRemoveStuntMessage.MESSAGE_TYPE: {
                        var msg = (CharacterRemoveStuntMessage)message;
                        int characterID = msg.characterID.value;
                        var character = CharacterManager.Instance.FindCharacter(characterID);
                        character.Stunts.Remove(msg.stuntID.value);
                    }
                    break;
                case CharacterUpdateStuntMessage.MESSAGE_TYPE: {
                        var msg = (CharacterUpdateStuntMessage)message;
                        int characterID = msg.characterID.value;
                        var character = CharacterManager.Instance.FindCharacter(characterID);
                        var stunt = character.Stunts[msg.stuntID.value];
                        ///////////////////////////////////
                    }
                    break;
                case CharacterClearStuntListMessage.MESSAGE_TYPE: {
                        var msg = (CharacterClearStuntListMessage)message;
                        int characterID = msg.characterID.value;
                        var character = CharacterManager.Instance.FindCharacter(characterID);
                        character.Stunts.Clear();
                    }
                    break;
                case CharacterAddExtraMessage.MESSAGE_TYPE: {
                        var msg = (CharacterAddExtraMessage)message;
                        int characterID = msg.characterID.value;
                        int itemID = msg.boundCharacterID.value;
                        var character = CharacterManager.Instance.FindCharacter(characterID);
                        var item = CharacterManager.Instance.FindCharacter(itemID);
                        var extra = new Extra(msg.extraID.value, item);
                        extra.Name = msg.description.name;
                        extra.Description = msg.description.text;
                        character.Extras.Add(extra);
                    }
                    break;
                case CharacterRemoveExtraMessage.MESSAGE_TYPE: {
                        var msg = (CharacterRemoveExtraMessage)message;
                        int characterID = msg.characterID.value;
                        var character = CharacterManager.Instance.FindCharacter(characterID);
                        character.Extras.Remove(msg.extraID.value);
                    }
                    break;
                case CharacterUpdateExtraMessage.MESSAGE_TYPE: {
                        var msg = (CharacterUpdateExtraMessage)message;
                        int characterID = msg.characterID.value;
                        var character = CharacterManager.Instance.FindCharacter(characterID);
                        var extra = character.Extras[msg.extraID.value];
                        extra.IsLongRangeWeapon = msg.isLongRangeWeapon;
                        extra.IsVehicle = msg.isVehicle;
                    }
                    break;
                case CharacterClearExtraListMessage.MESSAGE_TYPE: {
                        var msg = (CharacterClearExtraListMessage)message;
                        int characterID = msg.characterID.value;
                        var character = CharacterManager.Instance.FindCharacter(characterID);
                        character.Extras.Clear();
                    }
                    break;
            }
        }
    }
}
