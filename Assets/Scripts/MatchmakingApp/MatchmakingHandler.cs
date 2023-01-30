#if UNITY_STANDALONE || UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using System.Threading.Tasks;

using Random = UnityEngine.Random;

namespace WordPuzzle.Matchmaking
{
    public class MatchmakingHandler : IUpdateableRegular
    {
        #region Fields

        private bool _isRunning;
        private float _interval;
        private float _matchmakingTimer;
        private float _cleanupTimer;

        public Action<string> StatusMessage;

        private FirebaseDatabase _dbInstance;
        private DatabaseReference _dbReference;
        private DatabaseReference _queue;
        private DatabaseReference _matches;

        private static double MATCH_TIMEOUT = 2;
        private static double MATCHMAKING_TIMEOUT = 1;

        #endregion


        #region IUpdateableRegular

        public void UpdateRegular()
        {
            if (!_isRunning)
                return;

            if (_matchmakingTimer > 0)
                _matchmakingTimer -= Time.deltaTime;
            else
            {
                _matchmakingTimer = _interval;
                CheckMathmakingQueue();
            }

            if (_cleanupTimer > 0)
                _cleanupTimer -= Time.deltaTime;
            else
            {
                _cleanupTimer = _interval;
                CheckActiveMatches();
            }
        }

        #endregion


        #region Methods

        public void StartMatchmaking(float interval)
        {
            _interval = Mathf.Abs(interval);
            _matchmakingTimer = _interval / 2;
            _cleanupTimer = _interval;
            _isRunning = true;
            _dbInstance = FirebaseDatabase.DefaultInstance;
            _dbInstance.SetPersistenceEnabled(false);
            _dbReference = _dbInstance.RootReference;
            _queue = _dbInstance.GetReference(References.MATHMAKING_QUEUE_BRANCH);
            _matches = _dbInstance.GetReference(References.ACTIVE_MATCHES_BRANCH);
            StatusMessage?.Invoke("Matchmaker started");
        }

        public void StopMatchmaking()
        {
            _isRunning = false;
        }

        public async void CheckMathmakingQueue()
        {
            var DBTask = _queue.GetValueAsync();

            await DBTask;

            if (DBTask.Exception != null)
            {
                var message = $"Failed to register matchmaking task with {DBTask.Exception}  |  {DateTime.Now}";
                Debug.LogWarning(message);
                StatusMessage?.Invoke(message);
                return;
            }
            else if (DBTask.Result.Value == null)
            {
                var message = $"No players in queue  |  {DateTime.Now}";
                Debug.LogWarning(message);
                return;
            }

            HandleMathmaking(DBTask.Result);
        }

        public async void CheckActiveMatches()
        {
            var DBTask = _matches.GetValueAsync();

            await DBTask;

            if (DBTask.Exception != null)
            {
                var message = $"Failed to register matchmaking task with {DBTask.Exception}  |  {DateTime.Now}";
                Debug.LogWarning(message);
                StatusMessage?.Invoke(message);
                return;
            }
            else if (DBTask.Result.Value == null)
            {
                var message = $"No active matches  |  {DateTime.Now}";
                Debug.LogWarning(message);
                return;
            }

            HandleCleanup(DBTask.Result);
        }

        private void HandleMathmaking(DataSnapshot queue)
        {
            Debug.Log($"{queue.Key}  |  Children count: {queue.ChildrenCount}");

            foreach (var theme in queue.Children)
            {
                Debug.Log($"{theme.Key}  |  Children count: {theme.ChildrenCount}");

                var playersInQueue = new List<DataSnapshot>();

                foreach (var player in theme.Children)
                    playersInQueue.Add(player);

                List<DataSnapshot> playersAwaitingMatch = new List<DataSnapshot>();
                List<DataSnapshot> timedOutPlayers = new List<DataSnapshot>();

                SortPLayers(playersInQueue, playersAwaitingMatch, timedOutPlayers);

                MatchPlayers(playersAwaitingMatch, theme.Key);
                RemovePlayersFromMatchmaking(timedOutPlayers, theme.Key);
            }
        }

        private async void MatchPlayers(List<DataSnapshot> playersInQueue, string theme)
        {
            Debug.Log("Mathcing players"); 

            var matchedPlayers = new List<(string player1, string player2)>();

            while (playersInQueue.Count >= 2)
            {
                int firstIndex;
                int secondIndex;

                do
                {
                    firstIndex = Random.Range(0, playersInQueue.Count);
                    secondIndex = Random.Range(0, playersInQueue.Count);
                } while (firstIndex == secondIndex);

                var firstPlayer = playersInQueue[firstIndex];
                var secondPlayer = playersInQueue[secondIndex];
                matchedPlayers.Add(
                    (firstPlayer.Key, secondPlayer.Key)
                    );

                playersInQueue.Remove(firstPlayer);
                playersInQueue.Remove(secondPlayer);
            }

            foreach (var playerPair in matchedPlayers)
                await CreateMatch(theme, playerPair);
        }

        private async void RemovePlayersFromMatchmaking(List<DataSnapshot> playersToRemove, string theme)
        {
            Debug.Log("Removing players");
            foreach (var player in playersToRemove)
            {
                await _queue.Child(theme).Child(player.Key).RemoveValueAsync();
            } 
        }

        private void SortPLayers(List<DataSnapshot> playersInQueue, List<DataSnapshot> playersAwaitingMatch, List<DataSnapshot> timedOutPlayers)
        {
            Debug.Log("Sorting players");
            foreach (var player in playersInQueue)
            {
                var timeSinceLastActivity = GetTimeSinceLastActivity(player);
                Debug.Log("Matchmaking tiime: "+timeSinceLastActivity.TotalMinutes);
                if (timeSinceLastActivity.TotalMinutes > MATCHMAKING_TIMEOUT)
                {
                    timedOutPlayers.Add(player);
                    continue;
                }

                if (player.Child(References.MATCH_ID).Value == null)
                {
                    timedOutPlayers.Add(player);
                    continue;
                }

                if (player.Child(References.MATCH_ID).Value.ToString().Equals(References.PENDING))
                    playersAwaitingMatch.Add(player);
            }
        }

        private async Task CreateMatch(string theme, (string player1, string player2) players)
        {
            var matchID = GenerateMatchID();

            var DBTask1 = _matches.Child(matchID)
                .Child(References.PLAYERS).Child(players.player1).Child(References.IS_READY).SetValueAsync("false");

            var DBTask2 = _matches.Child(matchID)
                .Child(References.PLAYERS).Child(players.player2).Child(References.IS_READY).SetValueAsync("false");

            var DBTask3 = _matches.Child(matchID)
                .Child(References.PLAYERS).Child(players.player1).Child(References.HAS_LEFT_GAME).SetValueAsync("false");

            var DBTask4 = _matches.Child(matchID)
                .Child(References.PLAYERS).Child(players.player2).Child(References.HAS_LEFT_GAME).SetValueAsync("false");

            await Task.WhenAll(DBTask1, DBTask2, DBTask3, DBTask4);

            var DBTask5 = _queue.Child(theme)
                .Child(players.player1).Child(References.MATCH_ID).SetValueAsync(matchID);

            var DBTask6 = _queue.Child(theme
                ).Child(players.player2).Child(References.MATCH_ID).SetValueAsync(matchID);
        }

        private async void HandleCleanup(DataSnapshot matches)
        {
            var playersInMatchesTask = GetPlayersInMatches(matches);
            await playersInMatchesTask;

            var playersInMatches = playersInMatchesTask.Result;

            var matchesToRemove = new List<string>();

            foreach (var kvp in playersInMatches)
            {
                if (kvp.Value.ChildrenCount != 2)
                {
                    matchesToRemove.Add(kvp.Key);
                    continue;
                }

                var players = new List<DataSnapshot>(2);

                foreach (var player in kvp.Value.Children)
                    players.Add(player);

                if (bool.Parse(players[0].Child(References.HAS_LEFT_GAME).Value.ToString()) &&
                    bool.Parse(players[1].Child(References.HAS_LEFT_GAME).Value.ToString()))
                    matchesToRemove.Add(kvp.Key);
            }

            foreach (var match in matchesToRemove)
                playersInMatches.Remove(match);

            RemoveMatches(matchesToRemove);

            if (playersInMatches.Count != 0)
                CheckForInactivity(playersInMatches);
        }

        private async Task<Dictionary<string, DataSnapshot>> GetPlayersInMatches(DataSnapshot matches)
        {
            List<string> activeMatches = new List<string>((int)matches.ChildrenCount);
            List<Task<DataSnapshot>> playerRetrievalTasks = new List<Task<DataSnapshot>>((int)matches.ChildrenCount);
            Dictionary<string, DataSnapshot> playersInMatches = new Dictionary<string, DataSnapshot>();


            foreach (var match in matches.Children)
            {
                activeMatches.Add(match.Key);
                playerRetrievalTasks.Add(_matches.Child(match.Key).Child(References.PLAYERS).GetValueAsync());
            }

            await Task.WhenAll(playerRetrievalTasks.ToArray());

            for (int i = 0; i < activeMatches.Count; i++)
            {
                playersInMatches.Add(activeMatches[i], playerRetrievalTasks[i].Result);
            }

            return playersInMatches;
        }

        private void CheckForInactivity(Dictionary<string, DataSnapshot> playersInMatches)
        {
            foreach (var kvp in playersInMatches)
            {
                foreach (var player in kvp.Value.Children)
                    CheckInactivityForPlayerInMatch(player, kvp.Key);
            }
        }

        private async void CheckInactivityForPlayerInMatch(DataSnapshot playerToCheck, string matchID)
        {
            var timeSinceLastActivity = GetTimeSinceLastActivity(playerToCheck);
            Debug.Log(timeSinceLastActivity.TotalMinutes);
            if (timeSinceLastActivity.TotalMinutes > MATCH_TIMEOUT)
                await _matches.Child(matchID).Child(References.PLAYERS).Child(playerToCheck.Key).Child(References.HAS_LEFT_GAME).SetValueAsync("true");
        }

        private TimeSpan GetTimeSinceLastActivity(DataSnapshot playerToCheck)
        {
            long timestamp;
            try
            {
                timestamp = long.Parse(playerToCheck.Child(References.TIMESTAMP).Value.ToString());
            }
            catch
            {
                timestamp = 0;
            }
            var lastActivity = DateTime.FromBinary(timestamp);
            var currentTime = DateTime.UtcNow;

            return new TimeSpan(currentTime.Subtract(lastActivity).Ticks);
        }

        private void RemoveMatches(List<string> matches)
        {
            foreach (var match in matches)
                RemoveMatch(match);
        }

        private async void RemoveMatch(string matchId)
        {
            await _matches.Child(matchId).RemoveValueAsync();
        }

        private string GenerateMatchID()
        {
            return Guid.NewGuid().ToString();
        }

        #endregion
    }
}

#endif