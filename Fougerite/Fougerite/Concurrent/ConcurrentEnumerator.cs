using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Fougerite.Concurrent
{
    /// <summary>
    /// A thread-safe enumerator wrapper for .NET 3.5.
    /// This class maintains a <see cref="ReaderWriterLock"/> reader lock for the entire 
    /// duration of the enumeration to prevent the collection from being modified.
    /// </summary>
    /// <typeparam name="T">The type of objects to enumerate.</typeparam>
    public class ConcurrentEnumerator<T> : IEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;
        private readonly ReaderWriterLock _lock;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentEnumerator{T}"/> class.
        /// Automatically acquires a reader lock on the provided <see cref="ReaderWriterLock"/>.
        /// </summary>
        /// <param name="inner">The collection to be enumerated.</param>
        /// <param name="lock">The lock used to synchronize access to the collection.</param>
        public ConcurrentEnumerator(IEnumerable<T> inner, ReaderWriterLock @lock)
        {
            _lock = @lock;
            // Acquires the reader lock immediately upon creation.
            // This ensures the collection remains consistent while this enumerator exists.
            _lock.AcquireReaderLock(Timeout.Infinite);
            _inner = inner.GetEnumerator();
        }

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns>true if the enumerator was successfully advanced; false if the enumerator has passed the end of the collection.</returns>
        public bool MoveNext()
        {
            return _inner.MoveNext();
        }

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        public void Reset()
        {
            _inner.Reset();
        }

        /// <summary>
        /// Releases the reader lock held by this enumerator.
        /// This must be called (typically via a using block or foreach loop) to allow writer threads to modify the collection.
        /// </summary>
        public void Dispose()
        {
            // Releases the reader lock acquired in the constructor.
            _lock.ReleaseReaderLock();
        }

        /// <summary>
        /// Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        public T Current
        {
            get { return _inner.Current; }
        }

        /// <summary>
        /// Gets the element in the collection at the current position of the enumerator.
        /// </summary>
        object IEnumerator.Current
        {
            get { return _inner.Current; }
        }
    }
}