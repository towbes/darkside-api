using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;



//Not currently using this viewmodelbase
//Mvvm references: https://stackoverflow.com/questions/56952535/how-to-save-wpf-textbox-value-to-variable


namespace DarkSideModernGUI.Helpers
{


    public class ViewModelBase
    {
        [DllImport("darkside-api.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void HelloWorld();
 
        private ICommand _saveCommand;
        private ICommand _injectCommand;

        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand(
                        param => this.SaveObject(),
                        param => this.CanSave()
                    );
                }
                return _saveCommand;
            }
        }

        private bool CanSave()
        {
            return true;// Verify command can be executed here
        }

        private void SaveObject()
        {
            HelloWorld();
        }

        public ICommand InjectCommand (int pid)
        {
            {
                if (_injectCommand == null)
                {
                    _injectCommand = new RelayCommand(
                        param => this.InjectPid(pid),
                        param => this.CanInject(pid));
                }
            }
            return _injectCommand;
        }

        private bool CanInject(int pid)
        {
            return true;
        }

        private void InjectPid(int pid) {
            MessageBox.Show(String.Format("Injecting pid {0}",pid));
        }
    }
}
