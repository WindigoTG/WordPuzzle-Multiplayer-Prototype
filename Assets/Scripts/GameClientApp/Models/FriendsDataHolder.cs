using System.Collections.Generic;

namespace WordPuzzle
{
    public class FriendsDataHolder
    {
        #region Fields

        private readonly Dictionary<string, UserProfileData> _friendsDataByID = new Dictionary<string, UserProfileData>();
        private readonly Dictionary<string, UserProfileData> _pendingFriendsDataByID = new Dictionary<string, UserProfileData>();
        private readonly Dictionary<string, UserProfileData> _friendRequestsDataByID = new Dictionary<string, UserProfileData>();

        private readonly Dictionary<string, UserProfileData> _friendSearchDataByID = new Dictionary<string, UserProfileData>();

        private readonly Dictionary<string, UserProfileData> _uncategorizedDataByID = new Dictionary<string, UserProfileData>();

        #endregion


        #region Properties

        public Dictionary<string, UserProfileData> FriendsDataByID => _friendsDataByID;
        public Dictionary<string, UserProfileData> PendingFriendsDataByID => _pendingFriendsDataByID;
        public Dictionary<string, UserProfileData> FriendRequestsDataByID => _friendRequestsDataByID;
        public Dictionary<string, UserProfileData> FriendSearchDataByID => _friendSearchDataByID;
        public Dictionary<string, UserProfileData> UncategorizedDataByID => _uncategorizedDataByID;

        #endregion


        #region Methods

        public void AddFriendData(string userID, UserProfileData userProfileData)
        {
            _friendsDataByID.Add(userID, userProfileData);
        }

        public void AddPendingFriendData(string userID, UserProfileData userProfileData)
        {
            _pendingFriendsDataByID.Add(userID, userProfileData);
        }

        public void AddFriendRequestData(string userID, UserProfileData userProfileData)
        {
            _friendRequestsDataByID.Add(userID, userProfileData);
        }

        public void AddFriendSearchData(string userID, UserProfileData userProfileData)
        {
            _friendSearchDataByID.Add(userID, userProfileData);
        }

        public void AddUncotigorizedUserData(string userID, UserProfileData userProfileData)
        {
            _uncategorizedDataByID.Add(userID, userProfileData);
        }

        #endregion
    }
}