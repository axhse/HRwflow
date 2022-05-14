using System;
using System.Collections.Generic;
using System.Threading;

namespace HRwflow.Models
{
    public sealed class AcquireCertificate<T> : IDisposable
    {
        private readonly T _item;
        private readonly Action<T> _releaseAction;
        private readonly object _syncRoot = new();
        private bool _isReleased;

        public AcquireCertificate(T item, Action<T> releaseAction)
        {
            _item = item;
            _releaseAction = releaseAction;
            _isReleased = false;
        }

        public void Dispose()
        {
            ReportRelease();
        }

        public void ReportRelease()
        {
            lock (_syncRoot)
            {
                if (!_isReleased)
                {
                    _releaseAction(_item);
                    _isReleased = true;
                }
            }
        }
    }

    public class ItemLocker<T>
    {
        private readonly Dictionary<T, Mutex> _mutexes = new();
        private readonly object _syncRoot = new();
        private readonly Dictionary<T, int> _waitersCounts = new();

        public AcquireCertificate<T> Blank => new(default, (item) => { });

        public AcquireCertificate<T> Acquire(T item)
        {
            if (item is null)
            {
                return Blank;
            }
            Mutex mutex;
            lock (_syncRoot)
            {
                if (!_mutexes.ContainsKey(item))
                {
                    _mutexes.Add(item, new Mutex(initiallyOwned: false));
                    _waitersCounts.Add(item, 0);
                }
                mutex = _mutexes[item];
                _waitersCounts[item]++;
            }
            mutex.WaitOne();
            return new(item, Release);
        }

        private void Release(T item)
        {
            if (item is null)
            {
                return;
            }
            lock (_syncRoot)
            {
                var mutex = _mutexes[item];
                _waitersCounts[item]--;
                if (_waitersCounts[item] == 0)
                {
                    _mutexes.Remove(item);
                    _waitersCounts.Remove(item);
                }
                mutex.ReleaseMutex();
            }
        }
    }
}
