// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace MoonscraperEngine
{
    public class EnumLookupTable<EnumType, Value> : IList<Value> where EnumType : Enum
    {
        Value[] table = new Value[EnumX<EnumType>.Count];

        public Value this[int index] { get => table[index]; set => table[index] = value; }
        public Value this[EnumType index] { get => table[EnumX<EnumType>.ToInt(index)]; set => table[EnumX<EnumType>.ToInt(index)] = value; }

        public int Count => table.Length;

        public bool IsReadOnly => table.IsReadOnly;

        public void Add(Value item)
        {
            ((IList<Value>)table).Add(item);
        }

        public void Clear()
        {
            ((IList<Value>)table).Clear();
        }

        public bool Contains(Value item)
        {
            return ((IList<Value>)table).Contains(item);
        }

        public void CopyTo(Value[] array, int arrayIndex)
        {
            ((IList<Value>)table).CopyTo(array, arrayIndex);
        }

        public IEnumerator<Value> GetEnumerator()
        {
            return ((IList<Value>)table).GetEnumerator();
        }

        public int IndexOf(Value item)
        {
            return ((IList<Value>)table).IndexOf(item);
        }

        public void Insert(int index, Value item)
        {
            ((IList<Value>)table).Insert(index, item);
        }

        public bool Remove(Value item)
        {
            return ((IList<Value>)table).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<Value>)table).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return table.GetEnumerator();
        }
    }
}
