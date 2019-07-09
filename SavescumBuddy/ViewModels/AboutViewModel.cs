namespace SavescumBuddy.ViewModels
{
    class AboutViewModel : BaseViewModel
    {
        public string Version => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }
}
