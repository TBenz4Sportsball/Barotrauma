﻿using System;
using System.Diagnostics;
using Barotrauma;
using FluentAssertions;
using FsCheck;
using Xunit;

namespace TestProject;

public sealed class SerializableDateTimeTests
{
    private class CustomGenerators
    {
        private const short MinutesPerDay = 24 * 60;
        private const int SecondsPerDay = MinutesPerDay * 60;

        public static Arbitrary<SerializableDateTime> SerializableDateTimeGenerator()
        {
            return Arb.From(
                from int dateTimeDay in Gen.Choose(0, (int)(DateTime.MaxValue.Ticks / TimeSpan.TicksPerDay))
                from int dateTimeSeconds in Gen.Choose(0, SecondsPerDay)
                from int timeZoneMinutes in Gen.Choose(-MinutesPerDay / 2, MinutesPerDay / 2)
                select new SerializableDateTime(
                    DateTime.MinValue + TimeSpan.FromDays(dateTimeDay) + TimeSpan.FromSeconds(dateTimeSeconds),
                    new SerializableTimeZone(TimeSpan.FromMinutes(timeZoneMinutes))));
        }
    }
    
    public SerializableDateTimeTests()
    {
        Arb.Register<TestProject.CustomGenerators>();
        Arb.Register<CustomGenerators>();
    }

    [Fact]
    public void EqualityTest()
    {
        Prop.ForAll<SerializableDateTime>(EqualityCheck).QuickCheckThrowOnFailure();
    }
    
    [Fact]
    public void ParseTest()
    {
        var parseTest = "9369Y 09M 06D 03HR 43MIN 09SEC UTC+8:49";
        SerializableDateTime.Parse(parseTest);
        Prop.ForAll<SerializableDateTime>(ParseCheck).QuickCheckThrowOnFailure();
    }
    
    private static void EqualityCheck(SerializableDateTime original)
    {
        var local = original.ToLocal();
        var utc = original.ToUtc();
        original.Should().BeEquivalentTo(local);
        original.Should().BeEquivalentTo(utc);
        local.Should().BeEquivalentTo(utc);
    }

    private static void ParseCheck(SerializableDateTime original)
    {
        var str = original.ToString();
        SerializableDateTime.Parse(str).TryUnwrap(out var parsedTime).Should().BeTrue();
        parsedTime.Should().BeEquivalentTo(original);
    }
}
