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
    public class BasicRuleExecutionTests
    {
        private RuleBase _ruleBase = null!;
        private WorkingMemory _workingMemory = null!;

        [SetUp]
        public void SetUp()
        {
            var builder = new PackageBuilder();
            var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("org.drools.dotnet.tests.TestRules.SimpleRule.drl");
            
            builder.AddPackageFromDrl("SimpleRule.drl", stream);
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
        public void FireRules_WithHelloMessage_ShouldUpdateMessage()
        {
            // Arrange
            var message = new Message
            {
                status = Message.HELLO,
                message = "Initial"
            };

            // Act
            _workingMemory.assertObject(message);
            _workingMemory.fireAllRules();

            // Assert
            message.message.Should().Be("Hello from Drools!");
        }

        [Test]
        public void FireRules_WithGoodbyeMessage_ShouldUpdateMessage()
        {
            // Arrange
            var message = new Message
            {
                status = Message.GOODBYE,
                message = "Initial"
            };

            // Act
            _workingMemory.assertObject(message);
            _workingMemory.fireAllRules();

            // Assert
            message.message.Should().Be("Goodbye from Drools!");
        }

        [Test]
        public void FireRules_WithMultipleObjects_ShouldProcessAll()
        {
            // Arrange
            var helloMessage = new Message { status = Message.HELLO, message = "Initial Hello" };
            var goodbyeMessage = new Message { status = Message.GOODBYE, message = "Initial Goodbye" };

            // Act
            _workingMemory.assertObject(helloMessage);
            _workingMemory.assertObject(goodbyeMessage);
            _workingMemory.fireAllRules();

            // Assert
            helloMessage.message.Should().Be("Hello from Drools!");
            goodbyeMessage.message.Should().Be("Goodbye from Drools!");
        }

        [Test]
        public void RetractObject_ShouldRemoveFromWorkingMemory()
        {
            // Arrange
            var message = new Message { status = Message.HELLO, message = "Test" };
            var factHandle = _workingMemory.assertObject(message);

            // Act
            _workingMemory.retractObject(factHandle);
            _workingMemory.fireAllRules();

            // Assert
            message.message.Should().Be("Test"); // Should remain unchanged
        }

        [Test]
        public void ModifyObject_ShouldTriggerRulesAgain()
        {
            // Arrange
            var message = new Message { status = Message.HELLO, message = "Initial" };
            var factHandle = _workingMemory.assertObject(message);
            _workingMemory.fireAllRules();

            // Verify initial rule execution
            message.message.Should().Be("Hello from Drools!");

            // Act - modify and fire again
            message.status = Message.GOODBYE;
            _workingMemory.modifyObject(factHandle, message);
            _workingMemory.fireAllRules();

            // Assert
            message.message.Should().Be("Goodbye from Drools!");
        }
    }
}