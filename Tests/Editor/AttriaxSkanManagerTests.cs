#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Attriax.Unity;
using Attriax.Unity.Internal;
using NUnit.Framework;

namespace Attriax.Unity.Tests
{
    public sealed class AttriaxSkanManagerTests
    {
        [Test]
        public async Task InitializeRegistersFirstLaunchValue()
        {
            var clock = new DateTimeOffset(2026, 1, 4, 12, 0, 0, TimeSpan.Zero);
            var recorder = new SkanUpdateRecorder();
            var manager = CreateManager(
                clock,
                recorder,
                new AttriaxSkanConfig
                {
                    Enabled = true,
                    RegisterFirstLaunchValue = true,
                });

            await manager.InitializeAsync(isFirstLaunch: true);

            Assert.That(recorder.Calls.Count, Is.EqualTo(1));
            Assert.That(recorder.Calls[0].FineValue, Is.EqualTo(0));
            Assert.That(recorder.Calls[0].CoarseValue, Is.EqualTo(AttriaxSkanCoarseValue.Low));
            Assert.That(manager.State?.FineValue, Is.EqualTo(0));
            Assert.That(manager.State?.FirstLaunchValueRegistered, Is.True);
            Assert.That(manager.State?.InstallAnchorAt, Is.EqualTo(clock));
        }

        [Test]
        public async Task NonIosPlatformsSkipPersistedAndRuntimeSkanHandling()
        {
            var clock = new DateTimeOffset(2026, 1, 4, 12, 0, 0, TimeSpan.Zero);
            var recorder = new SkanUpdateRecorder();
            string? persistedState = "{\"enabled\":true,\"fineValue\":7}";
            var manager = new AttriaxSkanManager(
                new AttriaxSkanConfig
                {
                    Enabled = true,
                    RegisterFirstLaunchValue = true,
                },
                AttriaxPlatformType.Android,
                () => clock,
                () => persistedState,
                value => persistedState = value,
                (_, _) => { },
                null,
                recorder.UpdateAsync);

            await manager.InitializeAsync(isFirstLaunch: true);
            await manager.ApplyAppOpenResultAsync(new AttriaxAppOpenResult
            {
                AcceptedAt = clock,
                InstallState = AttriaxInstallState.Existing,
                Skan = BuildRuntimeConfiguration(1),
            });
            var updateResult = await manager.UpdateConversionValueAsync(0, null, false);
            var trackedEventResult = await manager.HandleTrackedEventAsync(
                "purchase",
                new Dictionary<string, object>());

            Assert.That(updateResult.Status, Is.EqualTo(AttriaxSkanUpdateStatus.NotSupported));
            Assert.That(updateResult.State, Is.Null);
            Assert.That(trackedEventResult, Is.Null);
            Assert.That(manager.State, Is.Null);
            Assert.That(persistedState, Is.Null);
            Assert.That(recorder.Calls, Is.Empty);
        }

        [Test]
        public async Task MatchingWindow1EventEncodesBitRange()
        {
            var clock = new DateTimeOffset(2026, 1, 4, 12, 0, 0, TimeSpan.Zero);
            var recorder = new SkanUpdateRecorder();
            var manager = CreateManager(
                clock,
                recorder,
                new AttriaxSkanConfig
                {
                    Enabled = true,
                    RegisterFirstLaunchValue = false,
                });

            await manager.InitializeAsync(isFirstLaunch: false);
            await manager.ApplyAppOpenResultAsync(new AttriaxAppOpenResult
            {
                AcceptedAt = clock,
                InstallState = AttriaxInstallState.Existing,
                Skan = BuildRuntimeConfiguration(
                    3,
                    window1: new AttriaxSkanWindow1
                    {
                        Groups = new List<AttriaxSkanWindow1Group>
                        {
                            new AttriaxSkanWindow1Group
                            {
                                Id = "group_revenue",
                                StartBit = 4,
                                BitCount = 2,
                                Events = new List<AttriaxSkanEvent>
                                {
                                    new AttriaxSkanEvent
                                    {
                                        Id = "event_add_to_cart",
                                        EventName = "add_to_cart",
                                    },
                                    new AttriaxSkanEvent
                                    {
                                        Id = "event_purchase",
                                        EventName = "purchase",
                                    },
                                    new AttriaxSkanEvent
                                    {
                                        Id = "event_subscription_started",
                                        EventName = "subscription_started",
                                        CoarseValue = AttriaxSkanCoarseValue.High,
                                        LockWindow = true,
                                    },
                                },
                            },
                        },
                    }),
            });

            var result = await manager.HandleTrackedEventAsync(
                "subscription_started",
                new Dictionary<string, object>());

            Assert.That(result, Is.Not.Null);
            Assert.That(result?.Status, Is.EqualTo(AttriaxSkanUpdateStatus.Updated));
            Assert.That(recorder.Calls.Last().FineValue, Is.EqualTo(48));
            Assert.That(recorder.Calls.Last().CoarseValue, Is.EqualTo(AttriaxSkanCoarseValue.High));
            Assert.That(recorder.Calls.Last().LockWindow, Is.True);
            Assert.That(manager.State?.SchemaVersion, Is.EqualTo(3));
            Assert.That(manager.State?.Schema?.Window1.Groups.Single().StartBit, Is.EqualTo(4));
            Assert.That(manager.State?.FineValue, Is.EqualTo(48));
        }

        [Test]
        public async Task MatchingWindow2EventAppliesCoarseOnlyUpdate()
        {
            var now = new DateTimeOffset(2026, 1, 8, 12, 0, 0, TimeSpan.Zero);
            var installAnchor = new DateTimeOffset(2026, 1, 4, 12, 0, 0, TimeSpan.Zero);
            var recorder = new SkanUpdateRecorder();
            var manager = CreateManager(
                now,
                recorder,
                new AttriaxSkanConfig
                {
                    Enabled = true,
                    RegisterFirstLaunchValue = false,
                });

            await manager.InitializeAsync(isFirstLaunch: false);
            await manager.ApplyAppOpenResultAsync(new AttriaxAppOpenResult
            {
                AcceptedAt = installAnchor,
                InstallState = AttriaxInstallState.Existing,
                Skan = BuildRuntimeConfiguration(
                    4,
                    window2: new AttriaxSkanCoarseWindow
                    {
                        Events = new List<AttriaxSkanCoarseWindowEvent>
                        {
                            new AttriaxSkanCoarseWindowEvent
                            {
                                Id = "purchase_window_2",
                                EventName = "purchase",
                                CoarseValue = AttriaxSkanCoarseValue.Medium,
                                LockWindow = true,
                                Conditions = new List<AttriaxSkanCondition>
                                {
                                    new AttriaxSkanCondition
                                    {
                                        Id = "plan",
                                        ParamKey = "plan",
                                        Operator = AttriaxSkanRuleOperator.Eq,
                                        Value = "pro",
                                    },
                                },
                            },
                        },
                    }),
            });

            var result = await manager.HandleTrackedEventAsync(
                "purchase",
                new Dictionary<string, object>
                {
                    ["plan"] = "pro",
                });

            Assert.That(result, Is.Not.Null);
            Assert.That(recorder.Calls.Count, Is.EqualTo(1));
            Assert.That(recorder.Calls[0].FineValue, Is.EqualTo(0));
            Assert.That(recorder.Calls[0].CoarseValue, Is.EqualTo(AttriaxSkanCoarseValue.Medium));
            Assert.That(recorder.Calls[0].LockWindow, Is.True);
            Assert.That(manager.State?.FineValue, Is.EqualTo(0));
            Assert.That(manager.State?.CoarseValue, Is.EqualTo(AttriaxSkanCoarseValue.Medium));
        }

        [Test]
        public async Task PurchaseSignalsUseLocalUsdRevenueAndCount()
        {
            var clock = new DateTimeOffset(2026, 1, 4, 12, 0, 0, TimeSpan.Zero);
            var recorder = new SkanUpdateRecorder();
            var manager = CreateManager(
                clock,
                recorder,
                new AttriaxSkanConfig
                {
                    Enabled = true,
                    RegisterFirstLaunchValue = false,
                });

            await manager.InitializeAsync(isFirstLaunch: false);
            await manager.ApplyAppOpenResultAsync(new AttriaxAppOpenResult
            {
                AcceptedAt = clock,
                InstallState = AttriaxInstallState.Existing,
                Skan = BuildRuntimeConfiguration(
                    6,
                    window1: new AttriaxSkanWindow1
                    {
                        Groups = new List<AttriaxSkanWindow1Group>
                        {
                            new AttriaxSkanWindow1Group
                            {
                                Id = "group_purchase",
                                StartBit = 0,
                                BitCount = 2,
                                Events = new List<AttriaxSkanEvent>
                                {
                                    new AttriaxSkanEvent
                                    {
                                        Id = "second_purchase",
                                        EventName = "purchase",
                                        CoarseValue = AttriaxSkanCoarseValue.Medium,
                                        Conditions = new List<AttriaxSkanCondition>
                                        {
                                            new AttriaxSkanCondition
                                            {
                                                Id = "count",
                                                ParamKey = "count",
                                                Operator = AttriaxSkanRuleOperator.Gte,
                                                Value = 2,
                                            },
                                        },
                                    },
                                    new AttriaxSkanEvent
                                    {
                                        Id = "revenue",
                                        EventName = "purchase",
                                        CoarseValue = AttriaxSkanCoarseValue.High,
                                        LockWindow = true,
                                        Conditions = new List<AttriaxSkanCondition>
                                        {
                                            new AttriaxSkanCondition
                                            {
                                                Id = "revenue",
                                                ParamKey = "revenue",
                                                Operator = AttriaxSkanRuleOperator.Gte,
                                                Value = 5,
                                            },
                                        },
                                    },
                                },
                            },
                        },
                    }),
            });

            await manager.HandleTrackedEventAsync(
                "purchase",
                new Dictionary<string, object>
                {
                    ["revenue"] = 2.5,
                });
            await manager.HandleTrackedEventAsync(
                "purchase",
                new Dictionary<string, object>
                {
                    ["revenue"] = 3.0,
                });

            Assert.That(recorder.Calls.Count, Is.EqualTo(1));
            Assert.That(recorder.Calls[0].FineValue, Is.EqualTo(2));
            Assert.That(recorder.Calls[0].CoarseValue, Is.EqualTo(AttriaxSkanCoarseValue.High));
            Assert.That(recorder.Calls[0].LockWindow, Is.True);
            Assert.That(manager.State?.PurchaseRevenueUsdMicros, Is.EqualTo(5500000));
            Assert.That(manager.State?.PurchaseCount, Is.EqualTo(2));
        }

        [Test]
        public async Task NonUsdPurchaseRevenueUsesConverterBeforeMatching()
        {
            var clock = new DateTimeOffset(2026, 1, 4, 12, 0, 0, TimeSpan.Zero);
            var recorder = new SkanUpdateRecorder();
            long? convertedAmountMicros = null;
            string? convertedCurrency = null;
            var manager = CreateManager(
                clock,
                recorder,
                new AttriaxSkanConfig
                {
                    Enabled = true,
                    RegisterFirstLaunchValue = false,
                },
                (amountMicros, currency, _) =>
                {
                    convertedAmountMicros = amountMicros;
                    convertedCurrency = currency;
                    return Task.FromResult<long?>(1200000);
                });

            await manager.InitializeAsync(isFirstLaunch: false);
            await manager.ApplyAppOpenResultAsync(new AttriaxAppOpenResult
            {
                AcceptedAt = clock,
                InstallState = AttriaxInstallState.Existing,
                Skan = BuildRuntimeConfiguration(
                    7,
                    window1: new AttriaxSkanWindow1
                    {
                        Groups = new List<AttriaxSkanWindow1Group>
                        {
                            new AttriaxSkanWindow1Group
                            {
                                Id = "group_purchase",
                                StartBit = 0,
                                BitCount = 1,
                                Events = new List<AttriaxSkanEvent>
                                {
                                    new AttriaxSkanEvent
                                    {
                                        Id = "revenue",
                                        EventName = "purchase",
                                        CoarseValue = AttriaxSkanCoarseValue.Medium,
                                        Conditions = new List<AttriaxSkanCondition>
                                        {
                                            new AttriaxSkanCondition
                                            {
                                                Id = "revenue",
                                                ParamKey = "revenue",
                                                Operator = AttriaxSkanRuleOperator.Gte,
                                                Value = 1.2,
                                            },
                                        },
                                    },
                                },
                            },
                        },
                    }),
            });

            await manager.HandleTrackedEventAsync(
                "purchase",
                new Dictionary<string, object>
                {
                    ["revenue"] = 0.99,
                    ["currency"] = "eur",
                });

            Assert.That(convertedAmountMicros, Is.EqualTo(990000));
            Assert.That(convertedCurrency, Is.EqualTo("EUR"));
            Assert.That(recorder.Calls.Count, Is.EqualTo(1));
            Assert.That(recorder.Calls[0].FineValue, Is.EqualTo(1));
            Assert.That(recorder.Calls[0].CoarseValue, Is.EqualTo(AttriaxSkanCoarseValue.Medium));
            Assert.That(manager.State?.PurchaseRevenueUsdMicros, Is.EqualTo(1200000));
            Assert.That(manager.State?.PurchaseCount, Is.EqualTo(1));
        }

        [Test]
        public async Task FailedNonUsdRevenueConversionFallsBackToOneUsd()
        {
            var clock = new DateTimeOffset(2026, 1, 4, 12, 0, 0, TimeSpan.Zero);
            var recorder = new SkanUpdateRecorder();
            var manager = CreateManager(
                clock,
                recorder,
                new AttriaxSkanConfig
                {
                    Enabled = true,
                    RegisterFirstLaunchValue = false,
                },
                (_, _, _) => throw new InvalidOperationException("conversion failed"));

            await manager.InitializeAsync(isFirstLaunch: false);
            await manager.ApplyAppOpenResultAsync(new AttriaxAppOpenResult
            {
                AcceptedAt = clock,
                InstallState = AttriaxInstallState.Existing,
                Skan = BuildRuntimeConfiguration(8),
            });

            await manager.HandleTrackedEventAsync(
                "purchase",
                new Dictionary<string, object>
                {
                    ["revenue"] = 9.99,
                    ["currency"] = "eur",
                });

            Assert.That(recorder.Calls.Count, Is.EqualTo(0));
            // A failed FX lookup optimistically counts as $1 USD = 1,000,000 micros.
            Assert.That(manager.State?.PurchaseRevenueUsdMicros, Is.EqualTo(1000000));
            Assert.That(manager.State?.PurchaseCount, Is.EqualTo(1));
        }

        [Test]
        public async Task ManualUpdateWithZeroFineValueDoesNotLatchFirstLaunch()
        {
            var clock = new DateTimeOffset(2026, 1, 4, 12, 0, 0, TimeSpan.Zero);
            var recorder = new SkanUpdateRecorder();
            var manager = CreateManager(
                clock,
                recorder,
                new AttriaxSkanConfig
                {
                    Enabled = true,
                    RegisterFirstLaunchValue = false,
                });

            await manager.InitializeAsync(isFirstLaunch: true);

            var result = await manager.UpdateConversionValueAsync(0, null, false);

            Assert.That(result.Status, Is.EqualTo(AttriaxSkanUpdateStatus.Updated));
            Assert.That(manager.State?.FineValue, Is.EqualTo(0));
            // A resolved fineValue of 0 must not flip the first-launch registration
            // latch; only an explicit install-registration update may.
            Assert.That(manager.State?.FirstLaunchValueRegistered, Is.False);
        }

        [Test]
        public async Task PurchaseCountDoesNotInventRevenue()
        {
            var clock = new DateTimeOffset(2026, 1, 4, 12, 0, 0, TimeSpan.Zero);
            var recorder = new SkanUpdateRecorder();
            var manager = CreateManager(
                clock,
                recorder,
                new AttriaxSkanConfig
                {
                    Enabled = true,
                    RegisterFirstLaunchValue = false,
                });

            await manager.InitializeAsync(isFirstLaunch: false);
            await manager.ApplyAppOpenResultAsync(new AttriaxAppOpenResult
            {
                AcceptedAt = clock,
                InstallState = AttriaxInstallState.Existing,
                Skan = BuildRuntimeConfiguration(
                    9,
                    window1: new AttriaxSkanWindow1
                    {
                        Groups = new List<AttriaxSkanWindow1Group>
                        {
                            new AttriaxSkanWindow1Group
                            {
                                Id = "group_purchase_count",
                                StartBit = 0,
                                BitCount = 1,
                                Events = new List<AttriaxSkanEvent>
                                {
                                    new AttriaxSkanEvent
                                    {
                                        Id = "second_purchase",
                                        EventName = "purchase",
                                        CoarseValue = AttriaxSkanCoarseValue.Medium,
                                        Conditions = new List<AttriaxSkanCondition>
                                        {
                                            new AttriaxSkanCondition
                                            {
                                                Id = "count",
                                                ParamKey = "count",
                                                Operator = AttriaxSkanRuleOperator.Gte,
                                                Value = 2,
                                            },
                                        },
                                    },
                                },
                            },
                        },
                    }),
            });

            await manager.HandleTrackedEventAsync("purchase", new Dictionary<string, object>());
            await manager.HandleTrackedEventAsync("purchase", new Dictionary<string, object>());

            Assert.That(recorder.Calls.Count, Is.EqualTo(1));
            Assert.That(recorder.Calls[0].FineValue, Is.EqualTo(1));
            Assert.That(recorder.Calls[0].CoarseValue, Is.EqualTo(AttriaxSkanCoarseValue.Medium));
            Assert.That(manager.State?.PurchaseCount, Is.EqualTo(2));
            Assert.That(manager.State?.PurchaseRevenueUsdMicros, Is.EqualTo(0));
        }

        [Test]
        public async Task AdShowSignalsUseLocalCount()
        {
            var clock = new DateTimeOffset(2026, 1, 4, 12, 0, 0, TimeSpan.Zero);
            var recorder = new SkanUpdateRecorder();
            var manager = CreateManager(
                clock,
                recorder,
                new AttriaxSkanConfig
                {
                    Enabled = true,
                    RegisterFirstLaunchValue = false,
                });

            await manager.InitializeAsync(isFirstLaunch: false);
            await manager.ApplyAppOpenResultAsync(new AttriaxAppOpenResult
            {
                AcceptedAt = clock,
                InstallState = AttriaxInstallState.Existing,
                Skan = BuildRuntimeConfiguration(
                    8,
                    window1: new AttriaxSkanWindow1
                    {
                        Groups = new List<AttriaxSkanWindow1Group>
                        {
                            new AttriaxSkanWindow1Group
                            {
                                Id = "group_ads",
                                StartBit = 0,
                                BitCount = 1,
                                Events = new List<AttriaxSkanEvent>
                                {
                                    new AttriaxSkanEvent
                                    {
                                        Id = "ads",
                                        EventName = "ad_show",
                                        CoarseValue = AttriaxSkanCoarseValue.Medium,
                                        Conditions = new List<AttriaxSkanCondition>
                                        {
                                            new AttriaxSkanCondition
                                            {
                                                Id = "shown",
                                                ParamKey = "shown",
                                                Operator = AttriaxSkanRuleOperator.Gte,
                                                Value = 2,
                                            },
                                        },
                                    },
                                },
                            },
                        },
                    }),
            });

            await manager.HandleTrackedEventAsync("ad_show", new Dictionary<string, object>());
            await manager.HandleTrackedEventAsync("ad_show", new Dictionary<string, object>());

            Assert.That(recorder.Calls.Count, Is.EqualTo(1));
            Assert.That(recorder.Calls[0].FineValue, Is.EqualTo(1));
            Assert.That(recorder.Calls[0].CoarseValue, Is.EqualTo(AttriaxSkanCoarseValue.Medium));
            Assert.That(manager.State?.AdShowCount, Is.EqualTo(2));
        }

        [Test]
        public async Task AppOpenEvaluatesRetentionMilestonesFromConfiguredEvents()
        {
            var now = new DateTimeOffset(2026, 1, 11, 12, 0, 0, TimeSpan.Zero);
            var installAnchor = new DateTimeOffset(2026, 1, 4, 12, 0, 0, TimeSpan.Zero);
            var recorder = new SkanUpdateRecorder();
            var manager = CreateManager(
                now,
                recorder,
                new AttriaxSkanConfig
                {
                    Enabled = true,
                    RegisterFirstLaunchValue = false,
                });

            await manager.InitializeAsync(isFirstLaunch: false);
            await manager.ApplyAppOpenResultAsync(new AttriaxAppOpenResult
            {
                AcceptedAt = installAnchor,
                InstallState = AttriaxInstallState.Existing,
                Skan = BuildRuntimeConfiguration(
                    5,
                    window2: new AttriaxSkanCoarseWindow
                    {
                        Events = new List<AttriaxSkanCoarseWindowEvent>
                        {
                            new AttriaxSkanCoarseWindowEvent
                            {
                                Id = "retention_day_7",
                                EventName = "_attriax_retention",
                                CoarseValue = AttriaxSkanCoarseValue.High,
                                Conditions = new List<AttriaxSkanCondition>
                                {
                                    new AttriaxSkanCondition
                                    {
                                        Id = "day",
                                        ParamKey = "day",
                                        Operator = AttriaxSkanRuleOperator.Eq,
                                        Value = 7,
                                    },
                                },
                            },
                        },
                    }),
            });

            Assert.That(recorder.Calls.Count, Is.EqualTo(1));
            Assert.That(recorder.Calls[0].FineValue, Is.EqualTo(0));
            Assert.That(recorder.Calls[0].CoarseValue, Is.EqualTo(AttriaxSkanCoarseValue.High));
            Assert.That(manager.State?.CompletedRetentionDays, Is.EquivalentTo(new[] { 7 }));
        }

        private static AttriaxSkanRuntimeConfiguration BuildRuntimeConfiguration(
            int version,
            AttriaxSkanWindow1? window1 = null,
            AttriaxSkanCoarseWindow? window2 = null,
            AttriaxSkanCoarseWindow? window3 = null)
        {
            return new AttriaxSkanRuntimeConfiguration
            {
                Enabled = true,
                Schema = new AttriaxSkanSchema
                {
                    Version = version,
                    Window1 = window1 ?? new AttriaxSkanWindow1(),
                    Window2 = window2 ?? new AttriaxSkanCoarseWindow(),
                    Window3 = window3 ?? new AttriaxSkanCoarseWindow(),
                },
            };
        }

        private static AttriaxSkanManager CreateManager(
            DateTimeOffset clock,
            SkanUpdateRecorder recorder,
            AttriaxSkanConfig? config = null,
            Func<long, string, DateTimeOffset, Task<long?>>? revenueConverter = null)
        {
            string? persistedState = null;
            return new AttriaxSkanManager(
                config ?? new AttriaxSkanConfig(),
                AttriaxPlatformType.IOS,
                () => clock,
                () => persistedState,
                value => persistedState = value,
                (_, _) => { },
                revenueConverter,
                recorder.UpdateAsync);
        }

        private sealed class SkanUpdateRecorder
        {
            public List<SkanUpdateCall> Calls { get; } = new List<SkanUpdateCall>();

            public Task<AttriaxSkanUpdateResult> UpdateAsync(
                AttriaxPlatformType platform,
                int fineValue,
                AttriaxSkanCoarseValue? coarseValue,
                bool lockWindow)
            {
                Calls.Add(new SkanUpdateCall(platform, fineValue, coarseValue, lockWindow));

                return Task.FromResult(new AttriaxSkanUpdateResult
                {
                    Status = AttriaxSkanUpdateStatus.Updated,
                    FineValue = fineValue,
                    CoarseValue = coarseValue,
                    LockWindow = lockWindow,
                });
            }
        }

        private readonly struct SkanUpdateCall
        {
            public SkanUpdateCall(
                AttriaxPlatformType platform,
                int fineValue,
                AttriaxSkanCoarseValue? coarseValue,
                bool lockWindow)
            {
                Platform = platform;
                FineValue = fineValue;
                CoarseValue = coarseValue;
                LockWindow = lockWindow;
            }

            public AttriaxPlatformType Platform { get; }

            public int FineValue { get; }

            public AttriaxSkanCoarseValue? CoarseValue { get; }

            public bool LockWindow { get; }
        }
    }
}
