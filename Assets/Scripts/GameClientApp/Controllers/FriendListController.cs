using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using System.Threading.Tasks;
using System.Linq;

namespace WordPuzzle
{
    public class FriendListController : BaseController
    {
        #region Fields

        private Context _context;
        private FriendsWindowView _view;
        private FriendsDataHolder _friendsDataHolder;
        private string _playerID;

        private List<FriendItemView> _friends = new List<FriendItemView>();

        private FriendList _currentlyShown = FriendList.None;

        private bool _isReady;

        private FirebaseDatabase _dbInstance;
        private DatabaseReference _dbReference;
        private DatabaseReference _friendRequestsReference;
        private Dictionary<string, DatabaseReference> _pendingFriends = new Dictionary<string, DatabaseReference>();
        private DatabaseReference _friendSearchResultReference;

        #endregion


        #region ClassLifeCycles

        public FriendListController(Context context)
        {
            _context = context;
            _context.SetFriendlistController(this);

            _dbInstance = FirebaseDatabase.DefaultInstance;
            _dbInstance.SetPersistenceEnabled(false);
            _dbReference = _dbInstance.RootReference;

            InstantiatePrefab<FriendsWindowView>(_context.UIPrefabsData.FriendsWindowPrefab, _context.CommonUiHolder, InitView);
        }

        #endregion


        #region Methods

        private void InitView(FriendsWindowView view)
        {
            _view = view;

            HideFriendsWindow();

            _view.BackButton.onClick.AddListener(HideFriendsWindow);
            _view.FriendsButton.onClick.AddListener(BuildFriendsList);
            _view.PendingButton.onClick.AddListener(BuildPendingFriendsList);
            _view.RequestButton.onClick.AddListener(BuildFriendRequestsList);
            _view.UncategorizedButton.onClick.AddListener(BuildUncategorizedList);
            _view.SearchOpenButton.onClick.AddListener(OnSearchOpenButtonClick);
            _view.SearchStartButton.onClick.AddListener(OnSearchStartButtonClick);
            _view.ButtonSearchCancel.onClick.AddListener(OnSearchCancelButtonClick);

            _view.SearchPanel.gameObject.SetActive(false);
        }

        public void ShowFriendsWindow()
        {
            if (_view.gameObject.activeSelf || !_isReady)
                return;

            BuildFriendsList();

            _view.gameObject.SetActive(true);
        }

        private void SetButtonsInteractable()
        {
            _view.FriendsButton.interactable = true;
            _view.PendingButton.interactable = true;
            _view.RequestButton.interactable = true;
            _view.UncategorizedButton.interactable = true;
        }

        public void HideFriendsWindow()
        {
            _view.gameObject.SetActive(false);
            _currentlyShown = FriendList.None;
        }

        public async Task RetrieveFriendList()
        {
            if (_isReady)
                return;

            _playerID = _context.PlayerProfile.PlayerID;

            _friendsDataHolder = _context.ProfileService.FriendsDataHolder;

            _friendRequestsReference = _dbInstance.GetReference($"{References.FRIEND_REQUESTS_BRANCH}/{_playerID}");

            var friendsTask = RetrieveFriendsData();
            var pendingTask = RetrievePendingFriendsData();
            var requestsTask = RetrieveFriendRequestsData();
            await Task.WhenAll(friendsTask, pendingTask, requestsTask);

            _isReady = true;

            _friendRequestsReference.ChildAdded += OnFriendRequestReceived;
        }

        private void OnFriendRequestReceived(object sender, ChildChangedEventArgs e)
        {
            AddNewFriendRequest(e.Snapshot.Key.ToString());
        }

        private async void AddNewFriendRequest(string userID)
        {
            UserProfileData userData;

            if (_friendsDataHolder.UncategorizedDataByID.ContainsKey(userID))
            {
                userData = _friendsDataHolder.UncategorizedDataByID[userID];
                _friendsDataHolder.UncategorizedDataByID.Remove(userID);
            }
            else
            {
                var userDataTask = _context.ProfileService.GetUserDataFromDB(userID);
                await userDataTask;
                userData = userDataTask.Result;
            }

            _friendsDataHolder.AddFriendRequestData(userID, userData);

            if (_currentlyShown == FriendList.Requests)
                BuildFriendRequestsList();

            if (_currentlyShown == FriendList.Uncategorized)
                BuildUncategorizedList();
        }

        private async Task RetrieveFriendsData()
        {
            var friendListTask = _dbReference.Child(References.USERS_BRANCH).Child(_playerID).Child(References.FRIENDS).GetValueAsync();

            while (!friendListTask.IsCompleted)
                await friendListTask;

            if (friendListTask.Result == null)
                return;

            foreach (var friend in friendListTask.Result.Children)
            {
                    var friendDataTask = _context.ProfileService.GetUserDataFromDB(friend.Value.ToString());

                    await friendDataTask;

                    _friendsDataHolder.AddFriendData(friend.Value.ToString(), friendDataTask.Result);
            }
        }

        private async Task RetrievePendingFriendsData()
        {
            var friendListTask = _dbReference.Child(References.USERS_BRANCH).Child(_playerID).Child(References.FRIENDS_PENDING).GetValueAsync();

            while (!friendListTask.IsCompleted)
                await friendListTask;

            if (friendListTask.Result == null)
                return;

            foreach (var friend in friendListTask.Result.Children)
            {
                var pendingTask = _dbReference.Child(References.FRIEND_REQUESTS_BRANCH).Child(friend.Value.ToString()).Child(_playerID).GetValueAsync();

                while (!pendingTask.IsCompleted)
                    await pendingTask;

                var value = int.Parse(pendingTask.Result.Value.ToString());

                if (value < 0)
                {
                    RemovePendingRequest(friend.Value.ToString());
                    continue;
                }

                var friendDataTask = _context.ProfileService.GetUserDataFromDB(friend.Value.ToString());

                await friendDataTask;

                if (value == 0)
                {
                    _friendsDataHolder.AddPendingFriendData(friend.Value.ToString(), friendDataTask.Result);

                    SubscripeToListenPendingFriend(friend.Value.ToString());

                    continue;
                }

                if (value > 0)
                {
                    PostFriend(friend.Value.ToString());

                    _friendsDataHolder.AddFriendData(friend.Value.ToString(), friendDataTask.Result);

                    RemovePendingRequest(friend.Value.ToString());

                    continue;
                }
            }

        }

        private void SubscripeToListenPendingFriend(string otherUserID)
        {
            var pendingReference = _dbInstance.GetReference($"{References.FRIEND_REQUESTS_BRANCH}/{otherUserID}/{_playerID}");
            pendingReference.ValueChanged += OnPendingFriendValueChanged;
            _pendingFriends.Add(otherUserID, pendingReference);
        }


        private void OnPendingFriendValueChanged(object sender, ValueChangedEventArgs e)
        {
            int value = int.Parse(e.Snapshot.Value.ToString());

            if (value < 0)
                RemovePendingFriendFromList(e.Snapshot.Reference.Parent.Key.ToString());

            if (value > 0)
                AddNewFriendToListFromPending(e.Snapshot.Reference.Parent.Key.ToString());
        }

        private void RemovePendingFriendFromList(string userID)
        {
            _pendingFriends[userID].ValueChanged -= OnPendingFriendValueChanged;
            _pendingFriends.Remove(userID);

            RemovePendingRequest(userID);

            _friendsDataHolder.UncategorizedDataByID.Add(userID, _friendsDataHolder.PendingFriendsDataByID[userID]);
            _friendsDataHolder.PendingFriendsDataByID.Remove(userID);

            if (_currentlyShown == FriendList.Pending)
                BuildPendingFriendsList();

            if (_currentlyShown == FriendList.Uncategorized)
                BuildUncategorizedList();
        }

        private void AddNewFriendToListFromPending(string userID)
        {
            _pendingFriends[userID].ValueChanged -= OnPendingFriendValueChanged;
            _pendingFriends.Remove(userID);

            RemovePendingRequest(userID);

            _friendsDataHolder.FriendsDataByID.Add(userID, _friendsDataHolder.PendingFriendsDataByID[userID]);

            _friendsDataHolder.PendingFriendsDataByID.Remove(userID);

            PostFriend(userID);

            if (_currentlyShown == FriendList.Pending)
                BuildPendingFriendsList();

            if (_currentlyShown == FriendList.Friends)
                BuildFriendsList();
        }

        private async Task RetrieveFriendRequestsData()
        {
            var friendRequestsTask = _dbReference.Child(References.FRIEND_REQUESTS_BRANCH).Child(_playerID).GetValueAsync();

            while (!friendRequestsTask.IsCompleted)
                await friendRequestsTask;

            if (friendRequestsTask.Result == null)
                return;

            foreach (var request in friendRequestsTask.Result.Children)
            {
                var value = int.Parse(request.Value.ToString());

                if (value != 0)
                    continue;

                var requestDataTask = _context.ProfileService.GetUserDataFromDB(request.Key.ToString());

                await requestDataTask;

                _friendsDataHolder.FriendRequestsDataByID.Add(request.Key.ToString(), requestDataTask.Result);
            }
        }

        private async void RemovePendingRequest(string otherUserID)
        {
            Task task1, task2;
            do
            {
                task1 = _dbReference.Child(References.FRIEND_REQUESTS_BRANCH).Child(otherUserID).Child(_playerID).RemoveValueAsync();
                task2 = _dbReference.Child(References.USERS_BRANCH).Child(_playerID).Child(References.FRIENDS_PENDING).Child(otherUserID).RemoveValueAsync();
                await Task.WhenAll(task1, task2);
            } while (task1.IsFaulted || task2.IsFaulted);
        }

        private async void PostFriend(string friendID)
        {
            Task friendTask;
            do
            {
                friendTask = _dbReference.Child(References.USERS_BRANCH).Child(_playerID).Child(References.FRIENDS).Child(friendID).SetValueAsync(friendID);
                await friendTask;
            } while (friendTask.IsFaulted);
        }

        private async void AddNewPendingFriend(string otherUserID)
        {
            UserProfileData userData;

            if (_friendsDataHolder.UncategorizedDataByID.ContainsKey(otherUserID))
            {
                userData = _friendsDataHolder.UncategorizedDataByID[otherUserID];
                _friendsDataHolder.UncategorizedDataByID.Remove(otherUserID);
            }
            else
            {
                var userDataTask = _context.ProfileService.GetUserDataFromDB(otherUserID);
                await userDataTask;
                userData = userDataTask.Result;
            }

            if (_friendsDataHolder.PendingFriendsDataByID.ContainsKey(otherUserID))
                _friendsDataHolder.PendingFriendsDataByID[otherUserID] = userData;
            else
                _friendsDataHolder.AddPendingFriendData(otherUserID, userData);

            PostFriendRequest(otherUserID);
            SubscripeToListenPendingFriend(otherUserID);

            if (_currentlyShown == FriendList.Pending)
                BuildPendingFriendsList();
        }

        private async void PostFriendRequest(string otherUserID)
        {
            Task requestTask1;
            Task requestTask2;
            do
            {
                requestTask1 = _dbReference.Child(References.FRIEND_REQUESTS_BRANCH).Child(otherUserID).Child(_playerID).SetValueAsync(0);
                requestTask2 = _dbReference.Child(References.USERS_BRANCH).Child(_playerID).Child(References.FRIENDS_PENDING).Child(otherUserID).SetValueAsync(otherUserID);
                await Task.WhenAll(requestTask1, requestTask2);
            } while (requestTask1.IsFaulted || requestTask2.IsFaulted);
        }

        private async void ConfirmFriendRequest(string friendID)
        {
            PostFriend(friendID);

            _friendsDataHolder.AddFriendData(friendID, _friendsDataHolder.FriendRequestsDataByID[friendID]);
            _friendsDataHolder.FriendRequestsDataByID.Remove(friendID);

            if (_currentlyShown == FriendList.Requests)
                BuildFriendRequestsList();

            if (_currentlyShown == FriendList.Friends)
                BuildFriendsList();

            Task confirmTask;
            do
            {
                confirmTask = _dbReference.Child(References.FRIEND_REQUESTS_BRANCH).Child(_playerID).Child(friendID).SetValueAsync(1);
                await confirmTask;
            } while (confirmTask.IsFaulted);
        }

        private async void DeclineFriendRequest(string userID)
        {
            _friendsDataHolder.UncategorizedDataByID.Add(userID, _friendsDataHolder.FriendRequestsDataByID[userID]);
            _friendsDataHolder.FriendRequestsDataByID.Remove(userID);

            if (_currentlyShown == FriendList.Requests)
                BuildFriendRequestsList();

            Task declineTask;
            do
            {
                declineTask = _dbReference.Child(References.FRIEND_REQUESTS_BRANCH).Child(_playerID).Child(userID).SetValueAsync(-1);
                await declineTask;
            } while (declineTask.IsFaulted);
        }

        private void BuildFriendsList()
        {
            _currentlyShown = FriendList.Friends;
            SetButtonsInteractable();
            _view.FriendsButton.interactable = false;
            _view.ListEmptyMessage.text = "Нет друзей в списке";
            _view.ListEmptyMessage.gameObject.SetActive(_friendsDataHolder.FriendsDataByID.Count == 0);
            BuildUserdList(_friendsDataHolder.FriendsDataByID, false);
        }

        private void BuildPendingFriendsList()
        {
            _currentlyShown = FriendList.Pending;
            SetButtonsInteractable();
            _view.PendingButton.interactable = false;
            _view.ListEmptyMessage.text = "Список пуст";
            _view.ListEmptyMessage.gameObject.SetActive(_friendsDataHolder.PendingFriendsDataByID.Count == 0);
            BuildUserdList(_friendsDataHolder.PendingFriendsDataByID, false);
        }

        private void BuildFriendRequestsList()
        {
            _currentlyShown = FriendList.Requests;
            SetButtonsInteractable();
            _view.RequestButton.interactable = false;
            _view.ListEmptyMessage.text = "Нет запросов в друзья";
            _view.ListEmptyMessage.gameObject.SetActive(_friendsDataHolder.FriendRequestsDataByID.Count == 0);
            BuildUserdList(_friendsDataHolder.FriendRequestsDataByID, true);
        }

        private void BuildUncategorizedList()
        {
            _currentlyShown = FriendList.Uncategorized;
            SetButtonsInteractable();
            _view.UncategorizedButton.interactable = false;
            _view.ListEmptyMessage.text = "Список пуст";
            _view.ListEmptyMessage.gameObject.SetActive(_friendsDataHolder.UncategorizedDataByID.Count == 0);
            BuildUserdList(_friendsDataHolder.UncategorizedDataByID, false);
        }

        private void BuildUserdList(Dictionary<string, UserProfileData> userList, bool isRequest)
        {
            ClearCurrentList();

            float itemHeight = 0;

            foreach (var user in userList)
            {
                var friendView = Object.Instantiate(_context.UIPrefabsData.FriendItemPrefab, _view.ScrolRect.content);
                friendView.FillUserData(user.Value);
                friendView.ProfileButton.onClick.AddListener(() => _context.ProfileService.ShowOtherUserProfile(user.Key));
                friendView.MessageButton.onClick.AddListener(() => _context.ChatController.ShowChatWithUser(user.Key));

                itemHeight = (friendView.transform as RectTransform).sizeDelta.y;

                friendView.DeclineButton.gameObject.SetActive(isRequest);
                if (isRequest)
                    friendView.DeclineButton.onClick.AddListener(() => DeclineFriendRequest(user.Key));

                _friends.Add(friendView);
            }

            var size = _view.ScrolRect.content.sizeDelta;
            size.y = itemHeight * _view.ScrolRect.content.childCount;
            _view.ScrolRect.content.sizeDelta = size;
        }

        private void ClearCurrentList()
        {
            foreach (var friend in _friends)
                Object.Destroy(friend.gameObject);
            _friends.Clear();
        }

        public void OnAddFriendButtonClick(string userID)
        {
            if (_friendsDataHolder.FriendRequestsDataByID.ContainsKey(userID))
                ConfirmFriendRequest(userID);
            else
                AddNewPendingFriend(userID);
        }

        public void OnSearchOpenButtonClick()
        {
            ClearCurrentList();
            _currentlyShown = FriendList.None;
            _view.ListEmptyMessage.gameObject.SetActive(false);
            _view.SearchPanel.gameObject.SetActive(true);
        }

        public void OnSearchStartButtonClick()
        {
            _friendsDataHolder.FriendSearchDataByID.Clear();
            ClearCurrentList();
            
            if (string.IsNullOrWhiteSpace(_view.SearchNicknameInput.text))
                return;

            if (_friendSearchResultReference != null)
                _friendSearchResultReference.ChildAdded -= OnSearchResultReceived;

            string nickname = _view.SearchNicknameInput.text;
            _view.SearchNicknameInput.text = "";

            PostSearchRequest(nickname);

            _view.ListEmptyMessage.text = $"Ищем {nickname}...";
            _view.ListEmptyMessage.gameObject.SetActive(true);

            _friendSearchResultReference = _dbInstance.GetReference($"{References.SEARCH_RESULT_BRANCH}/{_context.PlayerProfile.PlayerID}/{nickname}");
            _friendSearchResultReference.ChildAdded += OnSearchResultReceived;
        }

        private async void PostSearchRequest(string nickname)
        {
            Task task;
            do
            {
                task = _dbReference.Child(References.SEARCH_REQUESTS_BRANCH).Child(_context.PlayerProfile.PlayerID).SetValueAsync(nickname);
                await task;
            } while (task.IsFaulted);
            
        }

        private void OnSearchResultReceived(object sender, ChildChangedEventArgs e)
        {
            if (!_view.SearchPanel.gameObject.activeSelf)
                return;

            if (!e.Snapshot.Key.Equals(References.RESULT))
                return;

            _friendSearchResultReference.ChildAdded -= OnSearchResultReceived;
            _friendSearchResultReference = null;

            Debug.Log(e.Snapshot.Value);

            if (string.IsNullOrEmpty(e.Snapshot.Value.ToString()))
            {
                _view.ListEmptyMessage.text = "Увы, никого не найдено...";
                _view.ListEmptyMessage.gameObject.SetActive(true);
                return;
            }

            var IDs = e.Snapshot.Value.ToString().Split(References.SEPARATOR);

            GetAndDisplayFoundUsersData(IDs);
        }

        private async void GetAndDisplayFoundUsersData(string[] userIDs)
        {
            Dictionary<string, Task<UserProfileData>> userTasksByID = new Dictionary<string, Task<UserProfileData>>();

            foreach (var id in userIDs)
                if (!id.Equals(_context.PlayerProfile.PlayerID))
                    userTasksByID.Add(id, _context.ProfileService.GetUserDataFromDB(id));

            await Task.WhenAll(userTasksByID.Values.ToArray());

            foreach (var task in userTasksByID)
                _friendsDataHolder.AddFriendSearchData(task.Key, task.Value.Result);

            _view.ListEmptyMessage.text = "Увы, никого не найдено...";
            _view.ListEmptyMessage.gameObject.SetActive(_friendsDataHolder.FriendSearchDataByID.Count == 0);
            BuildUserdList(_friendsDataHolder.FriendSearchDataByID, false);
        }

        public void OnSearchCancelButtonClick()
        {
            _friendsDataHolder.FriendSearchDataByID.Clear();
            if (_friendSearchResultReference != null)
                _friendSearchResultReference.ChildAdded -= OnSearchResultReceived;
            _friendSearchResultReference = null;
            _view.SearchNicknameInput.text = "";
            _view.SearchPanel.gameObject.SetActive(false);

            BuildFriendsList();
        }

        #endregion


        #region IDisposable

        protected override void OnDispose()
        {
            foreach (var kvp in _pendingFriends)
                kvp.Value.ValueChanged -= OnPendingFriendValueChanged;
            _pendingFriends.Clear();

            if (_friendSearchResultReference != null)
                _friendSearchResultReference.ChildAdded -= OnSearchResultReceived;

            _view.BackButton.onClick.RemoveAllListeners();
            if (_friendRequestsReference != null)
                _friendRequestsReference.ChildAdded -= OnFriendRequestReceived;

            base.OnDispose();
        }

        #endregion
    }
}