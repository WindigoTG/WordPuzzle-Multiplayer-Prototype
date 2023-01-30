namespace WordPuzzle
{
    public class PlayerProfile
    {
        public SubscriptionProperty<GameState> CurrentState { get; }
        public int SelectedTheme;
        public int CurrentLevel;

        public string PlayerID;
        public string MatchID;

        public bool IsLevelPreSet;
        public Crossword CrosswordToPlay;

        public bool IsOnlinePlaySelected;

        public PlayerProfile()
        {
            CurrentState = new SubscriptionProperty<GameState>();
        }
    }
}