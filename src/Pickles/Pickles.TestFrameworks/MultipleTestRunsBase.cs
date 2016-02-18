﻿//  --------------------------------------------------------------------------------------------------------------------
//  <copyright file="MultipleTestRunsBase.cs" company="PicklesDoc">
//  Copyright 2011 Jeffrey Cameron
//  Copyright 2012-present PicklesDoc team and community contributors
//
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

using PicklesDoc.Pickles.ObjectModel;

namespace PicklesDoc.Pickles.TestFrameworks
{
    public abstract class MultipleTestRunsBase : ITestResults
    {
        private readonly ISingleResultLoader singleResultLoader;

        protected MultipleTestRunsBase(bool supportsExampleResults, IEnumerable<SingleTestRunBase> testResults)
        {
            this.SupportsExampleResults = supportsExampleResults;
            this.TestResults = testResults;
        }

        protected MultipleTestRunsBase(bool supportsExampleResults, IConfiguration configuration, ISingleResultLoader singleResultLoader, IExampleSignatureBuilder exampleSignatureBuilder, IScenarioOutlineExampleMatcher scenarioOutlineExampleMatcher = null)
        {
            if (exampleSignatureBuilder == null)
            {
                throw new ArgumentNullException(nameof(exampleSignatureBuilder));
            }

            this.SupportsExampleResults = supportsExampleResults;
            this.singleResultLoader = singleResultLoader;
            this.TestResults = this.GetSingleTestResults(configuration);

            this.SetExampleSignatureBuilder(exampleSignatureBuilder, scenarioOutlineExampleMatcher ?? new ScenarioOutlineExampleMatcher());
        }

        private void SetExampleSignatureBuilder(IExampleSignatureBuilder exampleSignatureBuilder, IScenarioOutlineExampleMatcher scenarioOutlineExampleMatcher)
        {
            foreach (var testResult in this.TestResults)
            {
                testResult.ExampleSignatureBuilder = exampleSignatureBuilder;
                testResult.ScenarioOutlineExampleMatcher = scenarioOutlineExampleMatcher;
            }
        }

        public bool SupportsExampleResults { get; }

        protected IEnumerable<SingleTestRunBase> TestResults { get; }

        public TestResult GetExampleResult(ScenarioOutline scenarioOutline, string[] arguments)
        {
            if (SupportsExampleResults)
            {
                var results = TestResults.Select(tr => tr.GetExampleResult(scenarioOutline, arguments)).ToArray();

                return EvaluateTestResults(results);
            }
            else
            {
                return TestResult.Passed;
            }
        }

        public TestResult GetFeatureResult(Feature feature)
        {
            var results = this.TestResults.Select(tr => tr.GetFeatureResult(feature)).ToArray();

            return EvaluateTestResults(results);
        }

        public TestResult GetScenarioOutlineResult(ScenarioOutline scenarioOutline)
        {
            var results = this.TestResults.Select(tr => tr.GetScenarioOutlineResult(scenarioOutline)).ToArray();

            return EvaluateTestResults(results);
        }

        public TestResult GetScenarioResult(Scenario scenario)
        {
            var results = this.TestResults.Select(tr => tr.GetScenarioResult(scenario)).ToArray();

            return EvaluateTestResults(results);
        }

        protected static TestResult EvaluateTestResults(TestResult[] results)
        {
            return results.Merge(true);
        }

        protected SingleTestRunBase ConstructSingleTestResult(FileInfoBase fileInfo)
        {
            return this.singleResultLoader.Load(fileInfo);
        }

        private IEnumerable<SingleTestRunBase> GetSingleTestResults(IConfiguration configuration)
        {
            SingleTestRunBase[] results;

            if (configuration.HasTestResults)
            {
                results = configuration.TestResultsFiles.Select(this.ConstructSingleTestResult).ToArray();
            }
            else
            {
                results = new SingleTestRunBase[0];
            }

            return results;
        }
    }
}
