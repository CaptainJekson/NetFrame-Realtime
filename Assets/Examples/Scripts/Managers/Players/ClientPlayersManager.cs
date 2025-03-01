using System.Collections.Generic;
using Examples.Scripts.Dataframes;
using Examples.Scripts.NetworkComponents;
using UnityEngine;

namespace Examples.Scripts.Managers.Players
{
    public class ClientPlayersManager : MonoBehaviour
    {
        [Header("Client")]
        [SerializeField] private ClientRealtimeManager clientManager;
        
        [Header("Templates")]
        [SerializeField] private NetworkTransformPlayer localPlayerTemplate;
        [SerializeField] private NetworkTransformPlayer remotePlayerTemplate;

        private Dictionary<int, NetworkTransformPlayer> _spawnedPlayers;
        
        private void Awake()
        {
            _spawnedPlayers = new Dictionary<int, NetworkTransformPlayer>();
            
            clientManager.Client.ConnectionSuccessful += OnConnectionSuccessful;
            clientManager.Client.Disconnected += OnDisconnected;
            
            clientManager.Client.Subscribe<PlayerSpawnResponseDataframe>(PlayerSpawnDataframeHandler);
            clientManager.Client.Subscribe<PlayerDeSpawnResponse>(PlayerDeSpawnResponseHandler);
        }

        private void OnConnectionSuccessful(int id)
        {
            var startPosition = new Vector3(Random.Range(-10f, 10f), Random.Range(-4f, 4f),0);
            var startRotation = Quaternion.identity;

            var requestSpawnDataframe = new PlayerSpawnRequestDataframe
            {
                StartPosition = startPosition,
                StartRotation = startRotation,
            };
            clientManager.Client.Send(ref requestSpawnDataframe);
        }
        
        private void OnDisconnected()
        {
            foreach (var player in _spawnedPlayers)
            {
                Destroy(player.Value.gameObject);
            }
            
            _spawnedPlayers.Clear();
        }
        
        private void PlayerSpawnDataframeHandler(PlayerSpawnResponseDataframe responseDataframe)
        {
            var isLocal = responseDataframe.IsLocal;
            var id = responseDataframe.Id;
            var startPosition = responseDataframe.StartPosition;
            var startRotation = responseDataframe.StartRotation;
            
            var spawnedPlayer = Instantiate(isLocal ? localPlayerTemplate : remotePlayerTemplate, startPosition, startRotation);
            spawnedPlayer.SetId(id);
            
            _spawnedPlayers.Add(id, spawnedPlayer);
        }
        
        private void PlayerDeSpawnResponseHandler(PlayerDeSpawnResponse responseDataframe)
        {
            var id = responseDataframe.Id;
            
            var spawnedPlayer = _spawnedPlayers[id];
            Destroy(spawnedPlayer.gameObject);

            _spawnedPlayers.Remove(id);
        }

        private void OnDestroy()
        {
            clientManager.Client.ConnectionSuccessful -= OnConnectionSuccessful;
            clientManager.Client.Disconnected -= OnDisconnected;
            
            clientManager.Client.Unsubscribe<PlayerSpawnResponseDataframe>(PlayerSpawnDataframeHandler);
            clientManager.Client.Unsubscribe<PlayerDeSpawnResponse>(PlayerDeSpawnResponseHandler);
        }
    }
}