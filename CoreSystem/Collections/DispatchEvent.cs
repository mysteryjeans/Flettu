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
using System.ComponentModel;

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
            lock (this)
            {
                handlerList.Add(new DispatchHandler(handler, dispatcher));
            }
        }

        /// <summary>
        /// Remove subscriber's handler method
        /// </summary>
        /// <param name="handler">Handler method of subscriber</param>
        public void Remove(Delegate handler)
        {
            lock (this)
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
        }

        /// <summary>
        /// Clear all handler's subscriptions
        /// </summary>
        public void Clear()
        {
            lock (this)
            {
                foreach (DispatchHandler handler in this.handlerList)
                {
                    handler.Dispose();
                }
                this.handlerList.Clear();
            }
        }

        /// <summary>
        /// Invoke of handler method one by one with associated dispatcher
        /// </summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="args">Event arguments</param>
        public void Fire(object sender, EventArgs args)
        {
            DispatchHandler[] handlers;

            lock (this)
            {
                var disposibleHandlers = (from handler in handlerList
                                          where !handler.NotDisposable
                                          select handler).ToArray();

                foreach (DispatchHandler rmvHandler in disposibleHandlers)
                {
                    this.handlerList.Remove(rmvHandler);
                    rmvHandler.Dispose();
                }

                handlers = handlerList.ToArray();
            }

            foreach (DispatchHandler handler in handlers)
                handler.Invoke(Dispatcher.CurrentDispatcher, sender, args);
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
            private static readonly TimeSpan INVOKE_TIMEOUT = TimeSpan.FromMilliseconds(3000);

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
                get { return this.Dispatcher.Thread.IsAlive && this.Dispatcher.Thread.ThreadState != ThreadState.WaitSleepJoin; }
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
                            || (target is DispatcherObject &&
                                   (dispatcher == null
                                    || dispatcher.HasShutdownStarted
                                    || dispatcher.HasShutdownFinished
                                    || (dispatcher.Thread.ThreadState & (ThreadState.Aborted
                                                                        | ThreadState.Stopped
                                                                        | ThreadState.StopRequested
                                                                        | ThreadState.AbortRequested)) != 0)));
                }
            }

            public bool NotDisposable
            {
                get
                {
                    // Obtaining strong reference
                    object target = this.Target;
                    Dispatcher dispatcher = this.Dispatcher;

                    //Checking target object(subscriber) and its thread state
                    return (target != null && (!(target is DispatcherObject) || dispatcher != null));
                }
            }

            /// <summary>
            /// Invoke handler method with associated dispatcher
            /// </summary>
            /// <param name="sender">Sender of event</param>
            /// <param name="args">Arguments</param>
            /// <remarks>
            /// Only invoke handlers method if. 
            ///     a) Subscriber is still alive (not claimed by GC)
            ///     b) Subscriber thread is alive
            /// </remarks>
            public void Invoke(Dispatcher currentDispatcher, object sender, EventArgs args)
            {
                // Obtaining strong refereces
                object target = this.Target;
                Dispatcher dispatcher = this.Dispatcher;
                
                if (currentDispatcher.Equals(dispatcher))
                {
                    // Handlers is subscribed in current thread
                    this.handlerInfo.Invoke(target, new object[] { sender, args });
                }
                else if (dispatcher.Thread.GetApartmentState() == ApartmentState.STA)
                {
                    // Handlers is subscribed in UI thread
                    // Only handles for UI controls should be called in thread own thread
                    // otherwise non-UI threads may be in lock wait state
                    dispatcher.Invoke(new Action(() => this.handlerInfo.Invoke(target, new object[] { sender, args })), null);
                }
                else
                {
                    // If thread is not marked with STA then its handlers can be called from other threads
                    this.handlerInfo.Invoke(target, new object[] { sender, args });
                }

            }

            public bool DelegateEquals(Delegate other)
            {
                object target = this.Target;
                return (target != null
                        && object.ReferenceEquals(target, other.Target)
                        && this.handlerInfo.Name == other.Method.Name);
            }

            public void Dispose()
            {
                //this.targetRef = null;
                this.handlerInfo = null;
                //this.dispatcherRef = null;
            }

        }

        #endregion
    }
}
