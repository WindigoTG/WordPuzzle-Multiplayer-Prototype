#if UNITY_STANDALONE || UNITY_EDITOR

using System.Collections.Generic;
using Firebase.Database;
using System.Threading.Tasks;

namespace WordPuzzle.Matchmaking
{
    public class LikesHelper
    {
        #region Fields

        private FirebaseDatabase _dbInstance;
        private DatabaseReference _dbReference;
        private DatabaseReference _likes;

        private bool _isStarted;

        private Queue<(string, long, string[])> _userLikesToUpdate = new Queue<(string, long, string[])>();

        private bool _isUpdateRunning;

        #endregion


        #region Methods

        public void StartListening()
        {
            _dbInstance = FirebaseDatabase.DefaultInstance;
            _dbInstance.SetPersistenceEnabled(false);
            _dbReference = _dbInstance.RootReference;
            _likes = _dbInstance.GetReference(References.LIKES);

            _likes.ChildAdded += OnNewLike;

            _isStarted = true;
        }

        public void StopListening()
        {
            _likes.ChildAdded -= OnNewLike;

            _isStarted = false;
        }

        private void OnNewLike(object sender, ChildChangedEventArgs e)
        {
            if (!_isStarted)
                return;

            var userID = e.Snapshot.Key;
            var likesCount = e.Snapshot.ChildrenCount;

            List<string> sendersIDs = new List<string>();
            foreach (var senderID in e.Snapshot.Children)
                sendersIDs.Add(senderID.Key);

            _userLikesToUpdate.Enqueue((userID, likesCount, sendersIDs.ToArray()));

            if (!_isUpdateRunning)
                UpdateUserLikesInfo();
        }

        private async void UpdateUserLikesInfo()
        {
            _isUpdateRunning = true;

            while (_userLikesToUpdate.Count > 0)
                await UpdateUserLikesInfo(_userLikesToUpdate.Dequeue());

            _isUpdateRunning = false;
        }

        private async Task UpdateUserLikesInfo((string userID, long likesCount, string[] sendersIDs) infoToUpdate)
        {
            Task<DataSnapshot> currentLikesTask;

            do
            {
                currentLikesTask = _dbReference.Child(References.USERS_BRANCH).Child(infoToUpdate.userID).Child(References.LIKES).GetValueAsync();
                await currentLikesTask;
            } while (currentLikesTask.IsFaulted);

            long likesCount;

            try
            {
                likesCount = long.Parse(currentLikesTask.Result.Value.ToString());
            }
            catch
            {
                likesCount = 0;
            }

            likesCount += infoToUpdate.likesCount;

            Task newLikesTask;

            do
            {
                newLikesTask = _dbReference.Child(References.USERS_BRANCH).Child(infoToUpdate.userID).Child(References.LIKES).SetValueAsync(likesCount);
                await newLikesTask;
            } while (newLikesTask.IsFaulted);

            List<Task> removalTasks = new List<Task>();
            Task faultedTask;
            do
            {
                removalTasks.Clear();

                foreach (var id in infoToUpdate.sendersIDs)
                    removalTasks.Add(_dbReference.Child(References.LIKES).Child(infoToUpdate.userID).Child(id).RemoveValueAsync());

                await Task.WhenAll(removalTasks.ToArray());
                faultedTask = removalTasks.Find(x => x.IsFaulted);
            } while (faultedTask != null);
        }

        #endregion
    }
}

#endif