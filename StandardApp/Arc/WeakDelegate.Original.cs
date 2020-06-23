// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

#pragma warning disable SA1649 // File name should match first type name

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Arc.WeakDelegate.Original
{
    public class WeakAction : WeakDelegate
    {
        public WeakAction(Action action, bool keepTargetAlive = false)
            : this(action.Target, action, keepTargetAlive)
        {
        }

        public WeakAction(object? target, Action action, bool keepTargetAlive = false)
            : base(target, action, keepTargetAlive)
        {
        }

        public void Execute()
        {
            if (this.StaticDelegate is Action action)
            {
                action();
                return;
            }

            var actionTarget = this.ActionTarget;

            if (this.IsAlive && actionTarget != null)
            {
                this.Method?.Invoke(actionTarget, null);
                return;
            }
        }
    }

    public class WeakAction<T> : WeakDelegate
    {
        public WeakAction(Action<T> action, bool keepTargetAlive = false)
            : this(action.Target, action, keepTargetAlive)
        {
        }

        public WeakAction(object? target, Action<T> action, bool keepTargetAlive = false)
            : base(target, action, keepTargetAlive)
        {
        }

        public void Execute(T t)
        {
            if (this.StaticDelegate is Action<T> action)
            {
                action(t);
                return;
            }

            var actionTarget = this.ActionTarget;

            if (this.IsAlive && actionTarget != null)
            {
                this.Method?.Invoke(actionTarget, new object?[] { t });
                return;
            }
        }
    }

    public class WeakFunc<TResult> : WeakDelegate
    {
        public WeakFunc(Func<TResult> function, bool keepTargetAlive = false)
            : this(function.Target, function, keepTargetAlive)
        {
        }

        public WeakFunc(object? target, Func<TResult> function, bool keepTargetAlive = false)
            : base(target, function, keepTargetAlive)
        {
        }

        [return: MaybeNull]
        public TResult Execute()
        {
            if (this.StaticDelegate is Func<TResult> function)
            {
                return function();
            }

            var actionTarget = this.ActionTarget;

            if (this.IsAlive && actionTarget != null)
            {
                return (TResult)this.Method?.Invoke(actionTarget, null);
            }

            return default(TResult);
        }
    }

    public class WeakFunc<T, TResult> : WeakDelegate
    {
        public WeakFunc(Func<T, TResult> function, bool keepTargetAlive = false)
            : this(function.Target, function, keepTargetAlive)
        {
        }

        public WeakFunc(object? target, Func<T, TResult> function, bool keepTargetAlive = false)
            : base(target, function, keepTargetAlive)
        {
        }

        [return: MaybeNull]
        public TResult Execute(T t)
        {
            if (this.StaticDelegate is Func<T, TResult> function)
            {
                return function(t);
            }

            var actionTarget = this.ActionTarget;

            if (this.IsAlive && actionTarget != null)
            {
                return (TResult)this.Method?.Invoke(actionTarget, new object?[] { t });
            }

            return default(TResult);
        }
    }

    public class WeakDelegate : Arc.WeakDelegate.IWeakDelegate
    {
        public WeakDelegate(Delegate @delegate, bool keepTargetAlive = false)
            : this(@delegate.Target, @delegate, keepTargetAlive)
        {
        }

        public WeakDelegate(object? target, Delegate @delegate, bool keepTargetAlive = false)
        {
#if NETFX_CORE
            if (@delegate.GetMethodInfo().IsStatic)
#else
            if (@delegate.Method.IsStatic)
#endif
            {
                this.StaticDelegate = @delegate;

                if (target != null)
                {
                    // Keep a reference to the target to control the WeakAction's lifetime.
                    this.Reference = new WeakReference(target);
                }

                return;
            }

#if NETFX_CORE
            this.Method = @delegate.GetMethodInfo();
#else
            this.Method = @delegate.Method;
#endif

            this.ActionReference = new WeakReference(@delegate.Target);
            this.LiveReference = keepTargetAlive ? @delegate.Target : null;
            this.Reference = new WeakReference(target);
        }

        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        public string MethodName
        {
            get
            {
                if (this.StaticDelegate != null)
                {
#if NETFX_CORE
                    return this.StaticDelegate.GetMethodInfo().Name;
#else
                    return this.StaticDelegate.Method.Name;
#endif
                }

                if (this.Method != null)
                {
                    return this.Method.Name;
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the Delegate's owner. This object is stored as a <see cref="WeakReference" />.
        /// </summary>
        public object? Target => this.Reference?.Target;

        /// <summary>
        /// Gets a value indicating whether the Action's owner is still alive.
        /// </summary>
        public bool IsAlive
        {
            get
            {
                if (this.LiveReference != null)
                {
                    return true;
                }

                if (this.Reference != null)
                {
                    return this.Reference.IsAlive;
                }

                if (this.StaticDelegate != null)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the WeakDelegate is static or not.
        /// </summary>
        public bool IsStatic => this.StaticDelegate != null;

        /// <summary>
        /// Gets the target of the weak reference.
        /// </summary>
        protected object? ActionTarget
        {
            get
            {
                if (this.LiveReference != null)
                {
                    return this.LiveReference;
                }

                return this.ActionReference?.Target;
            }
        }

        protected Delegate? StaticDelegate { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MethodInfo" /> corresponding to this WeakDelegate's method passed in the constructor.
        /// </summary>
        protected MethodInfo? Method { get; set; }

        /// <summary>
        /// Gets or sets a WeakReference to this action's target.
        /// </summary>
        protected WeakReference? ActionReference { get; set; }

        /// <summary>
        /// Gets or sets a WeakReference to the target passed when constructing the WeakDelegate.
        /// </summary>
        protected WeakReference? Reference { get; set; }

        /// <summary>
        /// Gets or sets a hard reference.
        /// </summary>
        protected object? LiveReference { get; set; }

        /// <summary>
        /// Sets the reference that this instance stores to null.
        /// </summary>
        public void MarkForDeletion()
        {
            this.Reference = null;
            this.ActionReference = null;
            this.LiveReference = null;
            this.Method = null;
            this.StaticDelegate = null;
        }
    }
}
