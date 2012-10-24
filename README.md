# CoreSystem Library #
It contains database independent APIs to access RDBMS transparently, use of ANSI SQL in projects allows to move comfortably on different database. CoreSystem also helps to reduce boiler plate code such as opening and closing connection during database access, there are lots of functions available that can query database for you and return ADO.NET Dataset. 

It supports updates to ObservableCollection from other threads for WPF controls, so that you can update your data collections from different thread and UI still gets updated according to the changes in object properties or data collections. It is also used by Crystal Mapper to transparently interact with different RDBMS.

* Database independent APIs that reduces boiler plate code to access database
* Asynchronous updates in WPF data binding scenarios
* Thread synchronization based on built-in value types (i.e. Int32, Int64 and etc)
* Extension functions for value types and reference types

Three namespaces that worth mentioning are:

* CoreSystem.Collections: contain a DispatchedObservableCollection class that support asynchronous updates in WPF data binding scenarios
* CoreSystem.Data: has data access class APIs that hide underlining provider and allow to work with abstract interfaces, it also reduces boiler plate code for the developer
* CoreSystem.Lock: help acquiring locks on integer, long and etc ids.
* CoreSystem.Dynamic: Expandable object for arbitrary properties.
 

### What’s new in Release 1.4.0.0? ###

CoreSystem.Dynamic is contains new class to create expandable objects.


**Class library documentation will be available in each release**