using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace WordPuzzle
{
    public class ChatController : BaseController
    {
        #region Fields

        private Context _context;
        private ChatWindowView _view;
        private FirebaseDatabase _dbInstance;
        private DatabaseReference _dbReference;
        private DatabaseReference _mRepositoryReference;
        private DatabaseReference _mIncomingReference;
        private DatabaseReference _mOutgoingReference;
        private DatabaseReference _mPlayerIncomingReference;
        private DatabaseReference _mPlayerOutgoingReference;

        private MessagesHolder _messagesHolder = new MessagesHolder();

        private List<ChatMesageView> _messages = new List<ChatMesageView>();

        private string _playerID;

        private string _currentChatUserID;

        #endregion


        #region ClassLifeCycles

        public ChatController(Context context)
        {
            _context = context;
            _context.SetChatController(this);

            _dbInstance = FirebaseDatabase.DefaultInstance;
            _dbInstance.SetPersistenceEnabled(false);
            _dbReference = _dbInstance.RootReference;
            _mRepositoryReference = _dbInstance.GetReference($"{References.MESSAGES_BRANCH}/{References.MESSAGES_REPOSITORY}");
            _mIncomingReference = _dbInstance.GetReference($"{References.MESSAGES_BRANCH}/{References.MESSAGES_INCOMING}");
            _mOutgoingReference = _dbInstance.GetReference($"{References.MESSAGES_BRANCH}/{References.MESSAGES_OUTGOING}");

            InstantiatePrefab<ChatWindowView>(_context.UIPrefabsData.ChatWindowPrefab, _context.CommonUiHolder, InitView);
        }

        #endregion


        #region Methods

        private void InitView(ChatWindowView view)
        {
            _view = view;

            _view.HideChatWindow();
            _view.BackButton.onClick.AddListener(() =>
            {
                _view.HideChatWindow();
                _view.SendButton.onClick.RemoveAllListeners();
                ClearMessageList();
                _currentChatUserID = null;
                _view.ResetInput();
            });
        }

        public async Task RetrieveMessages()
        {
            _playerID = _context.PlayerProfile.PlayerID;

            _mPlayerIncomingReference = _dbInstance.GetReference($"{References.MESSAGES_BRANCH}/{References.MESSAGES_INCOMING}/{_playerID}");
            _mPlayerOutgoingReference = _dbInstance.GetReference($"{References.MESSAGES_BRANCH}/{References.MESSAGES_OUTGOING}/{_playerID}");

            Task<DataSnapshot> task;
            do
            {
                task = _mPlayerOutgoingReference.GetValueAsync();
                await task;
            } while (task.IsFaulted);

            foreach (var message in task.Result.Children)
                RetrieveMessage(message.Key.ToString());

            _mPlayerIncomingReference.ChildAdded += OnMessageReceived;
        }

        private async Task<ChatMessage> RetrieveMessage(string messageId)
        {
            Task<DataSnapshot> task;
            do
            {
                task = _mRepositoryReference.Child(messageId).GetValueAsync();
                await task;
            } while (task.IsFaulted);

            if (task.Result == null)
                return null;

            ChatMessage message = new ChatMessage
            {
                Text = task.Result.Child(References.MESSAGES_CONTENT).Value.ToString(),
                IsSeen = bool.Parse(task.Result.Child(References.MESSAGES_IS_SEEN).Value.ToString()),
                Time = DateTime.FromBinary(long.Parse(task.Result.Child(References.MESSAGES_TIMESTAMP).Value.ToString())).ToLocalTime(),
                MessageID = messageId,
                From = task.Result.Child(References.MESSAGES_FROM).Value.ToString(),
                To = task.Result.Child(References.MESSAGES_TO).Value.ToString()
            };

            if (message.From.ToString().Equals(_playerID))
                _messagesHolder.AddSentMessage(message.To, message);
            else
                _messagesHolder.AddReceivedMessage(message.From, message);
                
            if (!message.IsSeen)
                SubscribeToMessageIsSeenValueChange(message);

            return message;
        }

        private void SubscribeToMessageIsSeenValueChange(ChatMessage message)
        {
            var messageRef = _dbInstance.GetReference($"{References.MESSAGES_BRANCH}/{References.MESSAGES_REPOSITORY}/{message.MessageID}/{References.MESSAGES_IS_SEEN}");
            messageRef.ValueChanged += OnMessageSeen;
            _messagesHolder.UnreadMessages.Add(message.MessageID, (message, messageRef));
        }

        private void OnMessageSeen(object sender, ValueChangedEventArgs e)
        {
            if (!bool.Parse(e.Snapshot.Value.ToString()))
                return;

            var messageID = e.Snapshot.Reference.Parent.Key.ToString();

            if (!_messagesHolder.UnreadMessages.ContainsKey(messageID))
                return;

            _messagesHolder.UnreadMessages[messageID].messageRef.ValueChanged -= OnMessageSeen;
            _messagesHolder.UnreadMessages[messageID].message.IsSeen = true;
            _messagesHolder.UnreadMessages.Remove(messageID);
        }

        public async void SendMessageToUser(string messageText, string userID)
        {
            if (string.IsNullOrWhiteSpace(messageText))
                return;

            var timestamp = DateTime.UtcNow.ToBinary();
            var messageId = $"{_playerID}-{timestamp}";

            ChatMessage message = new ChatMessage
            {
                Text = messageText,
                IsSeen = false,
                Time = DateTime.FromBinary(timestamp).ToLocalTime(),
                MessageID = messageId,
                From = _playerID,
                To = userID
            };

            SubscribeToMessageIsSeenValueChange(message);
            _messagesHolder.AddSentMessage(userID, message);

            AddMessageToList(message);

            List<Task> messageTasks = new List<Task>();
            Task faultedTask;

            do
            {
                messageTasks.Clear();
                messageTasks.Add(_mRepositoryReference.Child(messageId).Child(References.MESSAGES_FROM).SetValueAsync(_playerID));
                messageTasks.Add(_mRepositoryReference.Child(messageId).Child(References.MESSAGES_TO).SetValueAsync(userID));
                messageTasks.Add(_mRepositoryReference.Child(messageId).Child(References.MESSAGES_IS_SEEN).SetValueAsync(false));
                messageTasks.Add(_mRepositoryReference.Child(messageId).Child(References.MESSAGES_TIMESTAMP).SetValueAsync(timestamp));
                messageTasks.Add(_mRepositoryReference.Child(messageId).Child(References.MESSAGES_CONTENT).SetValueAsync(messageText));
                await Task.WhenAll(messageTasks.ToArray());

                faultedTask = messageTasks.Find(x => x.IsFaulted);
            } while (faultedTask != null);

            Task task;

            do
            {
                task = _mOutgoingReference.Child(_playerID).Child(messageId).SetValueAsync(messageId);
                await task;
            } while (task.IsFaulted);

            do
            {
                task = _mIncomingReference.Child(userID).Child(messageId).SetValueAsync(messageId);
                await task;
            } while (task.IsFaulted);
        }

        private async void OnMessageReceived(object sender, ChildChangedEventArgs e)
        {
            var message = RetrieveMessage(e.Snapshot.Key.ToString());

            await message;

            if (message.Result == null)
                return;

            if (message.Result.From.Equals(_currentChatUserID))
                AddMessageToList(message.Result);
            else
            {
                if (!_context.ProfileService.HasProfileData(message.Result.From))
                {
                    var task = _context.ProfileService.GetUserDataFromDB(message.Result.From);
                    await task;
                    _context.ProfileService.FriendsDataHolder.AddUncotigorizedUserData(message.Result.From, task.Result);
                }

                if (!message.Result.IsSeen)
                {
                    var userInfo = _context.ProfileService.GetUserDataToDisplay(message.Result.From);
                    _view.ShowNewMessageNotificationForUser(userInfo);
                }
            }
        }

        public void ShowChatWithUser(string userID)
        {
            if (_view.ChatWindowActiveSelf)
                return;

            var userData = _context.ProfileService.GetUserDataToDisplay(userID);
            _view.ShowChatWindowForUser(userData);

            BuildMessageListToDisplay(userID);
            _view.SendButton.onClick.AddListener(() => { 
                SendMessageToUser(_view.InputText, userID);
                _view.ResetInput();
            });

            _currentChatUserID = userID;
        }

        private async void BuildMessageListToDisplay(string userId)
        {
            ClearMessageList();

            List<ChatMessage> messages = new List<ChatMessage>();

            if (_messagesHolder.SentMessagesByUserID.ContainsKey(userId))
                messages.AddRange(_messagesHolder.SentMessagesByUserID[userId]);

            if (_messagesHolder.ReceivedMessagesByUserID.ContainsKey(userId))
                messages.AddRange(_messagesHolder.ReceivedMessagesByUserID[userId]);

            messages.Sort(CompareMessageDates);

            foreach (var message in messages)
            {
                CreateChatMessageView(message);
                if (message.To.Equals(_playerID) && !message.IsSeen)
                {
                    message.IsSeen = true;
                    PostMessageIsSeen(message);
                }
            }

            while (!AreMessagesAdjusted())
                await Task.Yield();

            float totalHeight = 0;
            foreach (var message in _messages)
                totalHeight += (message.transform as RectTransform).sizeDelta.y;

            var size = _view.ScrollRect.content.sizeDelta;
            size.y = totalHeight;
            _view.ScrollRect.content.sizeDelta = size;

        }

        private async void AddMessageToList(ChatMessage message)
        {
            var messageView = CreateChatMessageView(message);

            while (!messageView.IsAdjusted)
                await Task.Yield();

            var size = _view.ScrollRect.content.sizeDelta;
            size.y += (messageView.transform as RectTransform).sizeDelta.y;
            _view.ScrollRect.content.sizeDelta = size;

            if (message.To.Equals(_playerID) && !message.IsSeen)
            {
                message.IsSeen = true;
                PostMessageIsSeen(message);
            }
        }

        private async void PostMessageIsSeen(ChatMessage message)
        {
            Task task;
            do
            {
                task = _mRepositoryReference.Child(message.MessageID).Child(References.MESSAGES_IS_SEEN).SetValueAsync(message.IsSeen);
                await task;
            } while (task.IsFaulted);
        }

        private bool AreMessagesAdjusted()
        {
            var messageNotAdjusted = _messages.Find(x => !x.IsAdjusted);
            return messageNotAdjusted == null;
        }

        private ChatMesageView CreateChatMessageView(ChatMessage message)
        {
            var messageView = UnityEngine.Object.Instantiate(_context.UIPrefabsData.ChatMessageItemPrefab, _view.ScrollRect.content);
            messageView.SetText(message.Text, message.From.Equals(_playerID));
            _messages.Add(messageView);

            return messageView;
        }

        private int CompareMessageDates(ChatMessage a, ChatMessage b)
        {
            if (a == null)
            {
                if (b == null)
                    return 0;
                else
                    return - 1;
            }
            else
            {
                if (b == null)
                    return 1;
                else
                {
                    var timeDif = a.Time.Ticks - b.Time.Ticks;

                    if (timeDif == 0)
                        return 0;

                    if (timeDif > 0)
                        return 1;
                    else
                        return -1;
                }
            }
        }

        private void ClearMessageList()
        {
            var size = _view.ScrollRect.content.sizeDelta;
            size.y = 0;
            _view.ScrollRect.content.sizeDelta = size;

            foreach (var message in _messages)
                UnityEngine.Object.Destroy(message.gameObject);
            _messages.Clear();
        }

        #endregion


        #region IDisposable

        protected override void OnDispose()
        {
            foreach (var kvp in _messagesHolder.UnreadMessages)
                kvp.Value.messageRef.ValueChanged -= OnMessageSeen;
            _messagesHolder.UnreadMessages.Clear();

            if (_mPlayerIncomingReference != null)
                _mPlayerIncomingReference.ChildAdded -= OnMessageReceived;

            _view.SendButton.onClick.RemoveAllListeners();
            _view.BackButton.onClick.RemoveAllListeners();

            base.OnDispose();
        }

        #endregion
    }
}