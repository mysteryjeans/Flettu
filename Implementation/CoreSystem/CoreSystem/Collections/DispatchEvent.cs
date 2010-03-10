/*
 * Author: Faraz Masood Khan 
 * 
 * Date: 3/9/2008
 * 
 * Class: DispatchEvent, DispatchHandler
 * 
 * Copyright: Faraz Masood Khan @ Copyright ©  2008
 * 
 * Email: mk.faraz@gmail.com
 * 
 * Blogs: http://csharplive.wordpress.com, http://farazmasoodkhan.wordpress.com
 * 
 */

using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Threading;
using System.Reflection;
using System.Windows;
using System.Collections;

namespace CoreSystem.Collections
{
    /// <summary>
    /// This class is alternative to .NET builtin events.
    /// It stores subscriber's handler methods with repective dispatchers.
    /// </summary>
    /// <remarks>
    /// It helps .NET windows forms and WPF controls to accept events from
    /// other threads asynchronously
    /// </remarks>
    /// <see cref="DispatcherObject"/>
    public class DispatchEvent
    {
        #region Data Member

        private List<DispatchHandler> handlerList = new List<DispatchHandler>();

        #endregion

        #region Expose Methods

        /// <summary>
        /// Add a new subscriber's handler method with its current dispatcher
        /// </summary>
        /// <param name="handler">Handler method of subscriber</param>
        public void Add(Delegate handler)
        {
            this.Add(handler, Dispatcher.CurrentDispatcher);
        }

        /// <summary>
        /// Add a new subscriber's handler method with dispatcher 
        /// </summary>
        /// <param name="handler">Handler method of subscriber</param>
        /// <param name="dispatcher">Dispatcher in which handler method should be called</param>
        public void Add(Delegate handler, Dispatcher dispatcher)
        {
            handlerList.Add(new DispatchHandler(handler, dispatcher));
        }

        /// <summary>
        /// Remove subscriber's handler method
        /// </summary>
        /// <param name="handler">Handler method of subscriber</param>
        public void Remove(Delegate handler)
        {
            var rmvHandlers = (from dispatchHandler in handlerList
                               where dispatchHandler.DelegateEquals(handler)
                               select dispatchHandler).ToArray();

            if (rmvHandlers != null && rmvHandlers.Length > 0)
            {
                this.handlerList.Remove(rmvHandlers[0]);
                rmvHandlers[0].Dispose();
            }
        }

        /// <summary>
        /// Clear all handler's subscriptions
        /// </summary>
        public void Clear()
        {
            foreach (DispatchHandler handler in this.handlerList)
            {
                handler.Dispose();
            }
            this.handlerList.Clear();
        }

        /// <summary>
        /// Invoke of handler method one by one with associated dispatcher
        /// </summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="args">Event arguments</param>
        public void Fire(object sender, EventArgs args)
        {
            var disposableHandler = from handler in handlerList
                                    where handler.IsDisposable
                                    select handler;
            foreach (DispatchHandler rmvHandler in disposableHandler.ToArray())
            {
                this.handlerList.Remove(rmvHandler);
                rmvHandler.Dispose();
            }

            foreach (DispatchHandler handler in handlerList)
                handler.Invoke(sender, args);            
        }

        #endregion

        #region DispatchHandler Class

        /// <summary>
        /// Internal class to hold handler with associated dispatcher
        /// </summary>
        /// <remarks>
        /// This class only holds weak reference so that it subscriber
        /// could be claim by garbage collector
        /// </remarks>
        private class DispatchHandler : IDisposable
        {
            private MethodInfo handlerInfo;
            private WeakReference targetRef;
            private WeakReference dispatcherRef;

            public DispatchHandler(Delegate handler, Dispatcher dispatcher)
            {
                this.handlerInfo = handler.Method;              
                this.targetRef = new WeakReference(handler.Target);
                this.dispatcherRef = new WeakReference(dispatcher);                
            }

            private Dispatcher Dispatcher
            {
                get { return (Dispatcher)this.dispatcherRef.Target; }
            }

            private object Target
            {
                get { return this.targetRef.Target; }
            }
            
            private bool IsDispatcherThreadAlive
            {
                get { return this.Dispatcher.Thread.IsAlive; }
            }

            public bool IsDisposable
            {
                get
                {
                    // Obtaining strong reference
                    object target = this.Target;                    
                    Dispatcher dispatcher = this.Dispatcher;

                    //Checking target object(subscriber) and its thread state
                    return (target == null
                            || dispatcher == null
                            || (target is DispatcherObject &&
                               (dispatcher.Thread.ThreadState & (ThreadState.Aborted
                                                                | ThreadState.Stopped
                                                                | ThreadState.StopRequested
                                                                | ThreadState.AbortRequested)) != 0 ));
                }
            }

            /// <summary>
            /// Invoke handler method with associated dispatcher
            /// </summary>
            /// <param name="arg">Sender of event</param>
            /// <param name="args">Arguments</param>
            /// <remarks>
            /// Only invoke handlers method if. 
            ///     a) Subscriber is still alive (not claimed by GC)
            ///     b) Subscriber thread is alive
            /// </remarks>
            public void Invoke(object arg, params object[] args)
            {
                // Obtaining strong refereces
                object target = this.Target;
                Dispatcher dispatcher = this.Dispatcher;              
                
                // Invoking if it is still alive
                if (!this.IsDisposable)
                {
                    // Invoking handler in the same thread from which it was registered
                    if (this.IsDispatcherThreadAlive)
                    {
                        dispatcher.Invoke(DispatcherPriority.Send, new EventHandler(
                                                                                    delegate(object sender, EventArgs e)
                                                                                    {
                                                                                        this.handlerInfo.Invoke(target, new object[] { arg, e });
                                                                                    }), arg, args);
                    }
                    // if subscriber is derive from DispatcherObject class
                    // than leaving method call to be executed when thread got alive again
                    else if (target is DispatcherObject)
                    {
                        dispatcher.BeginInvoke(DispatcherPriority.Send, new EventHandler(
                                                                                        delegate(object sender, EventArgs e)
                                                                                        {
                                                                                            this.handlerInfo.Invoke(target, new object[] { arg, e });
                                                                                        }), arg, args);
                    }
                    // if subscriber is not derive from DispatcherObject class
                    // than invoking method(handler) in current thread (The thread that reaise this event)
                    else
                    {
                        ArrayList paramList = new ArrayList();
                        paramList.Add(arg);
                        paramList.AddRange(args);
                        this.handlerInfo.Invoke(target, paramList.ToArray());
                    }

                }
            }           

            public bool DelegateEquals(Delegate other)
            {
                object target = this.Target;
                return (target != null 
                        && object.ReferenceEquals(target, other.Target) 
                        &&  this.handlerInfo.Name == other.Method.Name);
            }          

            public void Dispose()
            {
                this.targetRef = null;
                this.handlerInfo = null;                
                this.dispatcherRef = null;                    
            }

        }

        #endregion     

    }
}
