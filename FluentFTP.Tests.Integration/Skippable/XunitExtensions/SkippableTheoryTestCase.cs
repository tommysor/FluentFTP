﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace FluentFTP.Tests.Integration.Skippable.XunitExtensions
{
	public class SkippableTheoryTestCase : XunitTheoryTestCase
	{
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
		public SkippableTheoryTestCase() { }

		public SkippableTheoryTestCase(IMessageSink diagnosticMessageSink, TestMethodDisplay defaultMethodDisplay, TestMethodDisplayOptions defaultMethodDisplayOptions, ITestMethod testMethod)
			: base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod) { }

		public override async Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink,
														IMessageBus messageBus,
														object[] constructorArguments,
														ExceptionAggregator aggregator,
														CancellationTokenSource cancellationTokenSource)
		{
			// Duplicated code from SkippableFactTestCase. I'm sure we could find a way to de-dup with some thought.
			var skipMessageBus = new SkippableFactMessageBus(messageBus);
			RunSummary result;
			if (SkippableState.ShouldSkip)
			{
				/*
				 * This does skip execution, but does not register as "skipped" in the summary.
				 */
				result = new RunSummary
				{
					Total = 1,
					Skipped = 1,
				};
			}
			else
			{
				result = await base.RunAsync(diagnosticMessageSink, skipMessageBus, constructorArguments, aggregator, cancellationTokenSource);
			}
			
			if (skipMessageBus.DynamicallySkippedTestCount > 0)
			{
				result.Failed -= skipMessageBus.DynamicallySkippedTestCount;
				result.Skipped += skipMessageBus.DynamicallySkippedTestCount;
			}

			return result;
		}
	}
}
