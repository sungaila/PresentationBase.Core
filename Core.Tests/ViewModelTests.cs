using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PresentationBase.Tests
{
    [TestClass]
    public class ViewModelTests
    {
        [TestMethod]
        public void InitialValues()
        {
            var viewModel = new DummyViewModel();

            Assert.IsNull(viewModel.Tag);
            Assert.IsNull(viewModel.ParentViewModel);
            Assert.IsNull(viewModel.RootViewModel);

            Assert.IsFalse(viewModel.IsDirty);
            Assert.IsFalse(viewModel.IsRefreshing);

            Assert.IsFalse(viewModel.HasErrors);
            Assert.IsTrue(viewModel.IsValid);
            Assert.IsFalse(viewModel.HasErrors);
            Assert.IsNotNull(viewModel.GetErrors(string.Empty));
            Assert.AreEqual(0, viewModel.GetErrors(string.Empty).Cast<string>().Count());
            Assert.IsNotNull(viewModel.GetErrors("NotExisting"));
            Assert.AreEqual(0, viewModel.GetErrors("NotExisting").Cast<string>().Count());

            Assert.IsNotNull(viewModel.Commands);
            Assert.AreEqual(0, viewModel.Commands.Count);
        }

        [TestMethod]
        public void Commands()
        {
            var viewModel = new TestViewModel();
            Assert.IsFalse(viewModel.Commands.TryGetValue(typeof(IViewModelCommand), out _));
            Assert.IsFalse(viewModel.Commands.TryGetValue(typeof(DummyCommand), out _));
            Assert.IsTrue(viewModel.Commands.TryGetValue(typeof(TestCommand), out IViewModelCommand? cmd3));
            Assert.IsInstanceOfType(cmd3, typeof(TestCommand));
        }

        [TestMethod]
        public void Properties()
        {
            // check inital values
            var viewModel = new TestViewModel();
            Assert.IsNull(viewModel.Name);
            Assert.IsTrue(viewModel.IsValid);
            Assert.IsFalse(viewModel.HasErrors);
            Assert.IsFalse(viewModel.IsDirty);

            // validation must fail
            viewModel.Name = "sungaila";
            Assert.IsFalse(viewModel.IsValid);
            Assert.IsTrue(viewModel.HasErrors);
            var errors = viewModel.GetErrors(nameof(TestViewModel.Name)) as IEnumerable<string>;
            Assert.IsNotNull(errors);
            Assert.AreEqual("Name cannot be stupid!", errors!.Single());
            Assert.IsTrue(viewModel.IsDirty);

            // validation must succeed
            viewModel.Name = "Tommy";
            Assert.IsTrue(viewModel.IsValid);
            Assert.IsFalse(viewModel.HasErrors);
            errors = viewModel.GetErrors(nameof(TestViewModel.Name)) as IEnumerable<string>;
            Assert.IsNotNull(errors);
            Assert.AreEqual(0, errors!.Count());

            // validation must fail
            viewModel.Name = string.Empty;
            Assert.IsFalse(viewModel.IsValid);
            Assert.IsTrue(viewModel.HasErrors);
            errors = viewModel.GetErrors(nameof(TestViewModel.Name)) as IEnumerable<string>;
            Assert.IsNotNull(errors);
            Assert.AreEqual("Name cannot be null or empty!", errors!.Single());

            bool propertyChangingRaised = false;
            bool propertyChangedRaised = false;
            bool errorsChangedRaised = false;
            viewModel.PropertyChanging += (s, e) =>
            {
                Assert.AreEqual(viewModel, s);

                if (e.PropertyName != nameof(TestViewModel.Name))
                    return;

                propertyChangingRaised = true;
                Assert.IsFalse(propertyChangedRaised, "Changing must be raised before Changed.");
            };
            viewModel.PropertyChanged += (s, e) =>
            {
                Assert.AreEqual(viewModel, s);

                if (e.PropertyName != nameof(TestViewModel.Name))
                    return;

                propertyChangedRaised = true;
                Assert.IsTrue(propertyChangingRaised, "Changing must be raised before Changed.");
            };
            viewModel.ErrorsChanged += (s, e) =>
            {
                Assert.AreEqual(viewModel, s);

                if (e.PropertyName != nameof(TestViewModel.Name))
                    return;

                errorsChangedRaised = true;
            };
            viewModel.IsDirty = false;

            // PropertyChanged must not be raised (value is unchanged)
            viewModel.Name = string.Empty;
            Assert.IsFalse(propertyChangingRaised);
            Assert.IsFalse(propertyChangedRaised);
            Assert.IsFalse(errorsChangedRaised);
            Assert.IsFalse(viewModel.IsDirty);

            // PropertyChanged must be raised
            viewModel.Name = "JC Denton";
            Assert.IsTrue(propertyChangingRaised);
            Assert.IsTrue(propertyChangedRaised);
            Assert.IsTrue(propertyChangedRaised);
            Assert.IsTrue(viewModel.IsDirty);

            var child = new DummyViewModel();
            Assert.IsNull(child.ParentViewModel);
            Assert.IsNull(child.RootViewModel);

            // ParentViewModel is set
            viewModel.Child = child;
            Assert.IsNotNull(viewModel.Child);
            Assert.AreEqual(viewModel, child.ParentViewModel);
            Assert.AreEqual(viewModel, child.RootViewModel);

            viewModel.IsDirty = false;
            child.IsDirty = false;

            // ignored properties should not set IsDirty
            child.FunLevel = 9001;
            Assert.IsFalse(viewModel.IsDirty);
            Assert.IsFalse(child.IsDirty);

            // check if IsDirty is bubbled to the parents
            child.Age = 42;
            Assert.IsTrue(viewModel.IsDirty);
            Assert.IsTrue(child.IsDirty);

            // the ParentViewModel is reset
            viewModel.Child = null;
            Assert.IsNull(viewModel.Child);
            Assert.IsNull(child.ParentViewModel);
            Assert.IsNull(child.RootViewModel);

            viewModel.IsDirty = false;
            child.IsDirty = false;

            // no IsDirty for the null ParentViewModel
            child.Age = 24;
            Assert.IsFalse(viewModel.IsDirty);
            Assert.IsTrue(child.IsDirty);
        }

        public class TestViewModel : ViewModel
        {
            private string? _name;

            public string? Name
            {
                get => _name;
                set => SetProperty(ref _name, value, NameValidation);
            }

            private IEnumerable<string> NameValidation(string? value)
            {
                if (string.IsNullOrEmpty(value))
                    yield return "Name cannot be null or empty!";
                else if (value == "sungaila")
                    yield return "Name cannot be stupid!";
            }

            private DummyViewModel? _child;

            public DummyViewModel? Child
            {
                get => _child;
                set => SetProperty(ref _child, value);
            }
        }

        public class TestCommand : ViewModelCommand<TestViewModel>
        {
            public bool Executed { get; set; }

            public override void Execute(TestViewModel parameter)
            {
                Executed = true;
            }
        }

        public class DummyViewModel : ViewModel
        {
            private int _age;

            public int Age
            {
                get => _age;
                set => SetProperty(ref _age, value);
            }

            private int _funLevel;

            public int FunLevel
            {
                get => _funLevel;
                set => SetProperty(ref _funLevel, value);
            }

            protected override IEnumerable<string> IgnoredDirtyProperties
            {
                get
                {
                    yield return nameof(FunLevel);
                }
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