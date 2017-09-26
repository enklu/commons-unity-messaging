using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;

namespace CreateAR.Commons.Unity.Messaging
{
    /// <summary>
    /// Represents a group of subscribers all subscribed to the same message
    /// type.
    /// </summary>
    internal class MessageSubscriberGroup
    {
        /// <summary>
        /// List of subscribers.
        /// </summary>
        private readonly List<Action<object>> _subscribers = new List<Action<object>>();

        /// <summary>
        /// List of subscribers that unsubscribed whilst dispatching.
        /// </summary>
        private readonly List<Action<object>> _toUnsubscribe = new List<Action<object>>();

        /// <summary>
        /// Used to gather exceptions.
        /// </summary>
        private readonly List<Exception> _exceptionScratch = new List<Exception>();

        /// <summary>
        /// Backing variable for Message property.
        /// </summary>
        private object _message;

        /// <summary>
        /// True iff Consume was called whilst dispatching.
        /// </summary>
        private bool _isAborted = false;

        /// <summary>
        /// Integral message type.
        /// </summary>
        public int MessageType { get; private set; }

        /// <summary>
        /// True iff currently dispatching
        /// </summary>
        public bool IsDispatching { get; private set; }

        /// <summary>
        /// Message we are currently dispatching. Only set whilst IsDispatching
        /// is true.
        /// </summary>
        public object Message {
            get
            {
                return _message;
            }
            private set
            {
                _message = value;

                IsDispatching = null != _message;

                if (null == value)
                {
                    _isAborted = false;
                }
            }
        }

        /// <summary>
        /// Creates a new subscriber group of a specific message type.
        /// </summary>
        /// <param name="messageType">The message type to subscribe to.</param>
        public MessageSubscriberGroup(int messageType)
        {
            MessageType = messageType;
        }
            
        /// <summary>
        /// Adds a subscriber.
        /// </summary>
        /// <param name="subscriber">The subscriber.</param>
        /// <param name="once">True iff this subscriber should be called only once.</param>
        /// <returns></returns>
        public Action AddSubscriber(Action<object, Action> subscriber, bool once = false)
        {
            Action unsub = null;
            Action<object> action = message =>
            {
                subscriber(message, unsub);

                if (once)
                {
                    unsub();
                }
            };

            unsub = () =>
            {
                if (IsDispatching)
                {
                    _toUnsubscribe.Add(action);
                }
                else
                {
                    _subscribers.Remove(action);
                }
            };

            _subscribers.Add(action);

            return unsub;
        }

        /// <summary>
        /// Publishes a message.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        public void Publish(object message)
        {
            Message = message;
            
            _exceptionScratch.Clear();

            for (int i = 0, len = _subscribers.Count; i < len; i++)
            {
                try
                {
                    _subscribers[i](message);
                }
                catch (Exception exception)
                {
                    _exceptionScratch.Add(exception);
                }

                if (_isAborted)
                {
                    break;
                }
            }

            // unsubscribes
            var length = _toUnsubscribe.Count;
            if (length > 0)
            {
                for (var i = 0; i < length; i++)
                {
                    _subscribers.Remove(_toUnsubscribe[i]);
                }
                _toUnsubscribe.Clear();
            }

            Message = null;

            var exceptions = _exceptionScratch.Count;
            if (1 == exceptions)
            {
                throw _exceptionScratch[0];
            }

            if (exceptions > 1)
            {
                var aggregate = new AggregateException();
                aggregate.Exceptions.AddRange(_exceptionScratch);

                throw aggregate;
            }
        }

        /// <summary>
        /// Aborts dispatching.
        /// </summary>
        public void Abort()
        {
            if (IsDispatching)
            {
                _isAborted = true;
            }
        }
    }
}