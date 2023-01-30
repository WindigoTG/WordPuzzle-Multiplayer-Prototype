using System;

namespace WordPuzzle
{
    public interface IReadOnlySubscriptionProperty<T>
    {
        T Value { get; }
        void SubscribeOnChange(Action<T> subscriptionAction);
        void UnSubscribeOnChange(Action<T> unsubscriptionAction);
    }
}
