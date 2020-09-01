using SavescumBuddy.Sqlite;

namespace SavescumBuddy.ViewModels
{
    class AboutViewModel : BaseViewModel
    {
        public AboutViewModel(IDataAccess dataAccess) : base(dataAccess) { }

        public string Version => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }
}
