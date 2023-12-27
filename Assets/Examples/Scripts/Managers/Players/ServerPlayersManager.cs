using System.Collections.Generic;
using Examples.Scripts.Dataframes;
using Examples.Scripts.Model;
using UnityEngine;

namespace Examples.Scripts.Managers.Players
{
    public class ServerPlayersManager : MonoBehaviour
    {
        [Header("Server")]
        [SerializeField] private ServerRealTimeManager serverManager;
        
        private Dictionary<int, PlayerModel> _players;

        private void Awake()
        {
            _players = new Dictionary<int, PlayerModel>();
            
            serverManager.Server.ClientDisconnect += OnClientDisconnect;
            
            serverManager.Server.Subscribe<PlayerSpawnRequestDataframe>(PlayerRequestSpawnDataframeHandler);
            serverManager.Server.Subscribe<PlayerMoveDataframe>(PlayerMoveDataframeHandler);
        }

        private void PlayerRequestSpawnDataframeHandler(PlayerSpawnRequestDataframe dataframe, int id)
        {
            if (_players.ContainsKey(id))
            {
                Debug.LogError($"[ServerRealTimeManager.PlayerRequestSpawnDataframeHandler] already spawned playerId: {id}");
                return;
            }
            
            var playerModel = new PlayerModel
            {
                CurrentPosition = dataframe.StartPosition,
                CurrentRotation = dataframe.StartRotation,
            };
            
            _players.Add(id, playerModel);

            var responseDataframe = new PlayerSpawnResponseDataframe
            {
                IsLocal = true,
                Id = id,
                StartPosition = dataframe.StartPosition,
                StartRotation = dataframe.StartRotation,
            };
            serverManager.Server.Send(ref responseDataframe, id); //отправляем локальный спавн

            responseDataframe.IsLocal = false;
            serverManager.Server.SendAllExcept(ref responseDataframe, id); //отправляем спавн другим игрока
        }
        
        private void PlayerMoveDataframeHandler(PlayerMoveDataframe dataframe, int id)
        {
            var responseDataframe = new PlayerMoveDataframe
            {
                Id = id,
                RemoteTime = dataframe.LocalTime,
                Position = dataframe.Position,
                Rotation = dataframe.Rotation,
            };
            serverManager.Server.SendAllExcept(ref responseDataframe, id);
        }
        
        private void OnClientDisconnect(int id)
        {
            _players.Remove(id);

            var dataframe = new PlayerDeSpawnResponse
            {
                Id = id,
            };
            serverManager.Server.SendAll(ref dataframe);
        }

        private void OnDestroy()
        {
            serverManager.Server.ClientDisconnect -= OnClientDisconnect;
            
            serverManager.Server.Unsubscribe<PlayerSpawnRequestDataframe>(PlayerRequestSpawnDataframeHandler);
            serverManager.Server.Unsubscribe<PlayerMoveDataframe>(PlayerMoveDataframeHandler);
        }
    }
}