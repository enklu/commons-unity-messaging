# Overview

#### Aim

The messaging library is designed to be intuitive and simple. It _is not_ designed for performance. This doesn't mean that it's slow (it's actually pretty fast), but it should not be used for passing hundreds (thousands?) of events per frame. Our messaging system is not a message _bus_ intended for anything and everything to pass through. For performance sensitive applications, just call functions directly. This messaging system is also, very purposefully, _single threaded_. For use in multithreaded applications, be sure to move messages to a single thread before publishing them.

#### Abstract

The messaging system has one main class: `MessageRouter` with two primary methods: `publish` and `subscribe`. Both are very straightforward and can pass any type of object. A base class for messages is not required (nor encouraged).

You can have any number of `MessageRouter`s in your application. They do not share state of any kind.

#### Subscribe

To subscribe to a type of event, simply call one of the flavors of `Subscribe` with an integral message type.

```csharp
// subscribe to a message type
router.Subscribe(messageType, message => {
	...
});

// subscribe once
router.SubscribeOnce(messageType, message => {
	...
	
	// automatically unsubscribed
});

// subscribe to all
router.SubscribeAll((message, messageType) => {
	// this method will receive all messages

	...
});
```

#### Unsubscribe

Unsubscription occurs through functions returned from subscription. This is so that we can safely allow subscription of any type of method: named class methods, delegates, or lambdas.

```csharp
// there is no way to safely remove this lambda without first obtaining a reference to it
MyThing.OnAdded += message => Log.Info(this, "Received!");
```

Each flavor of `Subscribe` has two different forms: one that returns an unsubscription method, and one that passes it to the callback.

```csharp
// subscribe functions return function to unsubscribe with
var unsub = router.Subscribe(...);

...

// unsubscribe
unsub();
```

Or

```csharp
// or use Subscribe variant to pass unsubscribe function to subscriber
router.Subscribe(messageType, (message, unsub) => {
	...
	
	// unsubscribe from within the callback!
	unsub();
});

router.SubscribeAll((message, messageType, unsub) => ...);
```

Subscription and unsubscription is stack-safe, meaning unsubscribe is always safe to call.

#### Consume

Sometimes it is useful to stop a message from propogating to later handlers. Since there is no required base class for messages, this is done by calling the router itself.

```csharp
router.Subscribe(messageType, message => {
	...

	// next subscriber won't get message 
	router.Consume(message);
});
```

#### Publishing

Publishing a message should be straight forward.

```csharp
// publish a message
router.Publish(messageType, message);

// publish many messages
router.Publish(messageType, messageA, messageB, messageC);
```