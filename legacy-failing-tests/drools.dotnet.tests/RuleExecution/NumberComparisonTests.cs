using System;
using System.IO;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using org.drools.dotnet.compiler;
using org.drools.dotnet.rule;
using org.drools.dotnet.tests.Models;

namespace org.drools.dotnet.tests.RuleExecution
{
    [TestFixture]
    public class NumberComparisonTests
    {
        private RuleBase _ruleBase = null!;
        private WorkingMemory _workingMemory = null!;

        [SetUp]
        public void SetUp()
        {
            var builder = new PackageBuilder();
            var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("org.drools.dotnet.tests.TestRules.NumberComparison.drl");
            
            builder.AddPackageFromDrl("NumberComparison.drl", stream);
            var package = builder.GetPackage();
            
            _ruleBase = RuleBaseFactory.NewRuleBase();
            _ruleBase.AddPackage(package);
            _workingMemory = _ruleBase.NewWorkingMemory();
            
            stream?.Close();
        }

        [TearDown]
        public void TearDown()
        {
            // WorkingMemory doesn't implement IDisposable
            // _workingMemory?.Dispose();
        }

        [Test]
        public void FireRules_WithHighValue_ShouldCategorizeAsHigh()
        {
            // Arrange
            var number = new TestNumber(150);

            // Act
            _workingMemory.assertObject(number);
            _workingMemory.fireAllRules();

            // Assert
            number.category.Should().Be("HIGH");
        }

        [Test]
        public void FireRules_WithLowValue_ShouldCategorizeAsLow()
        {
            // Arrange
            var number = new TestNumber(25);

            // Act
            _workingMemory.assertObject(number);
            _workingMemory.fireAllRules();

            // Assert
            number.category.Should().Be("LOW");
        }

        [Test]
        public void FireRules_WithMediumValue_ShouldCategorizeAsMedium()
        {
            // Arrange
            var number = new TestNumber(75);

            // Act
            _workingMemory.assertObject(number);
            _workingMemory.fireAllRules();

            // Assert
            number.category.Should().Be("MEDIUM");
        }

        [Test]
        public void FireRules_WithBoundaryValues_ShouldCategorizeCorrectly()
        {
            // Arrange
            var exactly50 = new TestNumber(50);
            var exactly100 = new TestNumber(100);

            // Act
            _workingMemory.assertObject(exactly50);
            _workingMemory.assertObject(exactly100);
            _workingMemory.fireAllRules();

            // Assert
            exactly50.category.Should().Be("MEDIUM");
            exactly100.category.Should().Be("MEDIUM");
        }

        [Test]
        public void FireRules_WithMultipleNumbers_ShouldCategorizeEach()
        {
            // Arrange
            var numbers = new[]
            {
                new TestNumber(25),   // LOW
                new TestNumber(75),   // MEDIUM
                new TestNumber(150),  // HIGH
                new TestNumber(1)     // LOW
            };

            // Act
            foreach (var number in numbers)
            {
                _workingMemory.assertObject(number);
            }
            _workingMemory.fireAllRules();

            // Assert
            numbers[0].category.Should().Be("LOW");
            numbers[1].category.Should().Be("MEDIUM");
            numbers[2].category.Should().Be("HIGH");
            numbers[3].category.Should().Be("LOW");
        }
    }
}