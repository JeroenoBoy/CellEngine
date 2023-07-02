using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;



namespace CellEngine.Utilities
{
    public struct Native2dArray<T> : IEnumerable<T>, IDisposable where T : struct
    {
        private NativeArray<T> _backingArray;
        
        public readonly int width;
        public readonly int height;

        public int  length => _backingArray.Length;
        public int2 size   => new int2(width, height);
        

        public Native2dArray(int width, int height, Allocator allocator)
        {
            this.width   = width;
            this.height  = height;
            _backingArray = new NativeArray<T>(width * height, allocator);
        }


        public Native2dArray(int2 size, Allocator allocator)
        {
            width     = size.x;
            height    = size.y;
            _backingArray = new NativeArray<T>(width * height, allocator);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIndex(int2 index) => index.x + index.y * width;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIndex(int x, int y) => x + y * width;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int2 GetIndex(int index)
        {
            int y = (int)floor(index / (float)width);
            return new int2(index - y * width, y);
        }


        public T[] ToArray() => _backingArray.ToArray();
        public IEnumerable<int2> Keys(int xOffset = 0, int yOffset = 0)
        {
            int maxX = xOffset + width;
            int maxY = yOffset + height;
            
            for (int y = yOffset; y < maxY; y++)
            for (int x = xOffset; x < maxX; x++ ) {
                yield return new int2(x, y);
            }
        }


        public void Fill(T value)
        {
            for (int i = _backingArray.Length; i --> 0;) {
                _backingArray[i] = value;
            }
        }


        public IEnumerator<T>   GetEnumerator()
        {
            return _backingArray.GetEnumerator();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public T this[int x, int y]
        {
            get => _backingArray[GetIndex(x, y)];
            set => _backingArray[GetIndex(x, y)] = value;
        }


        public T this[int2 index]
        {
            get => _backingArray[GetIndex(index)];
            set => _backingArray[GetIndex(index)] = value;
        }


        public T this[int index]
        {
            get => _backingArray[index];
            set => _backingArray[index] = value;
        }


        public void Dispose()
        {
            _backingArray.Dispose();
        }
    }
}
