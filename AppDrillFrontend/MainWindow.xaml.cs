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

using AppDrillCore.Connection;
using AppDrillCore.Model;
using AppDrillCore.Session;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace AppDrillFrontend
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, ISessionDataListener
    {
        private ISession _session;

        private Microsoft.Extensions.Logging.ILogger _logger;

        private string _version;

        public static string ClientSettingsFileName =
            $"{System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}" +
            $"{Path.DirectorySeparatorChar}" +
            $"AppDrill" +
            $"{Path.DirectorySeparatorChar}" +
            $"settings.json";

        public MainWindow()
        {
            InitializeComponent();

            InitializeVersion();

            InitializeLogger();

            InitializeClientSettings();
        }

        private void InitializeVersion()
        {
            string gitVersion = String.Empty;

            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("AppDrillFrontend.version.txt"))
            using (StreamReader reader = new(stream))
            {
                gitVersion = reader.ReadToEnd().Trim();
            }

            _version = $"1.0-{gitVersion}";
        }

        private void InitializeLogger()
        {
            var serilogLogger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File($"{Path.GetTempPath()}appdrill-.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            var loggerFactory = new LoggerFactory().AddSerilog(serilogLogger);

            _logger = loggerFactory.CreateLogger("Logger");

            if (_logger != null)
            {
                _logger.LogInformation("=====================================================================");
                _logger.LogInformation("AppDrill Troubleshooter version {Version}", _version);
                _logger.LogInformation("=====================================================================");
            }
        }

        private static void InitializeClientSettings()
        {
            ClientSettingsManager.Instance.LoadSettings(ClientSettingsFileName);
        }

        public void SessionDataChanged(ISessionDataListener.SessionDataChangedEventType eventType)
        {
            _logger.LogInformation("[MainWindow] Got SessionDataChanged event of type {Type}", eventType);

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                foreach (TabItem tab in symptomsTabControl.Items)
                {
                    var symptomControl = tab.Content as SymptomControl;
                    if (eventType == ISessionDataListener.SessionDataChangedEventType.SingleTest)
                    {
                        _session.RefreshSymptomData(symptomControl.SymptomId);
                    }
                    else
                    {
                        symptomControl.Refresh();
                    }
                }
            }));
        }

        private void ShowDialog(Window dialog)
        {
            dialog.Owner = this;
            dialog.ShowDialog();
        }

        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _logger.LogDebug("[MainWindow] Showing client settings dialog");

            ShowDialog(new ClientSettingsDialog());
        }

        private void NewSessionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _logger.LogDebug("[MainWindow] Showing new session dialog");

            ShowDialog(new NewSessionDialog(this));
        }

        private void RestartSessionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _logger.LogDebug("[MainWindow] RestartSessionMenuItem_Click()");

            RemoveAllSymptoms();
            commonSymptomsControl.Clear();
            UpdateSymptomsComboBox();
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _logger.LogDebug("[MainWindow] ExitMenuItem_Click()");

            Close();
        }

        public bool StartNewSession(string serverAddress, string modelName)
        {
            if (_session != null)
            {
                SessionManager.Instance.EndSession(_session);
            }

            _session = SessionManager.Instance.BeginSession(this, new ConnectionInfo(serverAddress, modelName), _logger);
            if (_session == null)
            {
                return false;
            }

            Clear();

            Title += $": {modelName}";

            mainTitleLabel.Content = $"Troubleshooting session for model \"{modelName}\"";

            restartSessionMenuItem.IsEnabled = true;

            commonSymptomsControl.Session = _session;

            UpdateSymptomsComboBox();

            return true;
        }

        private void Clear()
        {
            _logger.LogDebug("[MainWindow] Clear()");

            RemoveAllSymptoms();
            commonSymptomsControl.Clear();
            symptomsComboxBox.Text = "Session inactive";
            symptomsComboxBox.IsEnabled = false;
            Title = "AppDrill Troubleshooter";
        }

        private void UpdateSymptomsComboBox()
        {
            _logger.LogDebug("[MainWindow] UpdateSymptomsComboBox()");

            var symptoms = _session.GetSymptoms();
            if ((symptoms != null) && (symptoms.Count > 0))
            {
                var checkedSymptoms = symptoms.Select(x => new CheckedSymptom() { Checked = false, SymptomObj = x }).ToList();

                symptomsComboxBox.ItemsSource = checkedSymptoms;
                UpdateSymptomsComboBoxText();
                symptomsComboxBox.IsEnabled = true;
            }
            else
            {
                MessageBox.Show("Failed retrieving data from server", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateSymptomsComboBoxText()
        {
            symptomsComboxBox.Text = $"{symptomsComboxBox.Items.Count} symptoms available, {symptomsTabControl.Items.Count - 1} selected";
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _logger.LogDebug("[MainWindow] Showing about dialog");

            ShowDialog(new AboutDialog(_version));
        }

        private void SymptomCheckboxChecked(object sender, RoutedEventArgs e)
        {
            var chkbox = (sender as CheckBox);
            var symptom = chkbox.Content as Symptom;

            if ((bool)chkbox.IsChecked)
            {
                AddSymptom(symptom);
            }
            else
            {
                RemoveSymptom(symptom);
            }

            UpdateSymptomsComboBoxText();
        }

        private void AddSymptom(Symptom symptom)
        {
            _logger.LogDebug("[MainWindow] Adding symptom: id({Id}), name({SymptomName})", symptom.Id, symptom.Name);

            var newSymptomControl = new SymptomControl(symptom.Id, _session);
            var newTabItem = new TabItem
            {
                Header = symptom.Name,
                Content = newSymptomControl
            };
            symptomsTabControl.Items.Add(newTabItem);

            if (!_session.RefreshSymptomData(symptom.Id))
            {
                MessageBox.Show("Failed retrieving data from server", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveSymptom(Symptom symptom)
        {
            _logger.LogDebug("[MainWindow] Removing symptom: id({SymptomId}), name({SymptomName})", symptom.InstanceId, symptom.Name);

            for (int i = symptomsTabControl.Items.Count - 1; i > 0; --i)
            {
                var tab = symptomsTabControl.Items[i] as TabItem;
                var symptomControl = tab.Content as SymptomControl;
                if (symptomControl.SymptomId.Equals(symptom.Id))
                {
                    symptomsTabControl.Items.RemoveAt(i);
                    break;
                }
            }

            _session.DiscardSymptomData(symptom.Id);
        }

        private void RemoveAllSymptoms()
        {
            _logger.LogDebug("[MainWindow] Removing all symptoms");

            for (int i = symptomsTabControl.Items.Count - 1; i > 0; --i)
            {
                var tab = symptomsTabControl.Items[i] as TabItem;
                var symptomControl = tab.Content as SymptomControl;
                symptomsTabControl.Items.RemoveAt(i);
                _session.DiscardSymptomData(symptomControl.SymptomId);
            }
        }

        public List<string> GetModelNames(string serverAddress)
        {
            return SessionManager.Instance.GetModelNames(serverAddress, _logger);
        }
    }
}
