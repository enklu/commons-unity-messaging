using System;

namespace CreateAR.Commons.Unity.Messaging
{
    /// <summary>
    /// Interface for sending and receiving messages.
    /// </summary>
    public interface IMessageRouter
    {
        /// <summary>
        /// Subscribes to a specific messageType.
        /// </summary>
        /// <param name="messageType">The messageType to subscribe to.</param>
        /// <param name="subscriber">The subscriber to be called.</param>
        void Subscribe(
            int messageType,
            Action<object, Action> subscriber);

        /// <summary>
        /// Subscribes to a specific messageType and returns a method to unsubscribe.
        /// </summary>
        /// <param name="messageType">The messageType to subscribe to.</param>
        /// <param name="subscriber">The subscriber to call.</param>
        /// <returns></returns>
        Action Subscribe(
            int messageType,
            Action<object> subscriber);

        /// <summary>
        /// Subscribes to a specific messageType once and only once. When the
        /// first message is received, the subscriber is immediately unsubscribed.
        /// </summary>
        /// <param name="messageType">The messageType to subscribe to.</param>
        /// <param name="subscriber">The subscriber to call.</param>
        void SubscribeOnce(
            int messageType,
            Action<object, Action> subscriber);

        /// <summary>
        /// Subscribes to a specific messageType once and only once and returns
        /// a method to unsubscribe. When the first message is received, the
        /// subscriber is immediately unsubscribed.
        /// </summary>
        /// <param name="messageType">The messageType to subscribe to.</param>
        /// <param name="subscriber">The subscriber to call.</param>
        Action SubscribeOnce(
            int messageType,
            Action<object> subscriber);

        /// <summary>
        /// Subscribes to all message types.
        /// </summary>
        /// <param name="subscriber">The subscriber to call.</param>
        void SubscribeAll(Action<object, Action> subscriber);

        /// <summary>
        /// Subscribes to all message types and returns a method for unsubscribing.
        /// </summary>
        /// <param name="subscriber">The subscriber to call.</param>
        Action SubscribeAll(Action<object> subscriber);

        /// <summary>
        /// Publishes a method that will call subscribers of this message type.
        /// </summary>
        /// <param name="messageType">The message type to publish to.</param>
        /// <param name="message">The message to publish.</param>
        void Publish(
            int messageType,
            object message);

        /// <summary>
        /// Consumes a message, preventing further subscriptions to be called.
        /// </summary>
        /// <param name="message">The message to consume.</param>
        void Consume(object message);
    }
}