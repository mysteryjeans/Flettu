# CoreSystem Library #

It contains many small pieces of codes for reducing preliminary coding overheads, whether you want to transparently interact with database or synchronize threads base on user ids or store password safely or reduce boiler plate code such as opening and closing connection; CoreSystem Lib has something for you.
Features

* Database independent APIs that reduces boiler plate code to access database
* Asynchronous updates in WPF data binding scenarios
* Thread synchronization based on built-in value types (i.e. Int32, Int64, string and etc)
* Extension functions for value types and reference types
* Password encryption and authentication. 

### Namespaces ###

* CoreSystem.Collections: contain a DispatchedObservableCollection class that support asynchronous updates in WPF data binding scenarios
* CoreSystem.Data: has data access class APIs that hide underlining provider and allow to work with abstract interfaces, it also reduces boiler plate code for the developer
* CoreSystem.Lock: help acquiring locks on integer, long and etc ids.
* CoreSystem.Dynamic: Expandable object for arbitrary properties.
* CoreSystem.Crypto: Store password safely as random hash value to prevent against brute force attacks, or encrypt sensitive data quickly. 

### What’s new in Release 1.9.0.0? ###

* Cipher class modified to use random salt for each encryption
* Enum.In extension function to match with multiple enum values
* CheckNullOrEmpty function in Guard class to checking array parameters. 

### What’s new in Release 1.5.0.0? ###

* SyncString for locking on string values
* GunZip for compression and decompression from .gz  

### What’s new in Release 1.4.0.0? ###

* CoreSystem.Dynamic is contains new class to create expandable objects.

Copyright (c) Faraz Masood Khan <mk.faraz@gmail.com>
