using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace PresentationBase.Tests
{
    [TestClass]
    public class CommandTests
    {
        [TestMethod]
        public void CommandsDictionary()
        {
            var viewModel = new TestViewModel();
            Assert.AreEqual(1, viewModel.Commands.Count);

            var cmd = viewModel.Commands[typeof(TestCommand)];
            Assert.IsNotNull(cmd);
            Assert.IsInstanceOfType(cmd, typeof(TestCommand));
        }

        [TestMethod]
        public void Execute()
        {
            var viewModel = new TestViewModel();
            TestCommand cmd = (TestCommand)viewModel.Commands[typeof(TestCommand)];
            Assert.IsFalse(cmd.Executed);

            cmd.Execute(viewModel);
            Assert.IsTrue(cmd.Executed);
        }

        [TestMethod]
        public void CanExecute()
        {
            var viewModel = new TestViewModel();
            var cmd = viewModel.Commands[typeof(TestCommand)];
            Assert.IsFalse(cmd.CanExecute(viewModel));

            bool canExecuteChangedRaised = false;
            cmd.CanExecuteChanged += (s, e) =>
            {
                Assert.AreSame(cmd, s);
                canExecuteChangedRaised = true;
            };

            viewModel.Name = "Paul Denton";
            Assert.IsTrue(cmd.CanExecute(viewModel));
            Assert.IsTrue(canExecuteChangedRaised);
            canExecuteChangedRaised = false;

            viewModel.Name = "Paul Denton";
            Assert.IsTrue(cmd.CanExecute(viewModel));
            Assert.IsFalse(canExecuteChangedRaised);

            viewModel.Name = null;
            Assert.IsFalse(cmd.CanExecute(viewModel));
            Assert.IsTrue(canExecuteChangedRaised);
            canExecuteChangedRaised = false;

            cmd.RaiseCanExecuteChanged();
            Assert.IsFalse(cmd.CanExecute(viewModel));
            Assert.IsTrue(canExecuteChangedRaised);
        }

        public class TestViewModel : ViewModel
        {
            private string? _name;

            public string? Name
            {
                get => _name;
                set => SetProperty(ref _name, value);
            }
        }

        public class TestCommand : ViewModelCommand<TestViewModel>
        {
            public bool Executed { get; set; }

            public override void Execute(TestViewModel parameter)
            {
                Executed = true;
            }

            public override bool CanExecute(TestViewModel parameter)
            {
                return base.CanExecute(parameter) && parameter.Name != null;
            }
        }

        public class DummyCommand : ViewModelCommand<ViewModel>
        {
            public override void Execute(ViewModel parameter)
            {
                throw new NotImplementedException();
            }
        }
    }
}