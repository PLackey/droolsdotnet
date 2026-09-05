using FluentAssertions;
using NUnit.Framework;
using org.drools.dotnet.evaluator;

namespace org.drools.dotnet.tests.Evaluator
{
    [TestFixture]
    public class EvaluatorFactoryTests
    {
        [Test]
        public void IntegerFactory_GetIntegerEvaluator_ShouldReturnValidEvaluator()
        {
            // Act
            var evaluator = IntegerFactory.getIntegerEvaluator(org.drools.spi.Evaluator.__Fields.EQUAL);

            // Assert
            evaluator.Should().NotBeNull();
        }

        [Test]
        public void StringFactory_GetStringEvaluator_ShouldReturnValidEvaluator()
        {
            // Act
            var evaluator = StringFactory.getStringEvaluator(org.drools.spi.Evaluator.__Fields.EQUAL);

            // Assert
            evaluator.Should().NotBeNull();
        }

        [Test]
        public void BooleanFactory_GetBooleanEvaluator_ShouldReturnValidEvaluator()
        {
            // Act
            var evaluator = BooleanFactory.getBooleanEvaluator(org.drools.spi.Evaluator.__Fields.EQUAL);

            // Assert
            evaluator.Should().NotBeNull();
        }

        [Test]
        public void DoubleFactory_GetDoubleEvaluator_ShouldReturnValidEvaluator()
        {
            // Act
            var evaluator = DoubleFactory.getDoubleEvaluator(org.drools.spi.Evaluator.__Fields.EQUAL);

            // Assert
            evaluator.Should().NotBeNull();
        }

        [Test]
        public void FloatFactory_GetFloatEvaluator_ShouldReturnValidEvaluator()
        {
            // Act
            var evaluator = FloatFactory.getFloatEvaluator(org.drools.spi.Evaluator.__Fields.EQUAL);

            // Assert
            evaluator.Should().NotBeNull();
        }

        [Test]
        public void LongFactory_GetLongEvaluator_ShouldReturnValidEvaluator()
        {
            // Act
            var evaluator = LongFactory.getLongEvaluator(org.drools.spi.Evaluator.__Fields.EQUAL);

            // Assert
            evaluator.Should().NotBeNull();
        }

        [Test]
        public void ByteFactory_GetByteEvaluator_ShouldReturnValidEvaluator()
        {
            // Act
            var evaluator = ByteFactory.getByteEvaluator(org.drools.spi.Evaluator.__Fields.EQUAL);

            // Assert
            evaluator.Should().NotBeNull();
        }

        [Test]
        public void ShortFactory_GetShortEvaluator_ShouldReturnValidEvaluator()
        {
            // Act
            var evaluator = ShortFactory.getShortEvaluator(org.drools.spi.Evaluator.__Fields.EQUAL);

            // Assert
            evaluator.Should().NotBeNull();
        }

        [Test]
        public void CharacterFactory_GetCharacterEvaluator_ShouldReturnValidEvaluator()
        {
            // Act
            var evaluator = CharacterFactory.getCharacterEvaluator(org.drools.spi.Evaluator.__Fields.EQUAL);

            // Assert
            evaluator.Should().NotBeNull();
        }

        [Test]
        public void DateFactory_GetDateEvaluator_ShouldReturnValidEvaluator()
        {
            // Act
            var evaluator = DateFactory.getDateEvaluator(org.drools.spi.Evaluator.__Fields.EQUAL);

            // Assert
            evaluator.Should().NotBeNull();
        }

        [Test]
        public void ObjectFactory_GetObjectEvaluator_ShouldReturnValidEvaluator()
        {
            // Act
            var evaluator = ObjectFactory.getObjectEvaluator(org.drools.spi.Evaluator.__Fields.EQUAL);

            // Assert
            evaluator.Should().NotBeNull();
        }

        [Test]
        public void ArrayFactory_GetArrayEvaluator_ShouldReturnValidEvaluator()
        {
            // Act
            var evaluator = ArrayFactory.getArrayEvaluator(org.drools.spi.Evaluator.__Fields.EQUAL);

            // Assert
            evaluator.Should().NotBeNull();
        }
    }
}