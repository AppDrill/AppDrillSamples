using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AppDrillMobileApp.Models;
using Xamarin.Forms;

namespace AppDrillMobileApp.ViewModels
{
    [QueryProperty(nameof(ItemId), nameof(ItemId))]
    public class ItemDetailViewModel : BaseViewModel
    {
        private string itemId;
        private string serverAddress;
        private string modelName;
        public string Id { get; set; }

        public string ServerAddress
        {
            get => serverAddress;
            set => SetProperty(ref serverAddress, value);
        }

        public string ModelName
        {
            get => modelName;
            set => SetProperty(ref modelName, value);
        }

        public string ItemId
        {
            get
            {
                return itemId;
            }
            set
            {
                itemId = value;
                LoadItemId(value);
            }
        }

        public async void LoadItemId(string itemId)
        {
            try
            {
                var item = await DataStore.GetItemAsync(itemId);
                Id = item.Id;
                ServerAddress = item.ServerAddress;
                ModelName = item.ModelName;
            }
            catch (Exception)
            {
                Debug.WriteLine("Failed to Load Item");
            }
        }
    }
}
