using System.Collections.Generic;
using UnityEngine;

namespace WordPuzzle
{
    public struct UserProfileData
    {
        public string FirstName;
        public string LastName;
        public string Nickname;
        public string Country;
        public string City;
        public string Age;
        public string RegistrationDate;
        public string[] Friends;
        public string[] Interests;
        public int CrosswordsSolved;
        public int Likes;
        public List<string> LikesGiven;
        public Texture2D Photo;
    }
}