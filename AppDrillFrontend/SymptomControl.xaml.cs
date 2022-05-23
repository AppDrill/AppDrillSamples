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
using AppDrillCore.Session;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace AppDrillFrontend
{
    /// <summary>
    /// Interaction logic for SymptomControl.xaml
    /// </summary>
    public partial class SymptomControl : UserControl
    {
        public string SymptomId { get; }

        public ISession Session { get; set; }

        public SymptomControl()
        {
            InitializeComponent();
        }

        public SymptomControl(string symptomId, ISession session)
        {
            InitializeComponent();

            SymptomId = symptomId;
            Session = session;
        }

        public void Clear()
        {
            testsDataGrid.ItemsSource = null;
            failuresDataGrid.ItemsSource = null;
        }

        public void Refresh()
        {
            var tests = SymptomId == null ? Session.GetCommonTests() : Session.GetTestsBySymptomId(SymptomId);
            UpdateTests(tests);

            var failures = SymptomId == null ? Session.GetCommonFailures() : Session.GetFailuresBySymptomId(SymptomId);
            UpdateFailures(failures);
        }

        private void UpdateTests(List<Test> tests)
        {
            if (tests == null)
            {
                testsDataGrid.ItemsSource = null;
            }
            else
            {
                List<TestRow> testRows = new List<TestRow>();
                foreach (Test test in tests)
                {
                    testRows.Add(new TestRow(Session)
                    {
                        id = test.InstanceId,
                        name = test.Name,
                        description = test.Description,
                        rank = Math.Round(Session.GetTestRank(test.InstanceId), 4, MidpointRounding.AwayFromZero),
                        cost = Double.Parse(test.Cost),
                        duration = UInt32.Parse(test.Duration),
                        result = test.State,
                        applicable = test.Applicable,
                        url = test.Url
                    });
                }
                testsDataGrid.ItemsSource = testRows;
            }
        }

        private void UpdateFailures(List<Failure> failures)
        {
            if (failures == null)
            {
                failuresDataGrid.ItemsSource = null;
            }
            else
            {
                List<FailureRow> failureRows = new List<FailureRow>();
                foreach (Failure failure in failures)
                {
                    failureRows.Add(new FailureRow
                    {
                        id = failure.InstanceId,
                        name = failure.Name,
                        component = Session.GetComponentForFailure(failure.InstanceId)?.Name,
                        description = failure.Description,
                        probability = Double.Parse(failure.Probability),
                        resolution = failure.Resolution.ToString(),
                        calculatedProbabilityOfFailure = Session.GetCalculatedProbabilityOfFailure(failure.InstanceId)
                    });
                }
                failuresDataGrid.ItemsSource = failureRows;
            }
        }

        private void FailureRow_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var failureRow = (sender as DataGridRow).Item as FailureRow;

            var correctiveActions = Session.GetCorrectiveActions(failureRow.id);

            if (correctiveActions == null || correctiveActions.Count == 0)
            {
                MessageBox.Show("There are no corrective actions associated with this failure", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                var title = $"Corrective action for failure \"{failureRow.name}\"";

                try
                {
                    new DetailsDialog(title, correctiveActions[0].Name, correctiveActions[0].Description, correctiveActions[0].Url).ShowDialog();
                }
                catch (Exception)
                {
                    MessageBox.Show("Failed retrieving corrective action", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Details_ButtonClick(object sender, RoutedEventArgs e)
        {
            var testRow = (sender as Button).DataContext as TestRow;

            try
            {
                new DetailsDialog("Test details", testRow.name, testRow.description, testRow.url).ShowDialog();
            }
            catch (Exception)
            {
                MessageBox.Show("Failed retrieving test", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
