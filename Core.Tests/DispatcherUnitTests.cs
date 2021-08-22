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
            bool anyDispatcherExecuted = false;
            Dispatcher.Dispatch(() => anyDispatcherExecuted = true);
            Assert.IsTrue(anyDispatcherExecuted, "The dispatcher has not been executed.");
            AssertDispatcherExecution();
        }

        [TestMethod]
        public void ViewModelCommand()
        {
            new TestCommand().RaiseCanExecuteChanged();
            AssertDispatcherExecution();
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
            var rootViewModel = new TestViewModel();
            var childViewModel = new TestViewModel();

            rootViewModel.Children.Add(childViewModel);
            AssertDispatcherExecution();

            rootViewModel.Children.Remove(childViewModel);
            AssertDispatcherExecution();

            rootViewModel.Children.AddRange(new[] { new TestViewModel(), new TestViewModel() });
            AssertDispatcherExecution();

            rootViewModel.Children.Replace(new[] { new TestViewModel(), new TestViewModel(), new TestViewModel() });
            AssertDispatcherExecution();

            rootViewModel.Children.Insert(0, new TestViewModel());
            AssertDispatcherExecution();

            rootViewModel.Children.RemoveAt(0);
            AssertDispatcherExecution();

            rootViewModel.Children.Clear();
            AssertDispatcherExecution();
        }

        public class TestViewModel : ViewModel
        {
            public ObservableViewModelCollection<TestViewModel> Children { get; set; }

            public TestViewModel()
            {
                Children = new ObservableViewModelCollection<TestViewModel>(this);
            }
        }

        static void AssertDispatcherExecution()
        {
            Assert.IsTrue(TestDispatcherExecuted, $"{nameof(TestDispatcher)} has not been executed.");
            TestDispatcherExecuted = false;
        }

        static bool TestDispatcherExecuted { get; set; }

        [TestCleanup]
        public void Cleanup()
        {
            TestDispatcherExecuted = false;
        }

        public class TestDispatcher : Dispatcher
        {
            protected override void DispatchImpl(Action action)
            {
                Assert.IsNotNull(action);
                action.Invoke();
                TestDispatcherExecuted = true;
            }
        }
    }
}