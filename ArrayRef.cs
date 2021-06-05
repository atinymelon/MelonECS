using System;

namespace MelonECS
{
    public readonly struct ArrayRef<T>
    {
        public static ArrayRef<T> Empty => new ArrayRef<T>(null, 0, 0);
        
        public ref T this[int i] => ref target[i];
        public readonly int Length;
        
        private readonly T[] target;
        private readonly int startIndex;

        public ArrayRef(T[] target, int start, int count)
        {
            this.target = target;
            Length = count;
            startIndex = start;
        }

        public Enumerator GetEnumerator() => new Enumerator(target, startIndex, Length);
        
        public struct Enumerator
        {
            private readonly T[] target;
            private int index;
            private int count;

            public Enumerator(T[] target, int index, int count)
            {
                this.target = target;
                this.index = index;
                this.count = count;
            }

            public readonly ref T Current
            {
                get
                {
                    if (target is null || index < 0 || index > count)
                    {
                        throw new InvalidOperationException();
                    }
                    return ref target[index];
                }
            }

            public bool MoveNext() => ++index < count;
            public void Reset() => index = -1;
        }
    }
}