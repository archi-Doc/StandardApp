// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

#pragma warning disable SA1649 // File name should match first type name

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Arc.WeakDelegate.Original
{
    /// <summary>
    /// Stores a delegate without causing a hard reference to be created. The owner can be garbage collected at any time.
    /// </summary>
    public interface IWeakDelegate
    {
        /// <summary>
        /// Gets the Delegate's owner. This object is stored as a <see cref="WeakReference" />.
        /// </summary>
        object? Target { get; }

        /// <summary>
        /// Gets a value indicating whether the Delegate's owner is still alive.
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// Deletes all references, which notifies the cleanup method that this entry must be deleted.
        /// </summary>
        void MarkForDeletion();
    }

    public class WeakAction : WeakDelegate
    {
        public WeakAction(Action method, bool keepTargetAlive = false)
            : this(method.Target, method, keepTargetAlive)
        {
        }

        public WeakAction(object? target, Action method, bool keepTargetAlive = false)
            : base(target, method, keepTargetAlive)
        {
        }

        public void Execute(out bool executed)
        {
            if (this.StaticDelegate is Action method)
            {
                executed = true;
                method();
                return;
            }

            var delegateTarget = this.DelegateTarget;
            if (this.IsAlive && delegateTarget != null)
            {
                executed = true;
                this.Method?.Invoke(delegateTarget, null);
                return;
            }

            executed = false;
            return;
        }

        public void Execute()
        {
            if (this.StaticDelegate is Action method)
            {
                method();
                return;
            }

            var delegateTarget = this.DelegateTarget;
            if (this.IsAlive && delegateTarget != null)
            {
                this.Method?.Invoke(delegateTarget, null);
            }

            return;
        }
    }

    public class WeakAction<T> : WeakDelegate
    {
        public WeakAction(Action<T> method, bool keepTargetAlive = false)
            : this(method.Target, method, keepTargetAlive)
        {
        }

        public WeakAction(object? target, Action<T> method, bool keepTargetAlive = false)
            : base(target, method, keepTargetAlive)
        {
        }

        public void Execute(T t, out bool executed)
        {
            if (this.StaticDelegate is Action<T> method)
            {
                executed = true;
                method(t);
                return;
            }

            var delegateTarget = this.DelegateTarget;
            if (this.IsAlive && delegateTarget != null)
            {
                executed = true;
                this.Method?.Invoke(delegateTarget, new object?[] { t });
                return;
            }

            executed = false;
            return;
        }

        public void Execute(T t)
        {
            if (this.StaticDelegate is Action<T> method)
            {
                method(t);
                return;
            }

            var delegateTarget = this.DelegateTarget;
            if (this.IsAlive && delegateTarget != null)
            {
                this.Method?.Invoke(delegateTarget, new object?[] { t });
            }

            return;
        }
    }

    public class WeakFunc<TResult> : WeakDelegate
    {
        public WeakFunc(Func<TResult> method, bool keepTargetAlive = false)
            : this(method.Target, method, keepTargetAlive)
        {
        }

        public WeakFunc(object? target, Func<TResult> method, bool keepTargetAlive = false)
            : base(target, method, keepTargetAlive)
        {
        }

        [return: MaybeNull]
        public TResult Execute(out bool executed)
        {
            if (this.StaticDelegate is Func<TResult> method)
            {
                executed = true;
                return method();
            }

            var delegateTarget = this.DelegateTarget;
            if (this.IsAlive && delegateTarget != null)
            {
                executed = true;
                return (TResult)this.Method?.Invoke(delegateTarget, null);
            }

            executed = false;
            return default;
        }

        [return: MaybeNull]
        public TResult Execute()
        {
            if (this.StaticDelegate is Func<TResult> method)
            {
                return method();
            }

            var delegateTarget = this.DelegateTarget;
            if (this.IsAlive && delegateTarget != null)
            {
                return (TResult)this.Method?.Invoke(delegateTarget, null);
            }

            return default;
        }
    }

    public class WeakFunc<T, TResult> : WeakDelegate
    {
        public WeakFunc(Func<T, TResult> method, bool keepTargetAlive = false)
            : this(method.Target, method, keepTargetAlive)
        {
        }

        public WeakFunc(object? target, Func<T, TResult> method, bool keepTargetAlive = false)
            : base(target, method, keepTargetAlive)
        {
        }

        [return: MaybeNull]
        public TResult Execute(T t, out bool executed)
        {
            if (this.StaticDelegate is Func<T, TResult> method)
            {
                executed = true;
                return method(t);
            }

            var delegateTarget = this.DelegateTarget;
            if (this.IsAlive && delegateTarget != null)
            {
                executed = true;
                return (TResult)this.Method?.Invoke(delegateTarget, new object?[] { t });
            }

            executed = false;
            return default;
        }

        [return: MaybeNull]
        public TResult Execute(T t)
        {
            if (this.StaticDelegate is Func<T, TResult> method)
            {
                return method(t);
            }

            var delegateTarget = this.DelegateTarget;
            if (this.IsAlive && delegateTarget != null)
            {
                return (TResult)this.Method?.Invoke(delegateTarget, new object?[] { t });
            }

            return default;
        }
    }

    public class WeakDelegate : IWeakDelegate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WeakDelegate"/> class.
        /// </summary>
        /// <param name="delegate">The action that will be associated to this instance.</param>
        /// <param name="keepTargetAlive">If true, the target of the Action will be kept as a hard reference, which might cause a memory leak.</param>
        public WeakDelegate(Delegate @delegate, bool keepTargetAlive = false)
            : this(@delegate.Target, @delegate, keepTargetAlive)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WeakDelegate"/> class.
        /// </summary>
        /// <param name="target">The action's owner.</param>
        /// <param name="delegate">The action that will be associated to this instance.</param>
        /// <param name="keepTargetAlive">If true, the target of the Action will be kept as a hard reference, which might cause a memory leak.</param>
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

            this.DelegateReference = new WeakReference(@delegate.Target);
            this.HardReference = keepTargetAlive ? @delegate.Target : null;
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

        public object? Target => this.Reference?.Target;

        public bool IsAlive
        {
            get
            {
                if (this.HardReference != null)
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
        /// Gets the target of this delegate.
        /// </summary>
        protected object? DelegateTarget
        {
            get
            {
                if (this.HardReference != null)
                {
                    return this.HardReference;
                }

                return this.DelegateReference?.Target;
            }
        }

        /// <summary>
        /// Gets or sets a hard reference of this delegate. This property is used only when the delegate is static.
        /// </summary>
        protected Delegate? StaticDelegate { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="MethodInfo" /> corresponding to this WeakDelegate's method passed in the constructor.
        /// </summary>
        protected MethodInfo? Method { get; set; }

        /// <summary>
        /// Gets or sets a WeakReference to this delegate's target (new WeakReference(@delegate.Target)).
        /// </summary>
        protected WeakReference? DelegateReference { get; set; }

        /// <summary>
        /// Gets or sets a WeakReference to the target passed when constructing the WeakDelegate (new WeakReference(target)).
        /// </summary>
        protected WeakReference? Reference { get; set; }

        /// <summary>
        /// Gets or sets a hard reference to this delegate's target (keepTargetAlive ? @delegate.Target : null).
        /// </summary>
        protected object? HardReference { get; set; }

        /// <summary>
        /// Sets the reference that this instance stores to null.
        /// </summary>
        public void MarkForDeletion()
        {
            this.Reference = null;
            this.DelegateReference = null;
            this.HardReference = null;
            this.Method = null;
            this.StaticDelegate = null;
        }
    }
}
