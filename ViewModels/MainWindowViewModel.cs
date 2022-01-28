using NoisePower.Views;

namespace NoisePower.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        MainWindow mainWindow = new MainWindow();

        public string IsVACInstalled => mainWindow.InstalledVACText;

        public bool VACInstallBool => mainWindow.IsVACInstalled;
    }
}
