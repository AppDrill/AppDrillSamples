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
using System.Windows;

namespace AppDrillFrontend
{
    /// <summary>
    /// Interaction logic for ClientSettingsDialog.xaml
    /// </summary>
    public partial class ClientSettingsDialog : Window
    {
        public ClientSettingsDialog()
        {
            InitializeComponent();

            machineStateComboBox.ItemsSource = Enum.GetNames(typeof(ClientSettings.EMachineState));
            userLevelComboBox.ItemsSource = Enum.GetNames(typeof(ClientSettings.EUserLevel));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadClientSettings();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            SaveClientSettings();
        }

        private void LoadClientSettings()
        {
            var clientSettings = ClientSettingsManager.Instance.GetSettings();
            machineStateComboBox.SelectedItem = clientSettings.MachineState.ToString();
            userLevelComboBox.SelectedItem = clientSettings.UserLevel.ToString();
            testCostTextBox.Text = clientSettings.Weight.Cost.ToString();
            testDurationTextBox.Text = clientSettings.Weight.Duration.ToString();
            probabilityOfFailureTextBox.Text = clientSettings.Weight.FailureProbability.ToString();
        }

        private void SaveClientSettings()
        {
            var weight = new ClientSettings.WeightDistribution();
            weight.Cost = UInt16.Parse(testCostTextBox.Text);
            weight.Duration = UInt16.Parse(testDurationTextBox.Text);
            weight.FailureProbability = UInt16.Parse(probabilityOfFailureTextBox.Text);
            if (weight.IsValid())
            {
                var clientSettings = ClientSettingsManager.Instance.GetSettings();

                var setMachineState = clientSettings.SetMachineState(machineStateComboBox.SelectedItem.ToString());
                var setUserLevel = clientSettings.SetUserLevel(userLevelComboBox.SelectedItem.ToString());
                var setWeight = clientSettings.SetWeight(weight);

                if (setMachineState || setUserLevel || setWeight)
                {
                    ClientSettingsManager.Instance.SaveSettings(MainWindow.ClientSettingsFileName);
                }

                Close();
            }
            else
            {
                MessageBox.Show("Please provide values in the range 0..100. Values should sum exactly to 100%", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
