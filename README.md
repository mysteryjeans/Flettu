# CoreSystem Library #
It helps to reduce boiler plate codes such as opening and closing connection during database access through many overloaded functions, an independent RDBMS APIs to access database transparently, use of ANSI SQL in projects allows to move comfortably on different database with just small change of connection string in config. There are other classes as well for thread synchronization based on value types, asynchronous updates to ObservableCollection in WPF and much more. Main objective of this library to put together frequently use piece of codes may be used in very specific senarios.

* Database independent APIs that reduces boiler plate code to access database
* Asynchronous updates in WPF data binding scenarios
* Thread synchronization based on built-in value types (i.e. Int32, Int64 and etc)
* Extension functions for value types and reference types

### Some namespaces ###

* CoreSystem.Collections: contain a DispatchedObservableCollection class that support asynchronous updates in WPF data binding scenarios
* CoreSystem.Data: has data access class APIs that hide underlining provider and allow to work with abstract interfaces, it also reduces boiler plate code for the developer
* CoreSystem.Lock: help acquiring locks on integer, long and etc ids.
* CoreSystem.Dynamic: Expandable object for arbitrary properties.
 

### What’s new in Release 1.4.0.0? ###

CoreSystem.Dynamic is contains new class to create expandable objects.


### Getting Involve ###
Its not a trick library or contains very trivial code, just help us put together what you used most in one place.


**Class library documentation will be available in each release**
