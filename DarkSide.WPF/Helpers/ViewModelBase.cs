using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

//Not currently using this viewmodelbase
//Mvvm references: https://stackoverflow.com/questions/56952535/how-to-save-wpf-textbox-value-to-variable

namespace DarkSideModernGUI.Helpers;

public class ViewModelBase
{
    private ICommand _injectCommand;

    private ICommand _saveCommand;

    public ICommand SaveCommand
    {
        get
        {
            if (_saveCommand == null)
            {
                _saveCommand = new RelayCommand(
                    param => SaveObject(),
                    param => CanSave()
                );
            }

            return _saveCommand;
        }
    }

    [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void HelloWorld();

    private bool CanSave()
    {
        return true; // Verify command can be executed here
    }

    private void SaveObject()
    {
        HelloWorld();
    }

    public ICommand InjectCommand(int pid)
    {
        {
            if (_injectCommand == null)
            {
                _injectCommand = new RelayCommand(
                    param => InjectPid(pid),
                    param => CanInject(pid));
            }
        }

        return _injectCommand;
    }

    private bool CanInject(int pid)
    {
        return true;
    }

    private void InjectPid(int pid)
    {
        MessageBox.Show(string.Format("Injecting pid {0}", pid));
    }
}