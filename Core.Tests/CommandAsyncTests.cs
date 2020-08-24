using Microsoft.VisualStudio.TestTools.UnitTesting;
using PresentationBase.DtoConverters;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PresentationBase.Tests
{
    [TestClass]
    public class CommandAsyncTests
    {
        [TestMethod]
        public async Task ExecuteAsync()
        {
            var viewModel = new TestViewModel();
            var cmd = (TestCommand)viewModel.Commands[typeof(TestCommand)];
            Assert.IsFalse(cmd.Executed);
            Assert.IsFalse(cmd.IsWorking);
            Assert.IsFalse(cmd.CanExecute(viewModel));

            viewModel.Name = "Bob Page";
            Assert.IsTrue(cmd.CanExecute(viewModel));

            await cmd.ExecuteAsync(viewModel);
            Assert.IsTrue(cmd.Executed);
            Assert.IsFalse(cmd.IsWorking);
        }

        [TestMethod]
        public async Task HandleExceptionAsync()
        {
            var viewModel = new TestViewModel();

            var cmd2 = (Test2Command)viewModel.Commands[typeof(Test2Command)];
            await Assert.ThrowsExceptionAsync<NotImplementedException>(async () => { await cmd2.ExecuteAsync(viewModel); });
        }

        [TestMethod]
        public void HandleExceptionFireAndForget()
        {
            var viewModel = new TestViewModel();

            var cmd2 = (Test2Command)viewModel.Commands[typeof(Test2Command)];
            Assert.IsFalse(cmd2.IsWorking);
            cmd2.Execute(viewModel);

            while (cmd2.IsWorking) { }
            Assert.IsFalse(cmd2.IsWorking);
        }

        [TestMethod]
        public async Task HandleExceptionOverwrittenAsync()
        {
            var viewModel = new TestViewModel();

            var cmd3 = (Test3Command)viewModel.Commands[typeof(Test3Command)];
            Assert.IsFalse(cmd3.ExceptionHandled);
            await Assert.ThrowsExceptionAsync<NotImplementedException>(async () => { await cmd3.ExecuteAsync(viewModel); });
            Assert.IsFalse(cmd3.ExceptionHandled);
            Assert.IsFalse(cmd3.IsWorking);
        }

        [TestMethod]
        public void HandleExceptionOverwrittenFireAndForget()
        {
            var viewModel = new TestViewModel();

            var cmd3 = (Test3Command)viewModel.Commands[typeof(Test3Command)];
            Assert.IsFalse(cmd3.ExceptionHandled);
            cmd3.Execute(viewModel);

            while (cmd3.IsWorking) { }
            Assert.IsTrue(cmd3.ExceptionHandled);
            Assert.IsFalse(cmd3.IsWorking);
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

        public class TestCommand : ViewModelCommandAsync<TestViewModel>
        {
            public bool Executed { get; set; }

            protected override async Task ExecutionAsync(TestViewModel parameter)
            {
                Assert.IsTrue(IsWorking);

                await Task.Run(() =>
                {
                    Assert.IsTrue(IsWorking);
                    Executed = true;
                });

                Assert.IsTrue(IsWorking);
            }

            public override bool CanExecute(TestViewModel parameter)
            {
                return base.CanExecute(parameter) && parameter.Name != null;
            }
        }

        public class Test2Command : ViewModelCommandAsync<TestViewModel>
        {
            protected override async Task ExecutionAsync(TestViewModel parameter)
            {
                Assert.IsTrue(IsWorking);

                await Task.Run(() =>
                {
                    Assert.IsTrue(IsWorking);
                    throw new NotImplementedException();
                });

                Assert.IsTrue(IsWorking);
            }
        }

        public class Test3Command : ViewModelCommandAsync<TestViewModel>
        {
            protected override async Task ExecutionAsync(TestViewModel parameter)
            {
                Assert.IsTrue(IsWorking);

                await Task.Run(() =>
                {
                    Assert.IsTrue(IsWorking);
                    throw new NotImplementedException();
                });

                Assert.IsTrue(IsWorking);
            }

            public bool ExceptionHandled { get; set; }

            protected override void HandleUncaughtException(TestViewModel parameter, Exception ex)
            {
                ExceptionHandled = true;
            }
        }

        public class DummyCommand : ViewModelCommandAsync<ViewModel>
        {
            protected override Task ExecutionAsync(ViewModel parameter)
            {
                throw new NotImplementedException();
            }
        }
    }
}
