using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace PresentationBase.Tests
{
    [TestClass]
    public class DispatcherUnitTests
    {
        [TestMethod]
        public void Generic()
        {
            var expectedCount = ExecutionCount + 1;
            bool anyDispatcherExecuted = false;
            Dispatcher.Dispatch(() => anyDispatcherExecuted = true);
            Assert.IsTrue(anyDispatcherExecuted, "The dispatcher has not been executed.");
            AssertDispatcherExecution(expectedCount);
        }

        [TestMethod]
        public void ViewModelCommand()
        {
            var expectedCount = ExecutionCount + 1;
            new TestCommand().RaiseCanExecuteChanged();
            AssertDispatcherExecution(expectedCount);
        }

        public class TestCommand : ViewModelCommand<ViewModel>
        {
            public override void Execute(ViewModel parameter)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public void ObservableViewModelCollection()
        {
            var expectedCount = ExecutionCount + 1;
            var rootViewModel = new TestViewModel();
            var childViewModel = new TestViewModel();

            rootViewModel.Children.Add(childViewModel);
            AssertDispatcherExecution(expectedCount);

            expectedCount = ExecutionCount + 1;
            rootViewModel.Children.Remove(childViewModel);
            AssertDispatcherExecution(expectedCount);

            expectedCount = ExecutionCount + 1;
            rootViewModel.Children.AddRange(new[] { new TestViewModel(), new TestViewModel() });
            AssertDispatcherExecution(expectedCount);

            expectedCount = ExecutionCount + 1;
            rootViewModel.Children.Replace(new[] { new TestViewModel(), new TestViewModel(), new TestViewModel() });
            AssertDispatcherExecution(expectedCount);

            expectedCount = ExecutionCount + 1;
            rootViewModel.Children.Insert(0, new TestViewModel());
            AssertDispatcherExecution(expectedCount);

            expectedCount = ExecutionCount + 1;
            rootViewModel.Children.RemoveAt(0);
            AssertDispatcherExecution(expectedCount);

            expectedCount = ExecutionCount + 1;
            rootViewModel.Children.Clear();
            AssertDispatcherExecution(expectedCount);
        }

        public class TestViewModel : ViewModel
        {
            public ObservableViewModelCollection<TestViewModel> Children { get; set; }

            public TestViewModel()
            {
                Children = new ObservableViewModelCollection<TestViewModel>(this);
            }
        }

        static void AssertDispatcherExecution(int expectedCount)
        {
            Assert.IsTrue(ExecutionCount >= expectedCount, $"{nameof(TestDispatcher)} has not been executed.");
        }

        static int ExecutionCount { get; set; }

        [TestCleanup]
        public void Cleanup()
        {
            ExecutionCount = 0;
        }

        public class TestDispatcher : Dispatcher
        {
            protected override void DispatchImpl(Action action)
            {
                Assert.IsNotNull(action);
                action.Invoke();
                ExecutionCount++;
            }
        }
    }
}