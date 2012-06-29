# ServiceStack Redis Client Extensions
[Redis](http://code.google.com/p/redis/) is one of the fastest and feature-rich key-value stores to come from the [NoSQL](http://en.wikipedia.org/wiki/NoSQL) movement.
It is similar to memcached but the dataset is not volatile, and values can either be strings lists, sets, sorted sets or hashes.

[ServiceStack's C# Redis Client](https://github.com/ServiceStack/ServiceStack.Redis) is an Open Source C# Redis client based on [Miguel de Icaza](http://twitter.com/migueldeicaza) previous efforts with [redis-sharp](http://github.com/migueldeicaza/redis-sharp).
RedisExtensions adds support for Queue, Capped Collections and Stacks when you use ServiceStacks's C# Redis Client.

This Extensions are based on [QR](https://github.com/tnm/qr) which helps you create and work with queue, capped collection (bounded queue), deque, and stack data structures for Redis in Python. 

# Features

* Queue, Capped Collection and Stack structures.
* Concurrency full supported, due atomic Redis.io operations 
* Thread Safe
* Connection pooling
* Push Notification "New element on queue", when a new item has been 
added, QueueManager rises a notification through Redis.io's "Pub/Sub" to all subscripted clients. If the client is not busy, an Async 
Action will be executed with the Item as param.

# How to use it?

  // Create a Queue
    var cq1 = new QueueManager<string>(qn);
  // Clear the queue
    cq1.Flush();
  // Subscribe for New Items pushed notifications.
    cq1.SubscribeForNewItem(x => {
        Log.Debug("Incoming item cq1 Item:" + x);
    });
  // Or just pop it out on demand
  var item = cq1.Pop();

* In order to run the tests you must have a Redis Server running.
* If you already have one, please setup the URLs in app.config.
* If not, you may run services/start.bat to run an instance of redis

# More infomation about Redis?
I strongly suggest you to visit [ServiceStack's C# Redis Client](https://github.com/ServiceStack/ServiceStack.Redis) and take a look to the README file.
  
# What comes next?
* Add CappedCollectionManager and StackManager. __DONE__
* Add notification support for those structures. __DONE__
* Do some benchmark tests.
