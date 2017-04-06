using System;
using System.Collections.Generic;
using System.Linq;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Messages;
using Telerik.JustMock;
using Xunit;
using APTAT.Common;

namespace APTAT.DeploymentCore.UnitTests.Descent
{
    public class DescentResultTests
    {
        private readonly Guid Comp1Uid = new Guid("3EC5EB9F-84A9-4A53-9381-9CE1CB15953D");
        private readonly Guid Comp2Uid = new Guid("8A0CF07D-8994-440B-980F-175938956597");
        private readonly Guid Comp3Uid = new Guid("C08ABED7-21E8-4AE3-B816-1485D1277D0D");
        private readonly IReadOnlyList<Guid> CompUids;
        private readonly Guid UknownCompUid = new Guid("C0813ED7-21E8-4AE3-B816-1485D1277D0D");

        private readonly IComputer Computer1 = Mock.Create<IComputer>(Behavior.Strict);
        private readonly IComputer Computer2 = Mock.Create<IComputer>(Behavior.Strict);
        private readonly IComputer Computer3 = Mock.Create<IComputer>(Behavior.Strict);

        private readonly List<IComputer> Targets;
        private readonly IDevicesSourceDescription _deviceSourceDescription;

        private readonly IComputerDescentResultFactory _computerDescentResultFactory;
        private readonly IDescentWatcherFactory _descentWatcherFactory;
        private readonly IDescentWatcher _descentWatcher;


        public DescentResultTests()
        {
            Mock.Arrange(() => Computer1.Uid).Returns(Comp1Uid);
            Mock.Arrange(() => Computer2.Uid).Returns(Comp2Uid);
            Mock.Arrange(() => Computer3.Uid).Returns(Comp3Uid);
            Mock.Arrange(() => Computer1.Name).Returns("comp1");
            Mock.Arrange(() => Computer2.Name).Returns("comp2");
            Mock.Arrange(() => Computer3.Name).Returns("comp3");
            _descentWatcher = Mock.Create<IDescentWatcher>();
            _descentWatcherFactory = Mock.Create<IDescentWatcherFactory>(Behavior.Strict);
            Mock.Arrange(() => _descentWatcherFactory.CreateDescentWatcher(Arg.IsAny<Action<Guid>>(), Arg.IsAny<IScheduler>()))
                .Returns(_descentWatcher);
            _computerDescentResultFactory = Mock.Create<IComputerDescentResultFactory>(Behavior.Strict);
            Mock.Arrange(() => _computerDescentResultFactory.CreateComputerDescentResult(Arg.IsAny<IComputer>(), 
                Arg.AnyGuid,
                Arg.IsAny<ITimeStampProvider>()))
                .Returns((IComputer c, Guid operatoinUid, ITimeStampProvider timeStampProvider) =>
                {
                    var r = Mock.Create<IComputerDescentResultInternal>(Behavior.Strict);
                    TimeSpan modified = TimeSpan.Zero;
                    var completed = false;
                    var succeeded = false;
                    //Mock.Arrange(() => r.UpdateStatus(Arg.IsAny<IOperationDescentResult>()))
                    //    .DoInstead((IOperationDescentResult result) =>
                    //    {
                    //        modified = timeStampProvider.GetCurretTimestamp();
                    //        completed = true;
                    //        succeeded = result.Succeeded;
                    //    });
                    Mock.Arrange(() => r.Succeeded).Returns(() => succeeded);
                    Mock.Arrange(() => r.Completed).Returns(() => completed);
                    Mock.Arrange(() => r.Modified).Returns(() => modified);
                    Mock.Arrange(() => r.Computer).Returns(c);
                    return r;
                });

            _deviceSourceDescription = Mock.Create<IDevicesSourceDescription>(Behavior.Strict);
            Targets = new List<IComputer> { Computer1, Computer2, Computer3 };
            CompUids = new List<Guid> { Comp1Uid, Comp2Uid, Comp3Uid };
        }


        [Fact]
        public void ConstructorTest()
        {
            var descentWatcher = Mock.Create<IDescentWatcher>(Behavior.Strict);
            Mock.Arrange(() => descentWatcher.Start());
            foreach (var compUid in CompUids)
            {
                Mock.Arrange(() => descentWatcher.AddIdentity(compUid));
            }
            var descentWatcherFactory = Mock.Create<IDescentWatcherFactory>(Behavior.Strict);
            Mock.Arrange(() => descentWatcherFactory.CreateDescentWatcher(Arg.IsAny<Action<Guid>>(),  Arg.IsAny<IScheduler>()))
                .Returns(descentWatcher);
            var target = new DescentResult(Targets, _deviceSourceDescription,
                _computerDescentResultFactory, descentWatcherFactory);
            foreach(var compUid in CompUids)
            {
                Mock.Assert(() => descentWatcher.AddIdentity(compUid), Occurs.Once());
            }
            Mock.Assert(() => descentWatcher.Start(), Occurs.Once());
            Assert.Equal(Targets.Count, target.TotalTargetsCount);
            Assert.NotEqual(Guid.Empty, target.OperationUid);
            Assert.Equal(_deviceSourceDescription, target.DevicesSourceDescription);
            Assert.Null(target.CompletedTime);
            Assert.False(target.Completed);
            Assert.Equal(0, target.FailedCount);
            Assert.Equal(0, target.SucceededCount);
            Assert.Equal(3, target.InProgressCount);
            Assert.Equal(3, target.TotalTargetsCount);
            Assert.Equal(3, target.ComputerDescentResults.Count);
        }

        [Fact]
        public void ConstructorShouldRegisterAllComputerInDescentWatcherTest()
        {
            var descentWatcher = Mock.Create<IDescentWatcher>(Behavior.Strict);
            Mock.Arrange(() => descentWatcher.Start()).MustBeCalled();
            Mock.Arrange(() => descentWatcher.AddIdentity(Arg.Matches<Guid>(uid => CompUids.Contains(uid))))
                .Occurs(3);
            var descentWatcherFactory = Mock.Create<IDescentWatcherFactory>(Behavior.Strict);
            Mock.Arrange(() => descentWatcherFactory.CreateDescentWatcher(Arg.IsAny<Action<Guid>>(), Arg.IsAny<IScheduler>()))
                .Returns(descentWatcher);
            var target = new DescentResult(Targets, _deviceSourceDescription, _computerDescentResultFactory, descentWatcherFactory);
            Assert.Equal(Targets.Count, target.TotalTargetsCount);
            Assert.NotEqual(Guid.Empty, target.OperationUid);
            Assert.Equal(_deviceSourceDescription, target.DevicesSourceDescription);
            Assert.Null(target.CompletedTime);
            Assert.False(target.Completed);
            Assert.Equal(0, target.FailedCount);
            Assert.Equal(0, target.SucceededCount);
            Assert.Equal(3, target.InProgressCount);
            Assert.Equal(3, target.TotalTargetsCount);
            Assert.Equal(3, target.ComputerDescentResults.Count);
        }

        [Fact]
        public void OnDescentOperationEventTest()
        {
            Dictionary<Guid, IComputerDescentResultInternal> results;
            var factory = CreateAndArrangeComputerDescentFactory(out results);
            var target = new DescentResult(Targets, _deviceSourceDescription, factory
                , Mock.Create<IDescentWatcherFactory>());
            var errorCodeFormatted = new FormattedText(new ResourceText("", ""));
            var shortStatusFormatted = new FormattedText(new ResourceText("", ""));
            target.OnDescendOperationEvent(true, Computer1.Uid, errorCodeFormatted, shortStatusFormatted, "", 3, "");
            Mock.Assert(() => results[Computer1.Uid].UpdateStatus(Arg.IsAny<IOperationDescentResult>()), Occurs.Once());
            Assert.Null(target.CompletedTime);
            Assert.False(target.Completed);
            Assert.Equal(3, target.TotalTargetsCount);
            Assert.Equal(0, target.FailedCount);
            Assert.Equal(1, target.SucceededCount);
            Assert.Equal(2, target.InProgressCount);
            Assert.Equal(3, target.ComputerDescentResults.Count);
            target.OnDescendOperationEvent(true, Computer2.Uid, errorCodeFormatted, shortStatusFormatted, "", 3, "");
            Assert.Null(target.CompletedTime);
            Assert.False(target.Completed);
            Mock.Assert(() => results[Computer2.Uid].UpdateStatus(Arg.IsAny<IOperationDescentResult>()), Occurs.Once());
            Assert.Equal(2, target.SucceededCount);
            Assert.Equal(0, target.FailedCount);
            Assert.Equal(1, target.InProgressCount);
            Assert.Equal(3, target.TotalTargetsCount);
            Assert.Equal(3, target.ComputerDescentResults.Count);
            target.OnDescendOperationEvent(false, Computer3.Uid, errorCodeFormatted, shortStatusFormatted, "", 3, "");
            Assert.NotNull(target.CompletedTime);
            Mock.Assert(() => results[Computer3.Uid].UpdateStatus(Arg.IsAny<IOperationDescentResult>()), Occurs.Once());
            Assert.True(target.Completed);
            Assert.Equal(2, target.SucceededCount);
            Assert.Equal(1, target.FailedCount);
            Mock.Assert(results[Computer3.Uid]);
            Assert.Equal(0, target.InProgressCount);
            Assert.Equal(3, target.TotalTargetsCount);
            Assert.Equal(3, target.ComputerDescentResults.Count);
        }

        [Theory]
        [InlineData("text1", "text2", "text3", 3, "text4")]
        [InlineData("text1", "", "text3", 4, "text4")]
        [InlineData("text1", "text2", "", 6, "text4")]
        [InlineData("text1", "text2", "", 0, "")]
        [InlineData("", "", "", 3, "")]
        [InlineData("text1", "text2", "", 6, "text4", false)]
        [InlineData("", "", "", 3, "", false)]
        public void OnDescentOperationEventShouldCallUpdateStatusForFailed(string errorMessage, string shortStatus,
            string errorCode, int systemCode, string systemMessage, bool isCancelled = false)
        {
            Dictionary<Guid, IComputerDescentResultInternal> results;
            var factory = CreateAndArrangeComputerDescentFactory(out results);
            var target = new DescentResult(Targets, _deviceSourceDescription, factory
                , Mock.Create<IDescentWatcherFactory>());
            var errorCodeFormatted = new FormattedText(new ResourceText("", errorMessage));
            Assert.True(errorCodeFormatted.ToString() == errorMessage);
            var shortStatusFormatted = new FormattedText(new ResourceText("", shortStatus));
            Assert.True(shortStatusFormatted.ToString() == shortStatus);
            target.OnDescendOperationEvent(false,Computer1.Uid, errorCodeFormatted, shortStatusFormatted, errorCode, systemCode, systemMessage, isCancelled);
            Mock.Assert(() => results[Computer1.Uid].UpdateStatus(Arg.Matches<IOperationDescentResult>(r => r.IsCanceled == isCancelled &&
            r.ErrorCode == errorCode && r.ShortStatusMessage.ToString() == shortStatus && r.SystemCode == systemCode &&
            r.StatusMessage.ToString() == errorMessage && r.SystemMessage == systemMessage && !r.Succeeded)), Occurs.Once());
        }

        [Theory]
        [InlineData("text1", "text2", "text3", 3, "text4")]
        [InlineData("text1", "", "text3", 4, "text4")]
        [InlineData("text1", "text2", "", 6, "text4")]
        [InlineData("text1", "text2", "", 0, "")]
        [InlineData("", "", "", 3, "")]
        [InlineData("text1", "text2", "", 6, "text4", false)]
        [InlineData("", "", "", 3, "", false)]
        public void OnDescentOperationEventShouldCallUpdateStatusForSuccess(string errorMessage, string shortStatus,
            string errorCode, int systemCode, string systemMessage, bool isCancelled = false)
        {
            Dictionary<Guid, IComputerDescentResultInternal> results;
            var factory = CreateAndArrangeComputerDescentFactory(out results);
            var target = new DescentResult(Targets, _deviceSourceDescription, factory
                , Mock.Create<IDescentWatcherFactory>());
            var errorCodeFormatted = new FormattedText(new ResourceText("", errorMessage));
            Assert.True(errorCodeFormatted.ToString() == errorMessage);
            var shortStatusFormatted = new FormattedText(new ResourceText("", shortStatus));
            Assert.True(shortStatusFormatted.ToString() == shortStatus);
            target.OnDescendOperationEvent(true, Computer1.Uid, errorCodeFormatted, shortStatusFormatted, errorCode, systemCode, systemMessage, isCancelled);
            Mock.Assert(() => results[Computer1.Uid].UpdateStatus(Arg.Matches<IOperationDescentResult>(r => r.IsCanceled == isCancelled &&
            r.ErrorCode == "" && r.ShortStatusMessage.ToString() == shortStatus && r.SystemCode == 0 &&
            r.StatusMessage.ToString() == "" && r.SystemMessage == "" && r.Succeeded)), Occurs.Once());
        }

        [Fact]
        public void DescentWatcherTriggedTimeout()
        {
            var descentWatcher = Mock.Create<IDescentWatcher>(Behavior.Strict);
            Mock.Arrange(() => descentWatcher.Start()).MustBeCalled();
            Action<Guid> callbackAction = null;
            Mock.Arrange(() => descentWatcher.AddIdentity(Arg.Matches<Guid>(uid => CompUids.Contains(uid))))
                .Occurs(3);
            Mock.Arrange(() => descentWatcher.ExcludeIdentity(Comp1Uid)).OccursOnce();//.MustBeCalled();
            var descentWatcherFactory = Mock.Create<IDescentWatcherFactory>(Behavior.Strict);
            Mock.Arrange(() => descentWatcherFactory.CreateDescentWatcher(Arg.IsAny<Action<Guid>>(), Arg.IsAny<IScheduler>()))
                .Returns((Action<Guid> action) =>
                {
                    callbackAction = action;
                    return descentWatcher;
                });
            var target = new DescentResult(Targets, _deviceSourceDescription, _computerDescentResultFactory, descentWatcherFactory);
            Assert.NotNull(callbackAction);
            Assert.Equal(Targets.Count, target.TotalTargetsCount);
            Assert.NotEqual(Guid.Empty, target.OperationUid);
            Assert.Equal(_deviceSourceDescription, target.DevicesSourceDescription);
            Assert.Null(target.CompletedTime);
            Assert.False(target.Completed);
            Assert.Equal(0, target.FailedCount);
            Assert.Equal(0, target.SucceededCount);
            Assert.Equal(3, target.InProgressCount);
            Assert.Equal(3, target.TotalTargetsCount);
            Assert.Equal(3, target.ComputerDescentResults.Count);
            callbackAction.Invoke(Comp1Uid);
            Mock.Assert(descentWatcher);
        }

        [Fact]
        public void DescentWatcherWhenTimeoutWasExceededShouldRaiseTimeoutCallback()
        {
            var descentWatcher = Mock.Create<IDescentWatcher>(Behavior.Strict);
            Mock.Arrange(() => descentWatcher.DescentTimeout).Returns(TimeSpan.FromMinutes(1)).OccursOnce();
            Mock.Arrange(() => descentWatcher.Start()).MustBeCalled();
            Action<Guid> callbackAction = null;
            Mock.Arrange(() => descentWatcher.AddIdentity(Arg.Matches<Guid>(uid => CompUids.Contains(uid))))
                .Occurs(3);
            Mock.Arrange(() => descentWatcher.ExcludeIdentity(Comp1Uid)).OccursOnce();//.MustBeCalled();
            var descentWatcherFactory = Mock.Create<IDescentWatcherFactory>(Behavior.Strict);
            Mock.Arrange(() => descentWatcherFactory.CreateDescentWatcher(Arg.IsAny<Action<Guid>>(), Arg.IsAny<IScheduler>()))
                .Returns((Action<Guid> action) =>
                {
                    callbackAction = action;
                    return descentWatcher;
                });
            var target = new DescentResult(Targets, _deviceSourceDescription, _computerDescentResultFactory, descentWatcherFactory);
            Assert.NotNull(callbackAction);
            Assert.Equal(Targets.Count, target.TotalTargetsCount);
            Assert.NotEqual(Guid.Empty, target.OperationUid);
            Assert.Equal(_deviceSourceDescription, target.DevicesSourceDescription);
            Assert.Null(target.CompletedTime);
            Assert.False(target.Completed);
            Assert.Equal(0, target.FailedCount);
            Assert.Equal(0, target.SucceededCount);
            Assert.Equal(3, target.InProgressCount);
            Assert.Equal(3, target.TotalTargetsCount);
            Assert.Equal(3, target.ComputerDescentResults.Count);
            var trigged = false;
            target.RegisterTimeoutCallback(Comp1Uid, timeSpan => trigged = true);
            callbackAction.Invoke(Comp1Uid);
            Assert.True(trigged);
            Mock.Assert(descentWatcher);
        }

        [Fact]
        public void OnDescentOperationSecondCallForSameComputerShouldNotUpdateResults()
        {
            Dictionary<Guid, IComputerDescentResultInternal> results;
            var factory = CreateAndArrangeComputerDescentFactory(out results);
            var target = new DescentResult(Targets, _deviceSourceDescription, factory
                , Mock.Create<IDescentWatcherFactory>());
            var errorCodeFormatted = new FormattedText(new ResourceText("", ""));
            var shortStatusFormatted = new FormattedText(new ResourceText("", ""));
            target.OnDescendOperationEvent(true, Computer1.Uid, errorCodeFormatted, shortStatusFormatted, "", 3, "");
            target.OnDescendOperationEvent(true, Computer2.Uid, errorCodeFormatted, shortStatusFormatted, "", 3, "");
            var ex = Assert.Throws<ArgumentException>(() => 
                target.OnDescendOperationEvent(false, Computer2.Uid, errorCodeFormatted, shortStatusFormatted, "", 3, ""));
            Assert.Equal("computerUid", ex.ParamName);
            Mock.Assert(() => results[Computer1.Uid].UpdateStatus(Arg.IsAny<IOperationDescentResult>()), Occurs.Once());
            Mock.Assert(() => results[Computer2.Uid].UpdateStatus(Arg.IsAny<IOperationDescentResult>()), Occurs.Once());
            Mock.Assert(() => results[Computer3.Uid].UpdateStatus(Arg.IsAny<IOperationDescentResult>()), Occurs.Never());
            Assert.Null(target.CompletedTime);
            Assert.False(target.Completed);
            Assert.Equal(2, target.SucceededCount);
            Assert.Equal(0, target.FailedCount);
            Assert.Equal(1, target.InProgressCount);
            Assert.Equal(3, target.TotalTargetsCount);
            Assert.Equal(3, target.ComputerDescentResults.Count);
        }

        [Fact]
        public void OnDescentOperationEventWithUknownCompUidTest()
        {
            var target = new DescentResult(Targets, _deviceSourceDescription, _computerDescentResultFactory
                , Mock.Create<IDescentWatcherFactory>());
            var errorCodeFormatted = new FormattedText(new ResourceText("", ""));
            var shortStatusFormatted = new FormattedText(new ResourceText("", ""));
            
            var ex = Assert.Throws<InvalidOperationException>(
                () => target.OnDescendOperationEvent(true, UknownCompUid, errorCodeFormatted, shortStatusFormatted,"", 3, ""));
            Assert.Equal("Computer result not found by UID " + UknownCompUid, ex.Message);
        }

        [Fact]
        public void ChangedShouldntReactForAnotherBusinessObject()
        {
            Dictionary<Guid, IComputerDescentResultInternal> results;
            var factory = CreateAndArrangeComputerDescentFactory(out results);
            var descentWatcher = Mock.Create<IDescentWatcher>();
            var descentWatcherFactory = Mock.Create<IDescentWatcherFactory>(Behavior.Strict);
            Mock.Arrange(() => descentWatcherFactory.CreateDescentWatcher(Arg.IsAny<Action<Guid>>(), Arg.IsAny<IScheduler>()))
                .Returns(descentWatcher);
            var target = new DescentResult(Targets, _deviceSourceDescription, factory
                , descentWatcherFactory);
            var item = Mock.Create<Common.IChangesTrackingItem>(Behavior.Strict);
            Mock.Arrange(() => item.ChangeType).Returns(Common.ChangeType.Changed);
            var changedItem = Mock.Create<Common.IBusinessObject>(Behavior.Strict);
            Mock.Arrange(() => item.ChangedItem).Returns(changedItem);
            target.Changed(new Common.IChangesTrackingItem[] { item });
            foreach (var r in results)
            {
                Mock.Assert(() => r.Value.UpdateStatus(Arg.IsAny<IDescentProgressItem>()), Occurs.Never());
            }
            Mock.Assert(() => descentWatcher.NotifyProgress(Arg.AnyGuid), Occurs.Never());
            Mock.Assert(item);
            Mock.Assert(changedItem);
        }
        [Theory]
        [InlineData(Common.ChangeType.Added)]
        [InlineData(Common.ChangeType.Deleted)]
        public void ChangedShouldntReactForNonChangedType(Common.ChangeType changedType)
        {
            Dictionary<Guid, IComputerDescentResultInternal> results;
            var factory = CreateAndArrangeComputerDescentFactory(out results);
            var descentWatcher = Mock.Create<IDescentWatcher>();
            var descentWatcherFactory = Mock.Create<IDescentWatcherFactory>(Behavior.Strict);
            Mock.Arrange(() => descentWatcherFactory.CreateDescentWatcher(Arg.IsAny<Action<Guid>>(), Arg.IsAny<IScheduler>()))
                .Returns(descentWatcher);
            var target = new DescentResult(Targets, _deviceSourceDescription, factory, descentWatcherFactory);
            var item = Mock.Create<Common.IChangesTrackingItem>(Behavior.Strict);
            Mock.Arrange(() => item.ChangeType).Returns(changedType);
            target.Changed(new Common.IChangesTrackingItem[] { item });
            foreach (var r in results)
            {
                Mock.Assert(() => r.Value.UpdateStatus(Arg.IsAny<IDescentProgressItem>()), Occurs.Never());
            }
            Mock.Assert(() => descentWatcher.NotifyProgress(Arg.AnyGuid), Occurs.Never());
            Mock.Assert(item);
        }
        [Fact]
        public void ChangedShouldntReactForDescentProgressWithAnotherUid()
        {
            Dictionary<Guid, IComputerDescentResultInternal> results;
            var factory = CreateAndArrangeComputerDescentFactory(out results);
            var descentWatcher = Mock.Create<IDescentWatcher>();
            var descentWatcherFactory = Mock.Create<IDescentWatcherFactory>(Behavior.Strict);
            Mock.Arrange(() => descentWatcherFactory.CreateDescentWatcher(Arg.IsAny<Action<Guid>>(), Arg.IsAny<IScheduler>()))
                .Returns(descentWatcher);
            var target = new DescentResult(Targets, _deviceSourceDescription, factory
                , descentWatcherFactory);            
            var item = Mock.Create<Common.IChangesTrackingItem>(Behavior.Strict);
            Mock.Arrange(() => item.ChangeType).Returns(Common.ChangeType.Changed);
            var changedItem = Mock.Create<IDescentProgressItem>(Behavior.Strict);
            Mock.Arrange(() => item.ChangedItem).Returns(changedItem);
            Mock.Arrange(() => changedItem.OperationUid).Returns(Guid.NewGuid);
            target.Changed(new Common.IChangesTrackingItem[] { item });
            foreach(var r in results)
            {
                Mock.Assert(() => r.Value.UpdateStatus(Arg.IsAny<IDescentProgressItem>()), Occurs.Never());
            }
            Mock.Assert(() => descentWatcher.NotifyProgress(Arg.AnyGuid), Occurs.Never());
            Mock.Assert(item);
            Mock.Assert(changedItem);
        }
        [Fact]
        public void ChangedShouldntReactForDescentProgressWithEmptyComputerUid()
        {
            Dictionary<Guid, IComputerDescentResultInternal> results;
            var factory = CreateAndArrangeComputerDescentFactory(out results);
            var descentWatcher = Mock.Create<IDescentWatcher>();
            var descentWatcherFactory = Mock.Create<IDescentWatcherFactory>(Behavior.Strict);
            Mock.Arrange(() => descentWatcherFactory.CreateDescentWatcher(Arg.IsAny<Action<Guid>>(), Arg.IsAny<IScheduler>()))
                .Returns(descentWatcher);
            var target = new DescentResult(Targets, _deviceSourceDescription, factory
                , descentWatcherFactory);
            var item = Mock.Create<Common.IChangesTrackingItem>(Behavior.Strict);
            Mock.Arrange(() => item.ChangeType).Returns(Common.ChangeType.Changed);
            var changedItem = Mock.Create<IDescentProgressItem>(Behavior.Strict);
            Mock.Arrange(() => changedItem.ComputerUid).Returns(Guid.Empty);
            Mock.Arrange(() => item.ChangedItem).Returns(changedItem);
            Mock.Arrange(() => changedItem.OperationUid).Returns(target.OperationUid);
            target.Changed(new Common.IChangesTrackingItem[] { item });
            foreach (var r in results)
            {
                Mock.Assert(() => r.Value.UpdateStatus(Arg.IsAny<IDescentProgressItem>()), Occurs.Never());
            }
            Mock.Assert(() => descentWatcher.NotifyProgress(Arg.AnyGuid), Occurs.Never());
            Mock.Assert(item);
            Mock.Assert(changedItem);
        }
        [Theory]
        [InlineData(0.0)]
        [InlineData(20.0)]
        [InlineData(80.0)]
        public void ChangedShouldntUpdateForDescentProgressWithSamePercentagesAndStatus(double percentage)
        {
            Dictionary<Guid, IComputerDescentResultInternal> results;
            var factory = CreateAndArrangeComputerDescentFactory(out results);
            var descentWatcher = Mock.Create<IDescentWatcher>();
            var descentWatcherFactory = Mock.Create<IDescentWatcherFactory>(Behavior.Strict);
            Mock.Arrange(() => descentWatcherFactory.CreateDescentWatcher(Arg.IsAny<Action<Guid>>(), Arg.IsAny<IScheduler>()))
                .Returns(descentWatcher);
            var target = new DescentResult(Targets, _deviceSourceDescription, factory
                , descentWatcherFactory);
            var item = Mock.Create<Common.IChangesTrackingItem>(Behavior.Strict);
            Mock.Arrange(() => item.ChangeType).Returns(Common.ChangeType.Changed);
            var changedItem = Mock.Create<IDescentProgressItem>(Behavior.Strict);
            Mock.Arrange(() => changedItem.ComputerUid).Returns(Comp1Uid);
            Mock.Arrange(() => results[Comp1Uid].StatusMessage).Returns(new FormattedText(new ResourceText("", "")));
            Mock.Arrange(() => results[Comp1Uid].Percentage).Returns(percentage);
            Mock.Arrange(() => changedItem.ProgressPercentage).Returns(percentage);
            Mock.Arrange(() => changedItem.StatusMessage)
                .Returns(new FormattedText(new ResourceText("", "")));
            Mock.Arrange(() => item.ChangedItem).Returns(changedItem);
            Mock.Arrange(() => changedItem.OperationUid).Returns(target.OperationUid);
            target.Changed(new Common.IChangesTrackingItem[] { item });
            foreach (var r in results)
            {
                Mock.Assert(() => r.Value.UpdateStatus(Arg.IsAny<IDescentProgressItem>()), Occurs.Never());
            }
            Mock.Assert(() => descentWatcher.NotifyProgress(Arg.AnyGuid), Occurs.Once());
            Mock.Assert(item);
            Mock.Assert(changedItem);
        }
        [Theory]
        [InlineData(0.0)]
        [InlineData(20.0)]
        [InlineData(80.0)]
        public void ChangedShouldNotifyForDescentProgressWithSamePercentagesAndStatus(double percentage)
        {
            Dictionary<Guid, IComputerDescentResultInternal> results;
            var factory = CreateAndArrangeComputerDescentFactory(out results);
            var descentWatcher = Mock.Create<IDescentWatcher>();
            var descentWatcherFactory = Mock.Create<IDescentWatcherFactory>(Behavior.Strict);
            Mock.Arrange(() => descentWatcherFactory.CreateDescentWatcher(Arg.IsAny<Action<Guid>>(),Arg.IsAny<IScheduler>()))
                .Returns(descentWatcher);



            var target = new DescentResult(Targets, _deviceSourceDescription, factory
                , descentWatcherFactory);
            var item = Mock.Create<Common.IChangesTrackingItem>(Behavior.Strict);
            Mock.Arrange(() => item.ChangeType).Returns(Common.ChangeType.Changed);
            var changedItem = Mock.Create<IDescentProgressItem>(Behavior.Strict);
            Mock.Arrange(() => changedItem.ComputerUid).Returns(Comp1Uid);
            Mock.Arrange(() => results[Comp1Uid].StatusMessage).Returns(new FormattedText(new ResourceText("", "")));
            Mock.Arrange(() => results[Comp1Uid].Percentage).Returns(percentage);
            Mock.Arrange(() => changedItem.ProgressPercentage).Returns(percentage);
            Mock.Arrange(() => changedItem.StatusMessage)
                .Returns(new FormattedText(new ResourceText("", "")));
            Mock.Arrange(() => item.ChangedItem).Returns(changedItem);
            Mock.Arrange(() => changedItem.OperationUid).Returns(target.OperationUid);
            target.Changed(new Common.IChangesTrackingItem[] { item });
            foreach (var r in results)
            {
                Mock.Assert(() => r.Value.UpdateStatus(Arg.IsAny<IDescentProgressItem>()), Occurs.Never());
            }
            Mock.Assert(() => descentWatcher.NotifyProgress(Arg.AnyGuid), Occurs.Once());
            Mock.Assert(item);
            Mock.Assert(changedItem);
        }
        [Fact]
        public void ChangedShouldntReactForDescentProgressWithNonExistingComputerUid()
        {
            Dictionary<Guid, IComputerDescentResultInternal> results;
            var factory = CreateAndArrangeComputerDescentFactory(out results);
            var descentWatcher = Mock.Create<IDescentWatcher>();
            var descentWatcherFactory = Mock.Create<IDescentWatcherFactory>(Behavior.Strict);
            Mock.Arrange(() => descentWatcherFactory.CreateDescentWatcher(Arg.IsAny<Action<Guid>>(), Arg.IsAny<IScheduler>()))
                .Returns(descentWatcher);
            var target = new DescentResult(Targets, _deviceSourceDescription, factory
                , descentWatcherFactory);
            var item = Mock.Create<Common.IChangesTrackingItem>(Behavior.Strict);
            Mock.Arrange(() => item.ChangeType).Returns(Common.ChangeType.Changed);
            var changedItem = Mock.Create<IDescentProgressItem>(Behavior.Strict);
            Mock.Arrange(() => changedItem.ComputerUid).Returns(Guid.NewGuid);
            Mock.Arrange(() => item.ChangedItem).Returns(changedItem);
            Mock.Arrange(() => changedItem.OperationUid).Returns(target.OperationUid);
            target.Changed(new Common.IChangesTrackingItem[] { item });
            foreach (var r in results)
            {
                Mock.Assert(() => r.Value.UpdateStatus(Arg.IsAny<IDescentProgressItem>()), Occurs.Never());
            }
            Mock.Assert(item);
            Mock.Assert(changedItem);
        }
        [Theory]
        [InlineData(0.0, 10.0)]
        [InlineData(20.0, 30.0)]
        [InlineData(80.0, 32.0)]
        public void ChangedShouldUpdateIfPercentageDiffers(double prevPercentage, double newPercentage)
        {
            Dictionary<Guid, IComputerDescentResultInternal> results;
            var factory = CreateAndArrangeComputerDescentFactory(out results);
            var descentWatcher = Mock.Create<IDescentWatcher>();
            var descentWatcherFactory = Mock.Create<IDescentWatcherFactory>(Behavior.Strict);
            Mock.Arrange(() => descentWatcherFactory.CreateDescentWatcher(Arg.IsAny<Action<Guid>>(), Arg.IsAny<IScheduler>()))
                .Returns(descentWatcher);
            var target = new DescentResult(Targets, _deviceSourceDescription, factory
                , descentWatcherFactory);
            var item = Mock.Create<Common.IChangesTrackingItem>(Behavior.Strict);
            Mock.Arrange(() => item.ChangeType).Returns(Common.ChangeType.Changed);
            var changedItem = Mock.Create<IDescentProgressItem>(Behavior.Strict);
            Mock.Arrange(() => changedItem.ComputerUid).Returns(Comp1Uid);
            Mock.Arrange(() => results[Comp1Uid].StatusMessage).Returns(new FormattedText(new ResourceText("", "")));
            Mock.Arrange(() => results[Comp1Uid].Percentage).Returns(prevPercentage);
            Mock.Arrange(() => changedItem.ProgressPercentage).Returns(newPercentage);
            Mock.Arrange(() => changedItem.StatusMessage)
                .Returns(new FormattedText(new ResourceText("", "")));
            Mock.Arrange(() => item.ChangedItem).Returns(changedItem);
            Mock.Arrange(() => changedItem.OperationUid).Returns(target.OperationUid);
            Mock.Arrange(() => results[Comp1Uid].UpdateStatus(Arg.Matches<IDescentProgressItem>
                    (c => Object.ReferenceEquals(c, changedItem))));
            target.Changed(new Common.IChangesTrackingItem[] { item });

            Mock.Assert(() => results[Comp1Uid].UpdateStatus(Arg.Matches<IDescentProgressItem>
                    (c => Object.ReferenceEquals(c, changedItem))), Occurs.Once());
            Mock.Assert(() => descentWatcher.NotifyProgress(Arg.AnyGuid), Occurs.Once());
            Mock.Assert(item);
        }

        //public static IEnumerable<object[]> GetOperationResultsEnumerable()
        //{
        //    yield return new object[] { true, Computer1.Uid, "", "", "", 3, ""  };
        //    yield return new object[] { true, Computer2.Uid, "", "", "", 3, "" };
        //    yield return new object[] { true, Computer3.Uid, "", "", "", 3, "" };
        //}

        private static IComputerDescentResultFactory CreateAndArrangeComputerDescentFactory(out Dictionary<Guid, IComputerDescentResultInternal> createdResults)
        {
            var resultsInternal = createdResults = new Dictionary<Guid, IComputerDescentResultInternal>();
            var computerDescentResultFactory = Mock.Create<IComputerDescentResultFactory>(Behavior.Strict);
            Mock.Arrange(() => computerDescentResultFactory.CreateComputerDescentResult(Arg.IsAny<IComputer>(),
                Arg.AnyGuid,
                Arg.IsAny<ITimeStampProvider>()))
                .Returns((IComputer c, Guid operatoinUid, ITimeStampProvider timeStampProvider) =>
                {
                    var r = Mock.Create<IComputerDescentResultInternal>(Behavior.Strict);
                    resultsInternal.Add(c.Uid, r);
                    TimeSpan modified = TimeSpan.Zero;
                    var completed = false;
                    var succeeded = false;
                    Mock.Arrange(() => r.UpdateStatus(Arg.IsAny<IOperationDescentResult>()))
                        .DoInstead((IOperationDescentResult result) =>
                        {
                            modified = timeStampProvider.GetCurretTimestamp();
                            completed = true;
                            succeeded = result.Succeeded;
                        }).OccursOnce();
                    Mock.Arrange(() => r.Succeeded).Returns(() => succeeded);
                    Mock.Arrange(() => r.Completed).Returns(() => completed);
                    Mock.Arrange(() => r.Modified).Returns(() => modified);
                    Mock.Arrange(() => r.Computer).Returns(c);
                    return r;
                });
            return computerDescentResultFactory;
        }
    }
}
