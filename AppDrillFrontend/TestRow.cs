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

namespace AppDrillFrontend
{
    public class TestRow
    {
        private ISession _session;
        private TestResult _testResult;
        public TestRow(ISession session)
        {
            _session = session;
            _testResult = TestResult.Unknown;
        }

        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }

        public TestResult result
        {
            get { return _testResult; }
            set
            {
                if (value != _testResult)
                {
                    _testResult = value;
                    _session.SetTestResult(id, value);
                }
            }
        }

        public double cost { get; set; }
        public uint duration { get; set; }
        public double rank { get; set; }
        public bool applicable { get; set; }
        public string url { get; set; }
    }
}
