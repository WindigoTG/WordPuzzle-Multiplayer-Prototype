using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using System.Threading.Tasks;

namespace WordPuzzle
{
    public class ProfileService : BaseController
    {
        #region Fields

        private Context _context;
        private DatabaseReference _usersReference;
        private DatabaseReference _thisUserReference;
        private FirebaseDatabase _dbInstance;
        private DatabaseReference _dbReference;
        private DatabaseReference _thisUserLikesReference;

        private PlayerProfileUIView _view;

        private FriendsDataHolder _friendsDataHolder = new FriendsDataHolder();

        private UserProfileData _playerProfileInfo;
        private UserProfileData _otherPlayerProfileInfo;
        private string _otherPlayerID;
        private List<InterestItemView> _interestItems = new List<InterestItemView>();

        private StorageService _storageService;

        private bool _isReady;

        private string _currentlyShownProfileID;

        #endregion


        #region Properties

        public UserProfileData LocalPlayerInfo => _playerProfileInfo;
        public UserProfileData OtherPlayerInfo => _otherPlayerProfileInfo;
        public FriendsDataHolder FriendsDataHolder => _friendsDataHolder;

        #endregion


        #region ClassLifeCycles

        public ProfileService(Context context)
        {
            _context = context;
            _context.SetProfileService(this);
            _dbInstance = FirebaseDatabase.DefaultInstance;
            _dbInstance.SetPersistenceEnabled(false);
            _dbReference = _dbInstance.RootReference;

            _usersReference = _dbInstance.GetReference(References.USERS_BRANCH);

            _storageService = new StorageService();

            InstantiatePrefab<PlayerProfileUIView>(_context.UIPrefabsData.PlayerProfileUi, _context.CommonUiHolder, InitView);
        }

        #endregion


        #region Methods

        private void InitView(PlayerProfileUIView view)
        {
            _view = view;
            _view.gameObject.SetActive(false);
            _view.HideProfileUI();
            _view.HideProfileEditUI();

            _view.CloseButton.onClick.AddListener(OnCloseProfileButtonClick);
            _view.EditProfileButton.onClick.AddListener(OnEditProfileButtonClick);
            _view.CancelButton.onClick.AddListener(OnCancelCuttonClick);
            _view.SaveButton.onClick.AddListener(OnSaveProfileButton);
            _view.EditPhotoButton.onClick.AddListener(OnEditPhotoButtonClick);
            _view.CameraButton.onClick.AddListener(OnTakePhotoWithCameraButtonClick);
            _view.GalleryButton.onClick.AddListener(OnLoadPhotoFromGalleryButtonClick);
        }

        public async Task CreateUserInDB(string userID, string firstName, string lastName, string nickname)
        {
            var currentDate = System.DateTime.UtcNow;
            var registrationDate = currentDate.Day + "." + currentDate.Month + "." + currentDate.Year;
            Task regTask1, regTask2, regTask3, regTask4;
            do
            {
                regTask1 = _usersReference.Child(userID).Child(References.FIRST_NAME).SetValueAsync(firstName);
                regTask2 = _usersReference.Child(userID).Child(References.LAST_NAME).SetValueAsync(lastName);
                regTask3 = _usersReference.Child(userID).Child(References.REGISTRATION_DATE).SetValueAsync(registrationDate);
                regTask4 = _usersReference.Child(userID).Child(References.NICKNAME).SetValueAsync(nickname);
                await Task.WhenAll(regTask1, regTask2, regTask3, regTask4);
            } while (regTask1.IsFaulted || regTask2.IsFaulted || regTask3.IsFaulted || regTask4.IsFaulted);
        }

        public async Task<UserProfileData> GetUserDataFromDB(string userID)
        {
            var userData = new UserProfileData();

            var getDataTask = _usersReference.Child(userID).GetValueAsync();

            await getDataTask;

            userData.FirstName = getDataTask.Result.Child(References.FIRST_NAME).Value.ToString();
            userData.LastName = getDataTask.Result.Child(References.LAST_NAME).Value.ToString();
            userData.Nickname = getDataTask.Result.HasChild(References.NICKNAME) ? getDataTask.Result.Child(References.NICKNAME).Value.ToString() : "";
            userData.RegistrationDate = getDataTask.Result.Child(References.REGISTRATION_DATE).Value.ToString();
            userData.Age = getDataTask.Result.HasChild(References.AGE) ? getDataTask.Result.Child(References.AGE).Value.ToString() : "";
            userData.Country = getDataTask.Result.HasChild(References.COUNTRY) ? getDataTask.Result.Child(References.COUNTRY).Value.ToString() : "";
            userData.City = getDataTask.Result.HasChild(References.CITY) ? getDataTask.Result.Child(References.CITY).Value.ToString() : "";

            try
            {
                userData.CrosswordsSolved = int.Parse(getDataTask.Result.Child(References.CROSSWORDS_SOLVED).Value.ToString());
            }
            catch
            {
                userData.CrosswordsSolved = 0;
            }

            try
            {
                userData.Likes = int.Parse(getDataTask.Result.Child(References.LIKES).Value.ToString());
            }
            catch
            {
                userData.Likes = 0;
            }

            List<string> interests = new List<string>();
            if (getDataTask.Result.HasChild(References.INTERESTS))
                foreach (var interest in getDataTask.Result.Child(References.INTERESTS).Children)
                    interests.Add(interest.Value.ToString());

            if (interests.Count > 0)
                userData.Interests = interests.ToArray();

            List<string> likesGiven = new List<string>();
            if (getDataTask.Result.HasChild(References.LIKES_GIVEN))
                foreach (var like in getDataTask.Result.Child(References.LIKES_GIVEN).Children)
                    likesGiven.Add(like.Value.ToString());
            userData.LikesGiven = likesGiven;

            var photo = _storageService.GetPhotoByUserID(userID);
            await photo;
            if (photo.Result != null)
                userData.Photo = photo.Result;

            return userData;
        }

        private async Task UpdateUserInDB(UserProfileData profileData)
        {
            var userID = _context.PlayerProfile.PlayerID;

            List<Task> tasks = new List<Task>();
            Task faultedTask;
            do
            {
                tasks.Clear();
                faultedTask = null;
                tasks.Add(_usersReference.Child(userID).Child(References.FIRST_NAME).SetValueAsync(profileData.FirstName));
                tasks.Add(_usersReference.Child(userID).Child(References.LAST_NAME).SetValueAsync(profileData.LastName));
                tasks.Add(_usersReference.Child(userID).Child(References.NICKNAME).SetValueAsync(profileData.Nickname));
                tasks.Add(_usersReference.Child(userID).Child(References.AGE).SetValueAsync(profileData.Age));
                tasks.Add(_usersReference.Child(userID).Child(References.COUNTRY).SetValueAsync(profileData.Country));
                tasks.Add(_usersReference.Child(userID).Child(References.CITY).SetValueAsync(profileData.City));
                tasks.Add(_storageService.UploadUserPhoto(profileData.Photo, userID));
                await Task.WhenAll(tasks.ToArray());
                faultedTask = tasks.Find(x => x.IsFaulted);
            } while (faultedTask != null);

            do
            {
                tasks.Clear();
                faultedTask = null;

                var clearTask = _usersReference.Child(userID).Child(References.INTERESTS).RemoveValueAsync();
                await clearTask;

                if (clearTask.IsFaulted)
                {
                    faultedTask = clearTask;
                    continue;
                }

                for (int i = 0; i < profileData.Interests.Length; i++)
                    tasks.Add(_usersReference.Child(userID).Child(References.INTERESTS).Child(i.ToString()).SetValueAsync(profileData.Interests[i]));

                await Task.WhenAll(tasks.ToArray());
                faultedTask = tasks.Find(x => x.IsFaulted);
            } while (faultedTask != null);
        }

        public async Task RetrievePlayerData()
        {
            Task<UserProfileData> userDataTask;
            do
            {
                userDataTask = GetUserDataFromDB(_context.PlayerProfile.PlayerID);
                await userDataTask;
            } while (userDataTask.IsFaulted);

            _isReady = true;

            _playerProfileInfo = userDataTask.Result;

            _thisUserLikesReference = _dbInstance.GetReference($"{References.USERS_BRANCH}/{_context.PlayerProfile.PlayerID}/{References.LIKES}");
            _thisUserLikesReference.ValueChanged += OnLikesValueChanged;
        }

        private void OnLikesValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (e.Snapshot.Value == null)
                return;

            try
            {
                _playerProfileInfo.Likes = int.Parse(e.Snapshot.Value.ToString());
            }
            finally
            {
                if (_context.PlayerProfile.PlayerID.Equals(_currentlyShownProfileID))
                    _view.SetUserLikes(_playerProfileInfo.Likes.ToString());
            }
        }

        public async void RetrieveOtherPlayerData(string playerID)
        {
            _otherPlayerID = playerID;

            if (_friendsDataHolder.FriendsDataByID.ContainsKey(_otherPlayerID))
            {
                _otherPlayerProfileInfo = _friendsDataHolder.FriendsDataByID[_otherPlayerID];
                return;
            }

            if (_friendsDataHolder.PendingFriendsDataByID.ContainsKey(_otherPlayerID))
            {
                _otherPlayerProfileInfo = _friendsDataHolder.PendingFriendsDataByID[_otherPlayerID];
                return;
            }

            if (_friendsDataHolder.FriendRequestsDataByID.ContainsKey(_otherPlayerID))
            {
                _otherPlayerProfileInfo = _friendsDataHolder.FriendRequestsDataByID[_otherPlayerID];
                return;
            }

            Task<UserProfileData> userDataTask;
            do
            {
                userDataTask = GetUserDataFromDB(playerID);
                await userDataTask;
            } while (userDataTask.IsFaulted);

            _otherPlayerProfileInfo = userDataTask.Result;
        }

        public void ShowPlayerProfileUI()
        {
            if (_view.gameObject.activeSelf || !_isReady)
                return;

            FillProfileInfo(_playerProfileInfo);

            _view.gameObject.SetActive(true);
            _view.ShowProfileUI(true);

            _currentlyShownProfileID = _context.PlayerProfile.PlayerID;
        }

        public void ShowOtherPlayerInMatchProfile()
        {
            ShowOtherUserProfile(_otherPlayerID, _otherPlayerProfileInfo);
        }

        private void ShowOtherUserProfile(string userID, UserProfileData userData)
        {
            if (_view.gameObject.activeSelf)
                return;

            FillProfileInfo(userData);

            _view.gameObject.SetActive(true);
            _view.ShowProfileUI(false);

            _view.AddFriendButton.onClick.AddListener(() => { _context.FriendListController.OnAddFriendButtonClick(userID); _view.AddFriendButton.interactable = false; });
            _view.AddFriendButton.interactable = (!(_friendsDataHolder.FriendsDataByID.ContainsKey(userID) || _friendsDataHolder.PendingFriendsDataByID.ContainsKey(userID)));

            _view.LikeButton.gameObject.SetActive(true);
            _view.LikeButton.interactable = !_playerProfileInfo.LikesGiven.Contains(userID);
            _view.LikeButton.onClick.AddListener(() => OnLikeButtonClick(userID));

            _view.SendMessageButton.onClick.AddListener(() => _context.ChatController.ShowChatWithUser(userID));

            _currentlyShownProfileID = userID;
        }

        public void ShowOtherUserProfile(string userId)
        {
            if (_friendsDataHolder.FriendsDataByID.ContainsKey(userId))
            {
                ShowOtherUserProfile(userId, _friendsDataHolder.FriendsDataByID[userId]);
                return;
            }

            if (_friendsDataHolder.PendingFriendsDataByID.ContainsKey(userId))
            {
                ShowOtherUserProfile(userId, _friendsDataHolder.PendingFriendsDataByID[userId]);
                return;
            }

            if (_friendsDataHolder.FriendRequestsDataByID.ContainsKey(userId))
            {
                ShowOtherUserProfile(userId, _friendsDataHolder.FriendRequestsDataByID[userId]);
                return;
            }

            if(_friendsDataHolder.FriendSearchDataByID.ContainsKey(userId))
            {
                ShowOtherUserProfile(userId, _friendsDataHolder.FriendSearchDataByID[userId]);
                return;
            }
        }

        public bool HasProfileData(string userId)
        {
            if (userId.Equals(_otherPlayerID))
                return true;

            if (_friendsDataHolder.FriendsDataByID.ContainsKey(userId))
                return true;

            if (_friendsDataHolder.PendingFriendsDataByID.ContainsKey(userId))
                return true;

            if (_friendsDataHolder.FriendRequestsDataByID.ContainsKey(userId))
                return true;

            if (_friendsDataHolder.FriendSearchDataByID.ContainsKey(userId))
                return true;

            if (_friendsDataHolder.UncategorizedDataByID.ContainsKey(userId))
                return true;

            return false;
        }

        public UserProfileData GetUserDataToDisplay(string userId)
        {
            UserProfileData profileData;

            if (userId.Equals(_otherPlayerID))
                profileData = _otherPlayerProfileInfo;

            else if (_friendsDataHolder.FriendsDataByID.ContainsKey(userId))
                profileData = _friendsDataHolder.FriendsDataByID[userId];

            else if (_friendsDataHolder.PendingFriendsDataByID.ContainsKey(userId))
                profileData = _friendsDataHolder.PendingFriendsDataByID[userId];

            else if (_friendsDataHolder.FriendRequestsDataByID.ContainsKey(userId))
                profileData = _friendsDataHolder.FriendRequestsDataByID[userId];

            else if (_friendsDataHolder.FriendSearchDataByID.ContainsKey(userId))
                profileData = _friendsDataHolder.FriendSearchDataByID[userId];
            else if (_friendsDataHolder.UncategorizedDataByID.ContainsKey(userId))
                profileData = _friendsDataHolder.UncategorizedDataByID[userId];
            else
                profileData = new UserProfileData();

            return profileData;
        }

        private void FillProfileInfo(UserProfileData userProfile)
        {
            _view.SetUserName(userProfile.FirstName, userProfile.LastName, userProfile.Nickname);
            _view.SetUserAge(userProfile.Age);
            _view.SetUserLocation(userProfile.Country, userProfile.City);
            _view.SetUserRegistrationDate(userProfile.RegistrationDate);
            _view.SetUserInterests(userProfile.Interests);
            _view.SetUserLikes(userProfile.Likes.ToString());

            if (userProfile.Photo != null)
                _view.SetUserPhoto(userProfile.Photo);
            else
                _view.SetBlankUserPhoto();
        }

        public void HidePlayerProfileUI()
        {
            _view.gameObject.SetActive(false);
            _view.AddFriendButton.onClick.RemoveAllListeners();
            _view.LikeButton.onClick.RemoveAllListeners();
            _view.SendMessageButton.onClick.RemoveAllListeners();
        }

        private void OnLikeButtonClick(string userID)
        {
            if (userID.Equals(_otherPlayerID))
            {
                _otherPlayerProfileInfo.Likes++;
                if (_currentlyShownProfileID.Equals(_otherPlayerID))
                    _view.SetUserLikes(_otherPlayerProfileInfo.Likes.ToString());
            }

            if (_friendsDataHolder.FriendsDataByID.ContainsKey(userID))
            {
                var userData = _friendsDataHolder.FriendsDataByID[userID];
                userData.Likes++;
                _friendsDataHolder.FriendsDataByID[userID] = userData;
                _view.SetUserLikes(userData.Likes.ToString());
            }

            if (_friendsDataHolder.PendingFriendsDataByID.ContainsKey(userID))
            {
                var userData = _friendsDataHolder.PendingFriendsDataByID[userID];
                userData.Likes++;
                _friendsDataHolder.PendingFriendsDataByID[userID] = userData;
                _view.SetUserLikes(userData.Likes.ToString());
            }

            if (_friendsDataHolder.FriendRequestsDataByID.ContainsKey(userID))
            {
                var userData = _friendsDataHolder.FriendRequestsDataByID[userID];
                userData.Likes++;
                _friendsDataHolder.FriendRequestsDataByID[userID] = userData;
                _view.SetUserLikes(userData.Likes.ToString());
            }

            _view.LikeButton.interactable = false;
            PostLike(userID);
        }

        private async void PostLike(string userID)
        {
            _playerProfileInfo.LikesGiven.Add(userID);

            Task task;
            do
            {
                task = _usersReference.Child(_context.PlayerProfile.PlayerID).Child(References.LIKES_GIVEN).Child(userID).SetValueAsync(userID);
                await task;
            } while (task.IsFaulted);

            do
            {
                task = _dbReference.Child(References.LIKES).Child(userID).Child(_context.PlayerProfile.PlayerID).SetValueAsync(_context.PlayerProfile.PlayerID);
                await task;
            } while (task.IsFaulted);
        }

        private void OnEditProfileButtonClick()
        {
            if (_playerProfileInfo.Interests != null)
            {
                for (int i = 0; i < _playerProfileInfo.Interests.Length; i++)
                {
                    AddInterestItem();
                    _interestItems[i].SetInputModeWithText(_playerProfileInfo.Interests[i]);
                }
            }
            AddInterestItem();
             
            _view.FillProfileEditInputFields(_playerProfileInfo);

            if (_playerProfileInfo.Photo != null)
                _view.SetUserEditPhoto(_playerProfileInfo.Photo);
            _view.HideProfileUI();
            _view.ShowProfileEditUI();
        }

        private void AddInterestItem()
        {
            if (_interestItems.Count >= 5)
                return;

            var item = Object.Instantiate(_context.UIPrefabsData.InterestItemPrefab, _view.InterestsParent);
            item.AddButton.onClick.AddListener(() => {OnAddInterestButtonClick(item); });
            item.RemoveButton.onClick.AddListener(() => { OnRemoveInterestButtonClick(item); });
            item.SetBlankMode();

            _interestItems.Add(item);
        }

        private void OnAddInterestButtonClick(InterestItemView item)
        {
            if (!_interestItems.Contains(item))
                return;

            var emptyItem = _interestItems.Find(x => string.IsNullOrWhiteSpace(x.Text) && x != item);
            if (emptyItem != null)
                return;

            item.SetInputMode();

            if (_interestItems.Count < 5)
                AddInterestItem();
        }

        private void OnRemoveInterestButtonClick(InterestItemView item)
        {
            if (!_interestItems.Contains(item))
                return;

            var itemIndex = _interestItems.IndexOf(item);
            if (itemIndex == _interestItems.Count -1)
                item.SetBlankMode();
            else
            {
                Object.Destroy(_interestItems[itemIndex].gameObject);
                _interestItems.Remove(item);

                var existingBlankItem = _interestItems.Find(x => x.IsBlankMode);
                if (existingBlankItem == null)
                    AddInterestItem();
            }
        }

        private void ClearInterestItems()
        {
            foreach (var item in _interestItems)
                Object.Destroy(item.gameObject);

            _interestItems.Clear();
        }

        private void OnCancelCuttonClick()
        {
            _view.ShowProfileUI(true);
            _view.HideProfileEditUI();
            ClearInterestItems();
        }

        private async void OnSaveProfileButton()
        {
            var newProfileInfo = _playerProfileInfo;

            if (!string.IsNullOrWhiteSpace(_view.EditFirstNameText)) newProfileInfo.FirstName = _view.EditFirstNameText;
            if (!string.IsNullOrWhiteSpace(_view.EditLastNameText)) newProfileInfo.LastName = _view.EditLastNameText;
            if (!string.IsNullOrWhiteSpace(_view.EditNicknameText)) newProfileInfo.Nickname = _view.EditNicknameText;
            if (!string.IsNullOrWhiteSpace(_view.EditAgeText)) newProfileInfo.Age = _view.EditAgeText;
            if (!string.IsNullOrWhiteSpace(_view.EditCountryText)) newProfileInfo.Country = _view.EditCountryText;
            if (!string.IsNullOrWhiteSpace(_view.EditCityText)) newProfileInfo.City = _view.EditCityText;

            var interests = new List<string>();
            foreach (var interest in _interestItems)
                if (!string.IsNullOrWhiteSpace(interest.Text))
                    interests.Add(interest.Text);

            newProfileInfo.Interests = interests.ToArray();

            if (_view.EditedPhoto != _playerProfileInfo.Photo)
                newProfileInfo.Photo = _view.EditedPhoto;

            await UpdateUserInDB(newProfileInfo);
            _playerProfileInfo = newProfileInfo;

            FillProfileInfo(_playerProfileInfo);

            _view.ShowProfileUI(true);
            _view.HideProfileEditUI();
            ClearInterestItems();
        }

        private void OnCloseProfileButtonClick()
        {
            HidePlayerProfileUI();
        }

        private void OnEditPhotoButtonClick()
        {
            _view.ShowPhotoSourceDialogue();
        }

        private void OnLoadPhotoFromGalleryButtonClick()
        {
            PhotoSelectionHandler.GetPictureFromGallery(
                (picture) => { _view.SetUserEditPhoto(picture); },
                () => { });

            _view.HidePhotoSourceDialogue();
        }

        private void OnTakePhotoWithCameraButtonClick()
        {
            PhotoSelectionHandler.TakePhotoWithCamera(
                (picture) => { _view.SetUserEditPhoto(picture); },
                () => { });

            _view.HidePhotoSourceDialogue();
        }

        public async void IncrementSolvedCrosswordsCount()
        {
            _playerProfileInfo.CrosswordsSolved++;

            Task task;
            do
            {
                task = _usersReference.Child(_context.PlayerProfile.PlayerID).Child(References.CROSSWORDS_SOLVED).SetValueAsync(_playerProfileInfo.CrosswordsSolved);
                await task;
            } while (task.IsFaulted);
        }

        #endregion


        #region IDisposeable

        protected override void OnDispose()
        {
            if (_thisUserLikesReference != null)
                _thisUserLikesReference.ValueChanged -= OnLikesValueChanged;
            base.OnDispose();
        }

        #endregion
    }
}