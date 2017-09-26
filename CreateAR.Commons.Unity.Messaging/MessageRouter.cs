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

        /// <summary>
        /// Subscribes to a specific messageType.
        /// </summary>
        /// <param name="messageType">The messageType to subscribe to.</param>
        /// <param name="subscriber">The subscriber to be called.</param>
        public void Subscribe(
            int messageType,
            Action<object, Action> subscriber)
        {
            Group(messageType).AddSubscriber(subscriber);
        }

        /// <summary>
        /// Subscribes to a specific messageType and returns a method to unsubscribe.
        /// </summary>
        /// <param name="messageType">The messageType to subscribe to.</param>
        /// <param name="subscriber">The subscriber to call.</param>
        /// <returns></returns>
        public Action Subscribe(
            int messageType,
            Action<object> subscriber)
        {
            return Group(messageType)
                .AddSubscriber((message, unsub) => subscriber(message));
        }

        /// <summary>
        /// Subscribes to a specific messageType once and only once. When the
        /// first message is received, the subscriber is immediately unsubscribed.
        /// </summary>
        /// <param name="messageType">The messageType to subscribe to.</param>
        /// <param name="subscriber">The subscriber to call.</param>
        public void SubscribeOnce(
            int messageType,
            Action<object, Action> subscriber)
        {
            Group(messageType).AddSubscriber(subscriber, true);
        }

        /// <summary>
        /// Subscribes to a specific messageType once and only once and returns
        /// a method to unsubscribe. When the first message is received, the
        /// subscriber is immediately unsubscribed.
        /// </summary>
        /// <param name="messageType">The messageType to subscribe to.</param>
        /// <param name="subscriber">The subscriber to call.</param>
        public Action SubscribeOnce(
            int messageType,
            Action<object> subscriber)
        {
            return Group(messageType)
                .AddSubscriber((message, unsub) => subscriber(message), true);
        }

        /// <summary>
        /// Subscribes to all message types.
        /// </summary>
        /// <param name="subscriber">The subscriber to call.</param>
        public void SubscribeAll(Action<object, Action> subscriber)
        {
            _all.AddSubscriber(subscriber);
        }

        /// <summary>
        /// Subscribes to all message types and returns a method for unsubscribing.
        /// </summary>
        /// <param name="subscriber">The subscriber to call.</param>
        public Action SubscribeAll(Action<object> subscriber)
        {
            return _all.AddSubscriber((message, unsub) => subscriber(message));
        }

        /// <summary>
        /// Publishes a method that will call subscribers of this message type.
        /// </summary>
        /// <param name="messageType">The message type to publish to.</param>
        /// <param name="message">The message to publish.</param>
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

        /// <summary>
        /// Consumes a message, preventing further subscriptions to be called.
        /// </summary>
        /// <param name="message">The message to consume.</param>
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