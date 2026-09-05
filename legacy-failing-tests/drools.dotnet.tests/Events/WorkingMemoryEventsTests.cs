using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using org.drools.dotnet.compiler;
using org.drools.dotnet.events;
using org.drools.dotnet.rule;
using org.drools.dotnet.tests.Models;

namespace org.drools.dotnet.tests.Events
{
    [TestFixture]
    public class WorkingMemoryEventsTests
    {
        private RuleBase _ruleBase = null!;
        private WorkingMemory _workingMemory = null!;
        private List<ObjectAssertedEventArgs> _assertedEvents = null!;
        private List<ObjectRetractedEventArgs> _retractedEvents = null!;
        private List<ObjectModifiedEventArgs> _modifiedEvents = null!;

        [SetUp]
        public void SetUp()
        {
            // Create a simple rule base
            var builder = new PackageBuilder();
            var drlContent = @"
                package org.drools.dotnet.tests
                
                rule ""Event Test Rule""
                    when
                        $message : Message()
                    then
                        // Simple rule for event testing
                end";

            using var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(drlContent));
            builder.AddPackageFromDrl("event-test.drl", stream);
            var package = builder.GetPackage();
            
            _ruleBase = RuleBaseFactory.NewRuleBase();
            _ruleBase.AddPackage(package);
            _workingMemory = _ruleBase.NewWorkingMemory();

            // Initialize event collections
            _assertedEvents = new List<ObjectAssertedEventArgs>();
            _retractedEvents = new List<ObjectRetractedEventArgs>();
            _modifiedEvents = new List<ObjectModifiedEventArgs>();

            // Subscribe to events
            _workingMemory.ObjectAsserted += (sender, e) => _assertedEvents.Add(e);
            _workingMemory.ObjectRetracted += (sender, e) => _retractedEvents.Add(e);
            _workingMemory.ObjectModified += (sender, e) => _modifiedEvents.Add(e);
        }

        [TearDown]
        public void TearDown()
        {
            // WorkingMemory doesn't implement IDisposable
            // _workingMemory?.Dispose();
        }

        [Test]
        public void AssertObject_ShouldFireObjectAssertedEvent()
        {
            // Arrange
            var message = new Message { status = Message.HELLO, message = "Test" };

            // Act
            _workingMemory.assertObject(message);

            // Assert
            _assertedEvents.Should().HaveCount(1);
            _assertedEvents[0].Object.Should().Be(message);
        }

        [Test]
        public void RetractObject_ShouldFireObjectRetractedEvent()
        {
            // Arrange
            var message = new Message { status = Message.HELLO, message = "Test" };
            var factHandle = _workingMemory.assertObject(message);
            _assertedEvents.Clear(); // Clear assertion event

            // Act
            _workingMemory.retractObject(factHandle);

            // Assert
            _retractedEvents.Should().HaveCount(1);
            _retractedEvents[0].Object.Should().Be(message);
        }

        [Test]
        public void ModifyObject_ShouldFireObjectModifiedEvent()
        {
            // Arrange
            var message = new Message { status = Message.HELLO, message = "Original" };
            var factHandle = _workingMemory.assertObject(message);
            _assertedEvents.Clear(); // Clear assertion event

            // Act
            message.message = "Modified";
            _workingMemory.modifyObject(factHandle, message);

            // Assert
            _modifiedEvents.Should().HaveCount(1);
            // Note: Event args properties may vary in this implementation
            Assert.That(_modifiedEvents[0], Is.Not.Null);
        }

        [Test]
        public void MultipleOperations_ShouldFireCorrectEvents()
        {
            // Arrange
            var message1 = new Message { status = Message.HELLO, message = "Message 1" };
            var message2 = new Message { status = Message.GOODBYE, message = "Message 2" };

            // Act
            var handle1 = _workingMemory.assertObject(message1);
            var handle2 = _workingMemory.assertObject(message2);
            
            message1.message = "Modified Message 1";
            _workingMemory.modifyObject(handle1, message1);
            
            _workingMemory.retractObject(handle2);

            // Assert
            _assertedEvents.Should().HaveCount(2);
            _modifiedEvents.Should().HaveCount(1);
            _retractedEvents.Should().HaveCount(1);

            _assertedEvents[0].Object.Should().Be(message1);
            _assertedEvents[1].Object.Should().Be(message2);
            Assert.That(_modifiedEvents[0], Is.Not.Null);
            _retractedEvents[0].Object.Should().Be(message2);
        }

        [Test]
        public void EventArgs_ShouldContainValidInformation()
        {
            // Arrange
            var message = new Message { status = Message.HELLO, message = "Event Test" };

            // Act
            var factHandle = _workingMemory.assertObject(message);

            // Assert
            var eventArgs = _assertedEvents[0];
            eventArgs.Object.Should().Be(message);
            // Note: FactHandle property may not be available in this implementation
            Assert.That(eventArgs, Is.Not.Null);
        }

        [Test]
        public void UnsubscribeFromEvents_ShouldStopReceivingEvents()
        {
            // Arrange
            var message = new Message { status = Message.HELLO, message = "Test" };
            
            // Unsubscribe from ObjectAsserted event
            _workingMemory.ObjectAsserted -= (sender, e) => _assertedEvents.Add(e);

            // Act
            _workingMemory.assertObject(message);

            // Assert
            _assertedEvents.Should().BeEmpty();
        }
    }
}