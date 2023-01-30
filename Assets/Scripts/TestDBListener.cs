using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using System;
using System.Linq;
using TMPro;

public class TestDBListener : MonoBehaviour
{
    DatabaseReference _dbReference;
    [SerializeField] TextMeshProUGUI _text;
    string _message = "";
    bool _isMessagePending;

    DatabaseReference _test;
    DatabaseReference _test2;

    private void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                Debug.Log("Firebase depencencies resolved successfully");
                _dbReference = FirebaseDatabase.DefaultInstance.RootReference;

                //var testStruct = new TestStruct { Players = new string[] { "player1", "player2" } };

                //_dbReference.Push().SetRawJsonValueAsync(JsonUtility.ToJson(testStruct)).ContinueWith(task => { Debug.Log("Done"); });
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_isMessagePending)
        {
            _text.text = _message;
            _isMessagePending = false;
        }
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    public void SubscribeToListen()
    {
        _test = FirebaseDatabase.DefaultInstance.GetReference("Test");
        _test.ChildChanged += OnChildChanged;
        _test2 = FirebaseDatabase.DefaultInstance.GetReference("Test/Test");
        _test2.ValueChanged += OnValueChanged;
        //FirebaseDatabase.DefaultInstance.RootReference.ChildAdded += OnChildAdded;
        //FirebaseDatabase.DefaultInstance.RootReference.ChildChanged += OnChildChanged;
        //FirebaseDatabase.DefaultInstance.RootReference.ChildMoved += OnChildMoved;
        //FirebaseDatabase.DefaultInstance.RootReference.ChildRemoved += OnChildremoved;
    }

    public void Unsubscribe()
    {
        _test.ChildChanged -= OnChildChanged;
        _test2.ValueChanged -= OnValueChanged;
        //FirebaseDatabase.DefaultInstance.RootReference.ChildAdded -= OnChildAdded;
        //FirebaseDatabase.DefaultInstance.RootReference.ChildChanged -= OnChildChanged;
        //FirebaseDatabase.DefaultInstance.RootReference.ChildMoved -= OnChildMoved;
        //FirebaseDatabase.DefaultInstance.RootReference.ChildRemoved -= OnChildremoved;
    }

    private void OnValueChanged(object sender, ValueChangedEventArgs e)
    {
        Debug.Log($"Value Changed  |  {e.Snapshot.Key}  |  {e.Snapshot.Value.ToString()}  |  {DateTime.Now}");
        _message = $"Value Changed  |  {e.Snapshot.Key}  |  {e.Snapshot.Value.ToString()}  |  {DateTime.Now}";
        _isMessagePending = true;
    }

    private void OnChildremoved(object sender, ChildChangedEventArgs e)
    {
        Debug.Log($"Child removed  |  {e.Snapshot.Key}");
        _message = $"Child removed  |  {e.Snapshot.Key}";
        _isMessagePending = true;
        //Debug.Log($"Child removed  |  {sender.ToString()}  |  {e.PreviousChildName}");
    }

    private void OnChildMoved(object sender, ChildChangedEventArgs e)
    {
        Debug.Log($"Child Moved  |  {e.Snapshot.Key}");
        _message = $"Child Moved  |  {e.Snapshot.Key}";
        _isMessagePending = true;
        //Debug.Log($"Child Moved  |  {sender.ToString()}  |  {e.PreviousChildName}");
    }

    private void OnChildChanged(object sender, ChildChangedEventArgs e)
    {
        Debug.Log($"Child Changed  |  {e.Snapshot.Key}  |  {e.Snapshot.Value}  |  {DateTime.Now}");
        _message = $"Child Changed  |  {e.Snapshot.Key}  |  {e.Snapshot.Value}  |  {DateTime.Now}";
        _isMessagePending = true;
        //Debug.Log($"Child Changed  |  {sender.ToString()}  |  {e.PreviousChildName}");
    }

    private void OnChildAdded(object sender, ChildChangedEventArgs e)
    {
        Debug.Log($"Child Added  |  {e.Snapshot.Key}");
        _message = $"Child Added  |  {e.Snapshot.Key}";
        _isMessagePending = true;
        //Debug.Log($"Child Added  |  {sender.ToString()}  |  {e.PreviousChildName}");
    }
}

[System.Serializable]
public struct TestStruct
{
    public string[] Players;
    public WordPuzzle.Crossword Crossword;
}
