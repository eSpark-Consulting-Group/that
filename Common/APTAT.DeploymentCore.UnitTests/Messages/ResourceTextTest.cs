using System;
using APTAT.DeploymentCore.Messages;
using Telerik.JustMock;
using Xunit;

namespace APTAT.DeploymentCore.UnitTests.Messages
{

    /// <summary>
    ///This is a test class for ResourceTextTest and is intended
    ///to contain all ResourceTextTest Unit Tests
    ///</summary>
    public class ResourceTextTest
    {
        public ResourceTextTest()
        {
        }

        /// <summary>
        ///A test for ToString
        ///</summary>
        [Fact]
        public void ToStringTest()
        {
            string resourceId = "resourceId";
            string text = "text";
            ResourceText target = new ResourceText(resourceId, text);
            string expected = "text";
            string actual = target.ToString();
            Assert.Equal(expected, actual);
        }

        /// <summary>
        ///A test for ToString
        ///</summary>
        [Fact]
        public void ToStringWithTranslationTest()
        {
            string resourceId = "resourceId";
            string text = "text";
            ResourceText target = new ResourceText(resourceId, text);
            string expected = "Текст";

            IMessageTranslator antivirusRussionTranslator = Mock.Create<IMessageTranslator>();
            Mock.Arrange(() => antivirusRussionTranslator.Translate(resourceId)).Returns(expected);
            IMessageTranslatorCollection russianTranslators = Mock.Create<IMessageTranslatorCollection>();
            Mock.Arrange(() => russianTranslators[""]).Returns(antivirusRussionTranslator);

            string actual = target.ToString(russianTranslators);
            Assert.Equal(expected, actual);
        }

        /// <summary>
        ///A test for ToString
        ///</summary>
        [Fact]
        public void ToStringWithTranslatorsCollectionTest()
        {
            ResourceText target = new ResourceText("source", "resourceId", "text");
            string expected = "Текст";

            IMessageTranslator antivirusRussionTranslator = Mock.Create<IMessageTranslator>();

            Mock.Arrange(() => antivirusRussionTranslator.Translate("resourceId")).Returns(expected);
            IMessageTranslatorCollection russianTranslators = Mock.Create<IMessageTranslatorCollection>();
            Mock.Arrange(() => russianTranslators["source"]).Returns(antivirusRussionTranslator);

            string actual = target.ToString(russianTranslators);
            Assert.Equal(expected, actual);
        }

        /// <summary>
        ///A test for ToString
        ///</summary>
        [Fact]
        public void ToStringTestWithNullArgIMessageTranslatorCollection()
        {
            string resourceId = "resourceId";
            string text = "text";
            ResourceText target = new ResourceText(resourceId, text);
            string expected = "text";
            string actual = target.ToString((IMessageTranslatorCollection)null);
            Assert.Equal(expected, actual);
        }

        /// <summary>
        ///A test for GetHashCode
        ///</summary>
        [Fact]
        public void GetHashCodeTest()
        {
            string resourceId = string.Empty;
            string text = string.Empty;
            ResourceText target = new ResourceText(resourceId, text);
            int expected = string.Empty.GetHashCode() ^ string.Empty.GetHashCode() ^ string.Empty.GetHashCode();
            int actual = target.GetHashCode();
            Assert.Equal(expected, actual);
        }

        /// <summary>
        ///A test for Equals
        ///</summary>
        [Fact]
        public void EqualTest()
        {
            string resourceId = "resourceId";
            string text = "text";
            ResourceText target = new ResourceText(resourceId, text);
            ResourceText other = new ResourceText(resourceId, text);

            Assert.True(target.Equals(other));
        }

        /// <summary>
        ///A test for Equals
        ///</summary>
        [Fact]
        public void AreNotEqualTest()
        {
            ResourceText target = new ResourceText("Source", "ResourceId", "text");
            ResourceText other = new ResourceText("Source", "resourceId", "text");

            Assert.False(target.Equals(other));
        }


        /// <summary>
        ///A test for Equals
        ///</summary>
        [Fact]
        public void AreNotEqualSourceNameTest()
        {
            ResourceText target = new ResourceText("Source", "resourceId", "text");
            ResourceText other = new ResourceText("SourcE", "resourceId", "text");

            Assert.False(target.Equals(other));
        }

        /// <summary>
        ///A test for Equals
        ///</summary>
        [Fact]
        public void ObjectsEqualTest()
        {
            string resourceId = "resourceId";
            string text = "text";
            object target = new ResourceText(resourceId, text);
            object other = new ResourceText(resourceId, text);

            Assert.True(target.Equals(other));
        }

        /// <summary>
        ///A test for Equals
        ///</summary>
        [Fact]
        public void ObjectsAreNotEqualTest()
        {
            string resourceId = "resourceId";
            string text = "text";
            object target = new ResourceText(resourceId, text);
            string other = "text";

            Assert.False(target.Equals(other));
        }

        /// <summary>
        ///A test for ResourceText Constructor and getters
        ///</summary>
        [Fact]
        public void ConstructorAndGettersTest()
        {
            string resourceId = "resourceId";
            string text = "text";
            ResourceText target = new ResourceText(resourceId, text);

            Assert.Equal(resourceId, target.ResourceId);
            Assert.Equal(text, target.Text);
        }

        /// <summary>
        ///A test for ResourceText Constructor : resourceId parameter is null
        ///</summary>
        [Fact]
        public void ConstructorThrowsExceptionIfResourceIdArgIsNullTest()
        {
            string resourceId = null;
            string text = "text";
            var ex = Assert.Throws<ArgumentNullException>(() => new ResourceText(resourceId, text));
            Assert.Equal("resourceId", ex.ParamName);
        }

        /// <summary>
        ///A test for ResourceText Constructor : text parameter is null
        ///</summary>
        [Fact]
        public void ConstructorThrowsExceptionIfTextArgIsNullTest()
        {
            string resourceId = "resourceId";
            string text = null;
            var ex = Assert.Throws<ArgumentNullException>(() => new ResourceText(resourceId, text));
            Assert.Equal("text", ex.ParamName);
        }
    }
}
