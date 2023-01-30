#if UNITY_STANDALONE || UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using System.Threading.Tasks;
using System.Text;

namespace WordPuzzle.Matchmaking
{
    public class UserSearchHandler : IUpdateableRegular
    {
        #region Fields

        private bool _isRunning;
        private float _interval;
        private float _matchmakingTimer;
        private float _cleanupTimer;

        public Action<string> StatusMessage;

        private FirebaseDatabase _dbInstance;
        private DatabaseReference _dbReference;
        private DatabaseReference _searchRequests;
        private DatabaseReference _searchResults;

        private static double CLEANUP_DELAY = 1;

        #endregion

        #region IUpdateableRegular

        public void UpdateRegular()
        {
            if (!_isRunning)
                return;

            if (_cleanupTimer > 0)
                _cleanupTimer -= Time.deltaTime;
            else
            {
                _cleanupTimer = _interval;
                CheckSearchResults();
            }
        }

        #endregion


        #region Methods

        public void StartSearchHandler(float interval)
        {
            _interval = Mathf.Abs(interval);
            _cleanupTimer = _interval;
            _isRunning = true;
            _dbInstance = FirebaseDatabase.DefaultInstance;
            _dbInstance.SetPersistenceEnabled(false);
            _dbReference = _dbInstance.RootReference;
            _searchRequests = _dbInstance.GetReference(References.SEARCH_REQUESTS_BRANCH);
            _searchResults = _dbInstance.GetReference(References.SEARCH_RESULT_BRANCH);

            _searchRequests.ChildAdded += OnNewSearchRequest;
        }

        private void OnNewSearchRequest(object sender, ChildChangedEventArgs e)
        {
            RunSearch(e.Snapshot);
        }

        private async void RunSearch(DataSnapshot request)
        {
            _searchRequests.Child(request.Key).RemoveValueAsync();

            Task<DataSnapshot> usersTask;
            do
            {
                usersTask = _dbReference.Child(References.USERS_BRANCH).GetValueAsync();
                await usersTask;
            } while (usersTask.IsFaulted);

            List<DataSnapshot> users = new List<DataSnapshot>();

            foreach (var user in usersTask.Result.Children)
                users.Add(user);

            var results = users.FindAll(x => x.HasChild(References.NICKNAME) &&
                x.Child(References.NICKNAME).Value.ToString().ToLower().Equals(request.Value.ToString().ToLower()));

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < results.Count; i++)
            {
                sb.Append(results[i].Key);

                if (i < results.Count - 1)
                    sb.Append(References.SEPARATOR);
            }

            Task resultTask1, resultTask2;

            do
            {
                resultTask1 = _searchResults.Child(request.Key.ToString()).Child(request.Value.ToString()).Child(References.RESULT).SetValueAsync(sb.ToString());
                resultTask2 = _searchResults.Child(request.Key.ToString()).Child(request.Value.ToString()).Child(References.TIMESTAMP).SetValueAsync(DateTime.UtcNow.ToBinary());

                await Task.WhenAll(resultTask1, resultTask2);
            } while (resultTask1.IsFaulted || resultTask2.IsFaulted);
        }

        public void StopSearchHandler()
        {
            _isRunning = false;
            _searchRequests.ChildAdded -= OnNewSearchRequest;
        }

        private async void CheckSearchResults()
        {
            var DBTask = _searchResults.GetValueAsync();

            await DBTask;

            if (DBTask.Exception != null)
            {
                var message = $"Failed to register matchmaking task with {DBTask.Exception}  |  {DateTime.Now}";
                Debug.LogWarning(message);
                return;
            }
            else if (DBTask.Result.Value == null)
            {
                return;
            }

            HandleCleanup(DBTask.Result);
        }

        private void HandleCleanup(DataSnapshot searchResults)
        {
            List<(string branchID, string resultID)> resultsToRemove = new List<(string branchID, string resultID)>();

            foreach (var branch in searchResults.Children)
            {
                foreach(var result in branch.Children)
                {
                    long timestamp;
                    try
                    {
                        timestamp = long.Parse(result.Child(References.TIMESTAMP).Value.ToString());
                    }
                    catch
                    {
                        timestamp = 0;
                    }
                    var postedTime = DateTime.FromBinary(timestamp);
                    var currentTime = DateTime.UtcNow;

                    var time = new TimeSpan(currentTime.Subtract(postedTime).Ticks);

                    if (time.TotalMinutes >= CLEANUP_DELAY)
                        resultsToRemove.Add((branch.Key.ToString(), result.Key.ToString()));
                }
            }

            foreach (var result in resultsToRemove)
                RemoveResult(result);
        }

        private async void RemoveResult((string branchID, string resultID) result)
        {
            Task removalTask;
            do
            {
                removalTask = _searchResults.Child(result.branchID).Child(result.resultID).RemoveValueAsync();
                await removalTask;
            } while (removalTask.IsFaulted);
            
        }

        #endregion
    }
}

#endif