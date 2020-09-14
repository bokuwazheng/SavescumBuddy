using Prism.Events;
using Prism.Mvvm;
using SavescumBuddy.Core.Events;
using SavescumBuddy.Data;
using SavescumBuddy.Services.Interfaces;
using System;
using System.Collections.Generic;

namespace SavescumBuddy.Modules.List.ViewModels
{
    public class ListViewModel : BindableBase
    {
        private IEventAggregator _eventAggregator;
        private IDataAccess _dataAccess;
        private Backup _selectedBackup;
        private List<Backup> _backups;

        public ListViewModel(IEventAggregator eventAggregator, IDataAccess dataAccess)
        {
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _dataAccess = dataAccess ?? throw new ArgumentNullException(nameof(dataAccess));

            _eventAggregator.GetEvent<BackupListSortedEvent>().Subscribe(OnBackupFilterChanged);
            _eventAggregator.GetEvent<NextPageRequestedEvent>().Subscribe(OnBackupFilterChanged);
            _eventAggregator.GetEvent<PreviousPageRequestedEvent>().Subscribe(OnBackupFilterChanged);
            _eventAggregator.GetEvent<LastPageRequestedEvent>().Subscribe(OnBackupFilterChanged);
            _eventAggregator.GetEvent<FirstPageRequestedEvent>().Subscribe(OnBackupFilterChanged);
        }

        public Backup SelectedBackup { get => _selectedBackup; set => SetProperty(ref _selectedBackup, value, () => OnSelectedBackupChanged(value)); }
        public List<Backup> Backups { get => _backups ??= new List<Backup>(); set => SetProperty(ref _backups, value, () => OnBackupListChanged()); }

        private void OnSelectedBackupChanged(Backup backup)
        {
            _eventAggregator.GetEvent<SelectedBackupChangedEvent>().Publish(backup);
        }

        private void OnBackupListChanged()
        {
            _eventAggregator.GetEvent<BackupListChangedEvent>().Publish();
        }

        private void OnBackupFilterChanged(BackupSearchRequest request)
        {
            Backups = _dataAccess.SearchBackups(request);
        }
    }
}
