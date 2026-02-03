using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TrailMeisterUtilities
{
    public sealed class AppDisposables
    {
        private readonly List<IAppDisposable> _disposables = new();

        public static AppDisposables Instance { get; } = new();

        private AppDisposables() { }

        public void Register(IAppDisposable obj)
        {
            if (!_disposables.Contains(obj))
                _disposables.Add(obj);
        }

        public void DisposeAll()
        {
            // Dispose in reverse order of registration (safest)
            for (int i = _disposables.Count - 1; i >= 0; i--)
            {
                try
                {
                    _disposables[i].Dispose();
                }
                catch (Exception ex)
                {
                    // Log but keep disposing others
                    Debug.WriteLine($"Failed disposing {_disposables[i].Name}: {ex}");
                }
            }
            _disposables.Clear();
        }
    }

}
