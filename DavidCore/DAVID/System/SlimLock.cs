using System.ComponentModel;
using System.Runtime.CompilerServices;
using DAVID.CodeAnnotations;

namespace DAVID.Synchronization
{
    /// <summary>
    /// Alternativa k jednoduchým zámkům <see langword="lock"/>. Po vypršení timeout končí chybou. Nepodporuje rekurzi.
    /// </summary>
    //Převzato z ASOL.Framework.Shared.Threading.SlimLock </remarks>
    [Immutable]
    public sealed class SlimLock
    {
        public readonly string? Name;
        public readonly int Timeout;
        private readonly object _monitor = new();
        const int _INFINITE_TIMEOUT = System.Threading.Timeout.Infinite;
        const int _DEFAULT_TIMEOUT = 10_002;
        public SlimLock(string? name = null, int timeout = _DEFAULT_TIMEOUT)
        {
            Name = name;
            Timeout = timeout > _INFINITE_TIMEOUT ? timeout : _INFINITE_TIMEOUT;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IDisposable Lock()
        {
            if (Monitor.IsEntered(_monitor)) throw new LockRecursionException($"{nameof(SlimLock)} lock recursion occured ({Name})");
            if (!Monitor.TryEnter(_monitor,Timeout)) throw new TimeoutException($"{nameof(SlimLock)} lock timeout ({Name}) elapsed ({Timeout} ms)");
            return new _SlimLockToken(_monitor);
        }
        private struct _SlimLockToken : IDisposable
        {
            object? _monitor;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal _SlimLockToken(object monitor)
            {
                _monitor = monitor;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                if (_monitor is not null)
                {
                    Monitor.Exit(_monitor);
                    _monitor = null;
                }
            }
        }
    }

}