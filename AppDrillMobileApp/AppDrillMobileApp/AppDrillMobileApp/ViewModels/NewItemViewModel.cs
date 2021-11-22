using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using AppDrillMobileApp.Models;
using Xamarin.Forms;

using AppDrillCore.Session;

namespace AppDrillMobileApp.ViewModels
{
    public class NewItemViewModel : BaseViewModel
    {
        private string serverAddress;

        public NewItemViewModel()
        {
            ConnectCommand = new Command(OnConnect, ValidateConnect);
            CancelCommand = new Command(OnCancel);
            this.PropertyChanged +=
                (_, __) => ConnectCommand.ChangeCanExecute();
        }

        private bool ValidateConnect()
        {
            return !String.IsNullOrWhiteSpace(serverAddress);
        }

        public string ServerAddress
        {
            get => serverAddress;
            set => SetProperty(ref serverAddress, value);
        }

        public Command ConnectCommand { get; }
        public Command CancelCommand { get; }

        private async void OnCancel()
        {
            // This will pop the current page off the navigation stack
            await Shell.Current.GoToAsync("..");
        }

        private async void OnConnect()
        {
            var models = SessionManager.Instance.GetModelNames(ServerAddress);
            foreach (var model in models)
            {
                Item newItem = new Item()
                {
                    Id = Guid.NewGuid().ToString(),
                    ServerAddress = ServerAddress,
                    ModelName = model
                };

                await DataStore.AddItemAsync(newItem);
            }

            // This will pop the current page off the navigation stack
            await Shell.Current.GoToAsync("..");
        }
    }
}
