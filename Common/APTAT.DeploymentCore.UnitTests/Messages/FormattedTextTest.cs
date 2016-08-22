using System;
using System.Globalization;
using APTAT.DeploymentCore.Messages;
using Telerik.JustMock;
using Xunit;

namespace APTAT.DeploymentCore.UnitTests.Messages
{
    /// <summary>
    ///This is a test class for FormattedTextTest and is intended
    ///to contain all FormattedTextTest Unit Tests
    ///</summary>
    public class FormattedTextTest
    {
        
        public FormattedTextTest()
        {
        }

        /// <summary>
        ///A test for ToString
        ///</summary>
        [Fact]
        public void ToStringWithoutArgumentsTest()
        {
            ResourceText format = new ResourceText("id", "text");
            object[] arguments = new object[] { };
            FormattedText target = new FormattedText(format, arguments);
            string expected = "text";
            string actual = target.ToString();
            Assert.Equal(expected, actual);
        }

        /// <summary>
        ///A test for ToString
        ///</summary>
        [Fact]
        public void ToStringWithArgumentsTest()
        {
            ResourceText format = new ResourceText("id", "Text contans int '{0}' and string '{1}'");
            object[] arguments = new object[] { 2, "Virus" };
            FormattedText target = new FormattedText(format, arguments);
            string expected = "Text contans int '2' and string 'Virus'";
            string actual = target.ToString();
            Assert.Equal(expected, actual);
        }

        /// <summary>
        ///A test for ToString
        ///</summary>
        [Fact]
        public void ToStringWithNullIMessageTranslatorCollection()
        {
            ResourceText format = new ResourceText("id", "Text contans int '{0}' and string '{1}'");
            object[] arguments = new object[] { 2, "Virus" };
            FormattedText target = new FormattedText(format, arguments);
            string expected = "Text contans int '2' and string 'Virus'";
            string actual = target.ToString((IMessageTranslatorCollection)null);
            Assert.Equal(expected, actual);
        }


        /// <summary>
        ///A test for ToString
        ///</summary>
        [Fact]
        public void ToStringWithWrongArgumentsTest()
        {
            ResourceText format = new ResourceText("id", "Text contans int '{0}' and string '{1}'");
            object[] arguments = new object[] { 2 };
            FormattedText target = new FormattedText(format, arguments);
            string expected = "Text contans int '{0}' and string '{1}'";
            string actual = target.ToString();
            Assert.Equal(expected, actual);
        }

        /// <summary>
        ///A test for ToString
        ///</summary>
        [Fact]
        public void ToStringWithNestedFormattedTextArgumentsTest()
        {
            ResourceText title = new ResourceText("WarningTite", "Warning");
            FormattedText message = new FormattedText(
                new ResourceText("VirusFound", "The virus '{0}' has been found!"),
                "Win.CernobylWorm.32");

            ResourceText format = new ResourceText("MessageWrapper",
                "You have received at {0} the message with title '{1}' and text:\r\n{2}");
            object[] arguments = { "12:10", title, message };
            FormattedText target = new FormattedText(format, arguments);

            string expected = "You have received at 12:10 the message with title 'Warning' and text:\r\n"
                + "The virus 'Win.CernobylWorm.32' has been found!";
            string actual = target.ToString();
            Assert.Equal(expected, actual);
        }


        /// <summary>
        ///A test for ToString
        ///</summary>
        [Fact]
        public void ToStringWithWrongTranslationTest()
        {
            ResourceText format = new ResourceText("MessageWrapper",
                "You have received at {0} the message with title '{1}' and text:\r\n{2}");
            object[] arguments = { "12:10", "Warning", "Virus found" };
            FormattedText target = new FormattedText(format, arguments);

            string expected = "You have received at 12:10 the message with title 'Warning' and text:\r\n"
                + "Virus found";

            IMessageTranslator antivirusRussianTranslator = Mock.Create<IMessageTranslator>(Behavior.Strict);
            
            Mock.Arrange(() =>antivirusRussianTranslator.Translate("MessageWrapper", Arg.IsAny<CultureInfo>()))
                .Returns("Вы получили сообщение в {10} с заголовком '{1}' и текстом:\r\n{2}"); 
            // transaletion has wrong format for the first argument
            IMessageTranslatorCollection russianTranslators = Mock.Create<IMessageTranslatorCollection>(Behavior.Strict);
            Mock.Arrange(() => russianTranslators[""]).Returns(antivirusRussianTranslator);


            string actual = target.ToString(russianTranslators);
            Assert.Equal(expected, actual);
        }


        /// <summary>
        ///A test for ToString
        ///</summary>
        [Fact]
        public void ToStringWithNestedFormattedTextArgumentsAndTranslationTest()
        {
            ResourceText title = new ResourceText("WarningTite", "Warning");
            FormattedText message = new FormattedText(
                new ResourceText("VirusFound", "The virus '{0}' has been found!"),
                "Win.CernobylWorm.32");

            ResourceText format = new ResourceText("MessageWrapper",
                "You have received at {0} the message with title '{1}' and text:\r\n{2}");
            object[] arguments = { "12:10", title, message };
            FormattedText target = new FormattedText(format, arguments);

            string expected = "Вы получили сообщение в 12:10 с заголовком 'Предупреждение' и текстом:\r\n"
                + "Найден вирус 'Win.CernobylWorm.32'!";

            IMessageTranslator antivirusRussianTranslator =
                Mock.Create<IMessageTranslator>(Behavior.Strict);
            Mock.Arrange(() => antivirusRussianTranslator.Translate("WarningTite", Arg.IsAny<CultureInfo>()))
                .Returns("Предупреждение").MustBeCalled();
            Mock.Arrange(() => antivirusRussianTranslator.Translate("VirusFound", Arg.IsAny<CultureInfo>()))
                .Returns("Найден вирус '{0}'!").MustBeCalled();
            Mock.Arrange(() => antivirusRussianTranslator.Translate("MessageWrapper", Arg.IsAny<CultureInfo>()))
                .Returns(
                "Вы получили сообщение в {0} с заголовком '{1}' и текстом:\r\n{2}").MustBeCalled();
            IMessageTranslatorCollection russianTranslators = Mock.Create<IMessageTranslatorCollection>();
            Mock.Arrange(() => russianTranslators[""]).Returns(antivirusRussianTranslator);



            string actual = target.ToString(russianTranslators);
            Assert.Equal(expected, actual);
        }


        /// <summary>
        ///A test for ToString
        ///</summary>
        [Fact]
        public void ToStringWithNestedFormattedTextArgumentsAndTranslationFromDifferentSourcesTest()
        {
            ResourceText title = new ResourceText("ServerSide", "WarningTite", "Warning");
            FormattedText message = new FormattedText(
                new ResourceText("CIS", "VirusFound", "The virus '{0}' has been found!"),
                "Win.CernobylWorm.32");

            ResourceText format = new ResourceText("ServerSide", "MessageWrapper",
                "You have received at {0} the message with title '{1}' and text:\r\n{2}");
            object[] arguments = { "12:10", title, message };
            FormattedText target = new FormattedText(format, arguments);

            string expected = "Вы получили сообщение в 12:10 с заголовком 'Предупреждение' и текстом:\r\n"
                + "Найден вирус 'Win.CernobylWorm.32'!";

            var cisRussianTranslator = Mock.Create<IMessageTranslator>(Behavior.Strict);
            Mock.Arrange(() => cisRussianTranslator.Translate("VirusFound", Arg.IsAny<CultureInfo>()))
                .Returns("Найден вирус '{0}'!").MustBeCalled();
            
            var serverRussianTranslator = Mock.Create<IMessageTranslator>(Behavior.Strict);
            Mock.Arrange(() => serverRussianTranslator.Translate("WarningTite", Arg.IsAny<CultureInfo>()))
                .Returns("Предупреждение").MustBeCalled();
            Mock.Arrange(() => serverRussianTranslator.Translate("MessageWrapper", Arg.IsAny<CultureInfo>()))
                .Returns("Вы получили сообщение в {0} с заголовком '{1}' и текстом:\r\n{2}")
                .MustBeCalled();

            var russianTranslators = Mock.Create<IMessageTranslatorCollection>(Behavior.Strict);
            Mock.Arrange(() => russianTranslators["CIS"]).Returns(cisRussianTranslator);
            Mock.Arrange(() => russianTranslators["ServerSide"]).Returns(serverRussianTranslator);
            


            string actual = target.ToString(russianTranslators);
            Assert.Equal(expected, actual);
        }

        /// <summary>
        ///A test for ToString
        ///</summary>
        [Fact]
        public void ToStringWithNestedFormattedTextArgumentsAndTranslationFromDifferentSourcesTestNoTranslations()
        {
            ResourceText title = new ResourceText("ServerSide", "WarningTite", "Warning");
            FormattedText message = new FormattedText(
                new ResourceText("CIS", "VirusFound", "The virus '{0}' has been found!"),
                "Win.CernobylWorm.32");

            ResourceText format = new ResourceText("ServerSide", "MessageWrapper",
                "You have received at {0} the message with title '{1}' and text:\r\n{2}");
            object[] arguments = { "12:10", title, message };
            FormattedText target = new FormattedText(format, arguments);

            string expected = "Вы получили сообщение в 12:10 с заголовком 'Предупреждение' и текстом:\r\n"
                + "The virus 'Win.CernobylWorm.32' has been found!";

            IMessageTranslator cisRussianTranslator = Mock.Create<IMessageTranslator>(Behavior.Strict);
            Mock.Arrange(() => cisRussianTranslator.Translate("VirusFound", Arg.IsAny<CultureInfo>()))
                .Returns((string)null).MustBeCalled(); // no translation

            IMessageTranslator serverRussianTranslator = Mock.Create<IMessageTranslator>(Behavior.Strict);
            Mock.Arrange(() => serverRussianTranslator.Translate("WarningTite", Arg.IsAny<CultureInfo>())).Returns("Предупреждение")
                .MustBeCalled();
            Mock.Arrange(() => serverRussianTranslator.Translate("MessageWrapper", Arg.IsAny<CultureInfo>()))
                .Returns("Вы получили сообщение в {0} с заголовком '{1}' и текстом:\r\n{2}").MustBeCalled();

            IMessageTranslatorCollection russianTranslators = Mock.Create<IMessageTranslatorCollection>();
            Mock.Arrange(() => russianTranslators["CIS"]).Returns(cisRussianTranslator);
            Mock.Arrange(() => russianTranslators["ServerSide"]).Returns(serverRussianTranslator);
        

            string actual = target.ToString(russianTranslators);
            Assert.Equal(expected, actual);
        }

        /// <summary>
        ///A test for GetHashCode
        ///</summary>
        [Fact]
        public void GetHashCodeTest()
        {
            FormattedText target = new FormattedText(ResourceText.Empty, new object[] { });
            int expected = ResourceText.Empty.GetHashCode();
            int actual = target.GetHashCode();
            Assert.Equal(expected, actual);
        }

        /// <summary>
        ///A test for Equals
        ///</summary>
        [Fact]
        public void ObjectsEqualTest()
        {
            ResourceText format = new ResourceText("Id", "text {0}");
            object[] arguments = new object[] { 5 };
            FormattedText target = new FormattedText(format, arguments);
            object obj = new FormattedText(format, arguments);

            Assert.True(target.Equals(obj));
        }

        /// <summary>
        ///A test for Equals
        ///</summary>
        [Fact]
        public void ObjectsAreNotEqualTest()
        {
            ResourceText format = new ResourceText("Id", "text {0}");
            object[] arguments = new object[] { 5 };
            FormattedText target = new FormattedText(format, arguments);
            object obj = 5;

            Assert.False(target.Equals(obj));
        }

        /// <summary>
        ///A test for Equals
        ///</summary>
        [Fact]
        public void EqualTest()
        {
            ResourceText format = new ResourceText("Id", "text {0}");
            object[] arguments = new object[] { 5 };
            FormattedText target = new FormattedText(format, arguments);
            FormattedText other = new FormattedText(new ResourceText("Id", "text {0}"), new object[] { 5 });

            Assert.True(target.Equals(other));
        }

        /// <summary>
        ///A test for Equals
        ///</summary>
        [Fact]
        public void AreNotEqualTest()
        {
            ResourceText format = new ResourceText("Id", "text {0}");
            object[] arguments = new object[] { 5 };
            FormattedText target = new FormattedText(format, arguments);
            FormattedText other = new FormattedText(new ResourceText("Id", "text {0}"), new object[] { 5.0 });

            Assert.False(target.Equals(other));
        }


        /// <summary>
        ///A test for FormattedText Constructor and getters
        ///</summary>
        [Fact]
        public void ConstructorAndGettersTest()
        {
            ResourceText format = new ResourceText("id", "text");
            object[] arguments = new object[] { 1, "two" };
            FormattedText target = new FormattedText(format, arguments);

            Assert.Equal(format, target.Format);
            Assert.Equal(arguments.Length, target.Arguments.Length);
            for (int i = 0; i < arguments.Length; i++)
            {
                Assert.Equal(arguments[i], target.Arguments[i]);
            }
        }

        /// <summary>
        ///A test for FormattedText Constructor : formatStr parameter is null
        ///</summary>
        [Fact]
        public void ConstructorThrowsExceptionIfFormatArgIsNullTest()
        {
            ResourceText format = null;
            object[] arguments = new object[] {1, "two"};
            var ex = Assert.Throws<ArgumentNullException>(() => new FormattedText(format, arguments));
            Assert.Equal("format", ex.ParamName);
        }

        /// <summary>
        ///A test for FormattedText Constructor : arguments parameter is null
        ///</summary>
        [Fact]
        public void ConstructorThrowsExceptionIfArgumentsIsNullTest()
        {
            ResourceText format = new ResourceText("id", "text");
            object[] arguments = null;
            var ex = Assert.Throws<ArgumentNullException>(() => new FormattedText(format, arguments));
            Assert.Equal("arguments", ex.ParamName);
        }

        /// <summary>
        ///A test for FormattedText Constructor
        ///</summary>
        [Fact]
        public void ConstructorThrowsExceptionIfArgumentsItemHasWrongTypeTest()
        {
            ResourceText format = new ResourceText("id", "text");
            object[] arguments = { new byte[0] };
            var ex = Assert.Throws<ArgumentException>(() => new FormattedText(format, arguments));
            Assert.Equal("arguments", ex.ParamName);
        }
    }
}
