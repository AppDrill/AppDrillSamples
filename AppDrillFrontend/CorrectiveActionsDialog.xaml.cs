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

using AppDrillCore.Model;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AppDrillFrontend
{
    /// <summary>
    /// Interaction logic for CorrectiveActionsDialog.xaml
    /// </summary>
    public partial class CorrectiveActionsDialog : Window
    {
        public CorrectiveActionsDialog(string failureName, List<CorrectiveAction> correctiveActions)
        {
            InitializeComponent();

            Title = $"Corrective action for failure \"{failureName}\"";

            if (correctiveActions.Count > 0)
            {
                caLabel.Content = correctiveActions[0].Name;
                caTextBlock.Text = correctiveActions[0].Description;

                if (correctiveActions[0].Url != null)
                {
                    LoadImage(correctiveActions[0].Url);
                }
            }
            else
            {
                throw new ArgumentException();
            }
        }

        private void LoadImage(string imageUrl)
        {
            try
            {
                BitmapImage bitmap = new();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imageUrl);
                bitmap.EndInit();
                caImage.Source = bitmap;
            }
            catch (Exception)
            {
                MessageBox.Show($"Unable to load `{imageUrl}`", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
