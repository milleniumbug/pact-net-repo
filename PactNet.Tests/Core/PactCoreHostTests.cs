﻿using System.Collections.Generic;
using System.Linq;
using PactNet.Core;
using PactNet.Infrastructure.Outputters;
using Xunit;
using NSubstitute;

namespace PactNet.Tests.Core
{
    public class PactCoreHostTests
    {
        internal class TestHostConfig : IPactCoreHostConfig
        {
            public string Script { get; }
            public string Arguments { get; }
            public bool WaitForExit { get; }
            public IEnumerable<IOutput> Outputters { get; }
            public IDictionary<string, string> Environment { get; }

            public TestHostConfig(string script, IEnumerable<IOutput> outputters)
            {
                Script = script;
                Arguments = "";
                WaitForExit = true;
                Outputters = outputters;
            }
        }

        private IEnumerable<IOutput> _mockOutputters;

        private IPactCoreHost GetSubject(string script)
        {
            _mockOutputters = new List<IOutput>
            {
                Substitute.For<IOutput>(),
                Substitute.For<IOutput>(),
                Substitute.For<IOutput>()
            };

            var config = new TestHostConfig(script, _mockOutputters);

            return new PactCoreHost<TestHostConfig>(config);
        }

        [Fact]
        public void Start_WhenStdOutIsWrittenTo_LinesAreWrittenToTheOutputters()
        {
            var pactCoreHost = GetSubject("write-to-stdout");

            pactCoreHost.Start();

            _mockOutputters.ElementAt(0).Received(1).WriteLine("Hello world");
            _mockOutputters.ElementAt(1).Received(1).WriteLine("Hello world");
            _mockOutputters.ElementAt(2).Received(1).WriteLine("Hello world");
        }

        [Fact]
        public void Start_WhenStdErrIsWrittenTo_LinesAreWrittenToTheOutputters()
        {
            var pactCoreHost = GetSubject("write-to-stderr");

            pactCoreHost.Start();

            _mockOutputters.ElementAt(0).Received(1).WriteLine("Oh no");
            _mockOutputters.ElementAt(1).Received(1).WriteLine("Oh no");
            _mockOutputters.ElementAt(2).Received(1).WriteLine("Oh no");
        }
    }
}
