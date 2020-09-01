using Prism.Mvvm;
using SavescumBuddy.Sqlite;
using System;

namespace SavescumBuddy.ViewModels
{
    public abstract class BaseViewModel : BindableBase
    {
        public BaseViewModel(IDataAccess dbAccess)
        {
            SqliteDataAccess = dbAccess ?? throw new ArgumentNullException(nameof(dbAccess));
        }

        public IDataAccess SqliteDataAccess { get; }
    }
}
