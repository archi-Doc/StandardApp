// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections.Generic;

namespace Arc.CrossChannel
{
    public class Identifier_KeyMessage
    {
    }

    public class Identifier_MessageResult
    {
        public Type MessageType { get; }

        public Type ResultType { get; }

        public Identifier_MessageResult(Type messageType, Type resultType)
        {
            this.MessageType = messageType;
            this.ResultType = resultType;
        }

        public override int GetHashCode() => HashCode.Combine(this.MessageType, this.ResultType);

        public override bool Equals(object? obj)
        {
            if (obj == null || obj.GetType() != typeof(Identifier_MessageResult))
            {
                return false;
            }

            var x = (Identifier_MessageResult)obj;
            return this.MessageType == x.MessageType && this.ResultType == x.ResultType;
        }
    }

    public class Identifier_KeyMessage<TKey> : Identifier_KeyMessage
        where TKey : notnull
    {
        public TKey Key { get; }

        public Type MessageType { get; }

        public Identifier_KeyMessage(TKey key, Type messageType)
        {
            this.Key = key;
            this.MessageType = messageType;
        }

        public override int GetHashCode() => HashCode.Combine(this.Key, this.MessageType);

        public override bool Equals(object? obj)
        {
            if (obj == null || obj.GetType() != typeof(Identifier_KeyMessage<TKey>))
            {
                return false;
            }

            var x = (Identifier_KeyMessage<TKey>)obj;
            return EqualityComparer<TKey>.Default.Equals(this.Key, x.Key) && this.MessageType == x.MessageType;
        }
    }

    public class Identifier_KeyMessageResult
    {
    }

    public class Identifier_KeyMessageResult<TKey> : Identifier_KeyMessageResult
        where TKey : notnull
    {
        public TKey Key { get; }

        public Type MessageType { get; }

        public Type ResultType { get; }

        public Identifier_KeyMessageResult(TKey key, Type messageType, Type resultType)
        {
            this.Key = key;
            this.MessageType = messageType;
            this.ResultType = resultType;
        }

        public override int GetHashCode() => HashCode.Combine(this.Key, this.MessageType, this.ResultType);

        public override bool Equals(object? obj)
        {
            if (obj == null || obj.GetType() != typeof(Identifier_KeyMessageResult<TKey>))
            {
                return false;
            }

            var x = (Identifier_KeyMessageResult<TKey>)obj;
            return EqualityComparer<TKey>.Default.Equals(this.Key, x.Key) && this.MessageType == x.MessageType && this.ResultType == x.ResultType;
        }
    }
}
