using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using org.drools.dotnet.compiler;
using org.drools.dotnet.rule;
using org.drools.dotnet.tests.Models;

namespace org.drools.dotnet.tests.Integration
{
    [TestFixture]
    public class EndToEndTests
    {
        [Test]
        public void CompleteWorkflow_FromDrlToExecution_ShouldWork()
        {
            // Arrange - Create a complete rule
            var drlContent = @"
                package org.drools.dotnet.tests

                rule ""Age Classification""
                    when
                        $person : Person( age >= 18 )
                    then
                        $person.adult = true;
                        update($person);
                end

                rule ""Minor Classification""
                    when
                        $person : Person( age < 18 )
                    then
                        $person.adult = false;
                        update($person);
                end";

            // Act - Complete workflow
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(drlContent));
            
            var builder = new PackageBuilder();
            builder.AddPackageFromDrl("age-rules.drl", stream);
            var package = builder.GetPackage();

            var ruleBase = RuleBaseFactory.NewRuleBase();
            ruleBase.AddPackage(package);

            var workingMemory = ruleBase.NewWorkingMemory();

            var adult = new Person("Alice", 25);
            var minor = new Person("Bob", 16);

            workingMemory.assertObject(adult);
            workingMemory.assertObject(minor);
            workingMemory.fireAllRules();

            // Assert
            adult.adult.Should().BeTrue();
            minor.adult.Should().BeFalse();

            // WorkingMemory doesn't implement IDisposable
            // workingMemory.Dispose();
        }

        [Test]
        public void MultipleRulePackages_ShouldWorkTogether()
        {
            // Arrange
            var ageRules = @"
                package org.drools.dotnet.tests.age

                rule ""Adult Check""
                    when
                        $person : Person( age >= 18 )
                    then
                        $person.adult = true;
                        update($person);
                end";

            var nameRules = @"
                package org.drools.dotnet.tests.name

                rule ""Name Length Check""
                    when
                        $person : Person( name.Length > 5 )
                    then
                        System.Console.WriteLine(""Long name: "" + $person.name);
                end";

            // Act
            var builder1 = new PackageBuilder();
            using var stream1 = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(ageRules));
            builder1.AddPackageFromDrl("age-rules.drl", stream1);

            var builder2 = new PackageBuilder();
            using var stream2 = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(nameRules));
            builder2.AddPackageFromDrl("name-rules.drl", stream2);

            var ruleBase = RuleBaseFactory.NewRuleBase();
            ruleBase.AddPackage(builder1.GetPackage());
            ruleBase.AddPackage(builder2.GetPackage());

            var workingMemory = ruleBase.NewWorkingMemory();
            var person = new Person("Alexander", 25);

            workingMemory.assertObject(person);
            workingMemory.fireAllRules();

            // Assert
            person.adult.Should().BeTrue();

            // WorkingMemory doesn't implement IDisposable
            // workingMemory.Dispose();
        }

        [Test]
        public void RuleBaseLoader_EndToEndWorkflow_ShouldWork()
        {
            // Arrange
            var drlContent = @"
                package org.drools.dotnet.tests

                rule ""Message Processing""
                    when
                        $msg : Message( status == Message.HELLO )
                    then
                        $msg.message = ""Processed: "" + $msg.message;
                        update($msg);
                end";

            // Act
            var loader = new RuleBaseLoader();
            var ruleBase = loader.loadFromString(drlContent);
            var workingMemory = ruleBase.NewWorkingMemory();

            var message = new Message
            {
                status = Message.HELLO,
                message = "Test Message"
            };

            workingMemory.assertObject(message);
            workingMemory.fireAllRules();

            // Assert
            message.message.Should().Be("Processed: Test Message");

            // WorkingMemory doesn't implement IDisposable
            // workingMemory.Dispose();
        }

        [Test]
        public void ComplexRuleInteractions_ShouldWorkCorrectly()
        {
            // Arrange - Rules that interact with each other
            var drlContent = @"
                package org.drools.dotnet.tests

                rule ""Initialize Person""
                    when
                        $person : Person( adult == false && age >= 18 )
                    then
                        $person.adult = true;
                        update($person);
                end

                rule ""Adult Message""
                    when
                        $person : Person( adult == true )
                        $message : Message( status == Message.HELLO )
                    then
                        $message.message = ""Hello Adult: "" + $person.name;
                        update($message);
                end";

            // Act
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(drlContent));
            
            var builder = new PackageBuilder();
            builder.AddPackageFromDrl("interaction-rules.drl", stream);
            
            var ruleBase = RuleBaseFactory.NewRuleBase();
            ruleBase.AddPackage(builder.GetPackage());
            
            var workingMemory = ruleBase.NewWorkingMemory();

            var person = new Person("Charlie", 20) { adult = false };
            var message = new Message { status = Message.HELLO, message = "Initial" };

            workingMemory.assertObject(person);
            workingMemory.assertObject(message);
            workingMemory.fireAllRules();

            // Assert
            person.adult.Should().BeTrue();
            message.message.Should().Be("Hello Adult: Charlie");

            // WorkingMemory doesn't implement IDisposable
            // workingMemory.Dispose();
        }

        [Test]
        public void ErrorHandling_InvalidRule_ShouldThrowMeaningfulException()
        {
            // Arrange
            var invalidDrl = @"
                package org.drools.dotnet.tests

                rule ""Invalid Syntax Rule""
                    when
                        invalid syntax here !!!
                    then
                        do something invalid;
                end";

            // Act & Assert
            var builder = new PackageBuilder();
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(invalidDrl));
            
            var exception = Assert.Throws<Exception>(() => 
                builder.AddPackageFromDrl("invalid.drl", stream));
            
            exception.Should().NotBeNull();
            exception.Message.Should().NotBeNullOrEmpty();
        }
    }
}