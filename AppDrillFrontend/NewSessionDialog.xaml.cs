/*
 * Copyright (C) 2021 AppDrill, https://appdrill.io
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *          http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using AppDrillCore.Session;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AppDrillFrontend
{
    /// <summary>
    /// Interaction logic for NewSessionDialog.xaml
    /// </summary>
    public partial class NewSessionDialog : Window
    {
        private MainWindow _mainWindow;

        public NewSessionDialog(MainWindow mainWindow)
        {
            InitializeComponent();

            _mainWindow = mainWindow;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var clientSettings = ClientSettingsManager.Instance.GetSettings();
            serverAddressTextBox.Text = clientSettings.ServerAddress;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if ((serverAddressTextBox.Text.Length > 0) && (modelNamesComboBox.Text.Length > 0))
            {
                StartNewSession();
            }
            else
            {
                MessageBox.Show("Invalid input", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartNewSession()
        {
            if (_mainWindow.StartNewSession(serverAddressTextBox.Text, modelNamesComboBox.Text))
            {
                Close();
            }
            else
            {
                MessageBox.Show("Unable to start a session", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (serverAddressTextBox.Text.Length > 0)
            {
                var connectButton = sender as Button;

                connectButton.Content = "Connecting";
                connectButton.IsEnabled = false;

                try
                {
                    var serverAddress = serverAddressTextBox.Text;
                    var modelNames = await Task.Run(() => _mainWindow.GetModelNames(serverAddress));

                    if (modelNames != null)
                    {
                        modelNames.Sort();
                        modelNamesComboBox.ItemsSource = modelNames;

                        if (modelNames.Count > 0)
                        {
                            modelNamesComboBox.IsEnabled = true;
                            modelNamesComboBox.SelectedIndex = 0;

                            var clientSettings = ClientSettingsManager.Instance.GetSettings();
                            if (clientSettings.SetServerAddress(serverAddressTextBox.Text))
                            {
                                ClientSettingsManager.Instance.SaveSettings(MainWindow.ClientSettingsFileName);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Unable to retrieve models from server", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Unable to contact server", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    connectButton.Content = "Connect";
                    connectButton.IsEnabled = true;
                }
            }
            else
            {
                MessageBox.Show("Invalid input", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
