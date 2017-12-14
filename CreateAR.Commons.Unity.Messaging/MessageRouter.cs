using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.Commons.Unity.Messaging
{
    /// <summary>
    /// Extra-safe message dispatch system.
    /// </summary>
    public class MessageRouter : IMessageRouter
    {
        /// <summary>
        /// A MessageSubscriberGroup specifically for SubscribeAll.
        /// </summary>
        private readonly MessageSubscriberGroup _all = new MessageSubscriberGroup(-1);

        /// <summary>
        /// A list of SubscriberGroups.
        /// </summary>
        private readonly List<MessageSubscriberGroup> _groups = new List<MessageSubscriberGroup>();

        /// <inheritdoc cref="IMessageRouter"/>
        public void Subscribe(
            int messageType,
            Action<object, Action> subscriber)
        {
            Group(messageType).AddSubscriber(subscriber);
        }

        /// <inheritdoc cref="IMessageRouter"/>
        public Action Subscribe(
            int messageType,
            Action<object> subscriber)
        {
            return Group(messageType)
                .AddSubscriber((message, unsub) => subscriber(message));
        }

        /// <inheritdoc cref="IMessageRouter"/>
        public void SubscribeOnce(
            int messageType,
            Action<object, Action> subscriber)
        {
            Group(messageType).AddSubscriber(subscriber, true);
        }

        /// <inheritdoc cref="IMessageRouter"/>
        public Action SubscribeOnce(
            int messageType,
            Action<object> subscriber)
        {
            return Group(messageType)
                .AddSubscriber((message, unsub) => subscriber(message), true);
        }

        /// <inheritdoc cref="IMessageRouter"/>
        public void SubscribeAll(Action<object, Action> subscriber)
        {
            _all.AddSubscriber(subscriber);
        }

        /// <inheritdoc cref="IMessageRouter"/>
        public Action SubscribeAll(Action<object> subscriber)
        {
            return _all.AddSubscriber((message, unsub) => subscriber(message));
        }

        /// <inheritdoc cref="IMessageRouter"/>
        public void Publish(
            int messageType,
            object message)
        {
            _all.Publish(message);

            for (int i = 0, len = _groups.Count; i < len; i++)
            {
                var group = _groups[i];
                if (group.MessageType == messageType)
                {
                    // cannot publish in the middle of a dispatch
                    if (group.IsDispatching)
                    {
                        Log.Warning(this, "Cyclical publish caught in middle of dispatch and discarded.\n\t[{0}] = {1}",
                            messageType,
                            message);
                        return;
                    }

                    group.Publish(message);

                    break;
                }
            }
        }

        /// <inheritdoc cref="IMessageRouter"/>
        public void Publish(int messageType)
        {
            Publish(messageType, Async.Void.Instance);
        }

        /// <inheritdoc cref="IMessageRouter"/>
        public void Consume(object message)
        {
            for (int i = 0, len = _groups.Count; i < len; i++)
            {
                var group = _groups[i];
                if (group.Message == message)
                {
                    group.Abort();

                    // keep going
                }
            }
        }

        /// <summary>
        /// Retrieves the <c>MessageSubscriberGroup</c> for a specific messageType.
        /// </summary>
        /// <param name="messageType">The messageType to retrieve the group for.</param>
        /// <returns></returns>
        private MessageSubscriberGroup Group(int messageType)
        {
            MessageSubscriberGroup subscribers = null;
            for (int i = 0, len = _groups.Count; i < len; i++)
            {
                var group = _groups[i];
                if (group.MessageType == messageType)
                {
                    subscribers = group;

                    break;
                }
            }

            if (null == subscribers)
            {
                subscribers = new MessageSubscriberGroup(messageType);
                _groups.Add(subscribers);
            }
            return subscribers;
        }
    }
}