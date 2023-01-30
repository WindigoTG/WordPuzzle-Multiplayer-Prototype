using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;

namespace WordPuzzle
{
    public class MessagesHolder
    {
        #region Fields

        private Dictionary<string, List<ChatMessage>> _sentMessagesByUserID = new Dictionary<string, List<ChatMessage>>();
        private Dictionary<string, List<ChatMessage>> _receivedMessagesByUserID = new Dictionary<string, List<ChatMessage>>();
        private Dictionary<string, (ChatMessage message, DatabaseReference messageRef)> _unreadMessages = new Dictionary<string, (ChatMessage, DatabaseReference)>();

        #endregion


        #region Properties

        public Dictionary<string, List<ChatMessage>> SentMessagesByUserID => _sentMessagesByUserID;
        public Dictionary<string, List<ChatMessage>> ReceivedMessagesByUserID => _receivedMessagesByUserID;
        public Dictionary<string, (ChatMessage message, DatabaseReference messageRef)> UnreadMessages => _unreadMessages;

        #endregion


        #region Methods

        public void AddSentMessage(string userId, ChatMessage message)
        {
            if (!_sentMessagesByUserID.ContainsKey(userId))
                _sentMessagesByUserID.Add(userId, new List<ChatMessage>());

            _sentMessagesByUserID[userId].Add(message);
        }

        public void AddReceivedMessage(string userId, ChatMessage message)
        {
            if (!_receivedMessagesByUserID.ContainsKey(userId))
                _receivedMessagesByUserID.Add(userId, new List<ChatMessage>());

            _receivedMessagesByUserID[userId].Add(message);
        }

        #endregion
    }
}