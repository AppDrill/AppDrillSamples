using System.ComponentModel;
using AppDrillMobileApp.ViewModels;
using Xamarin.Forms;

namespace AppDrillMobileApp.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}