using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace PresentationBase.Tests
{
    [TestClass]
    public class TrxViewModelTests
    {
        [TestMethod]
        public void ThrowsIfCannotEndEdit()
        {
            Assert.ThrowsException<InvalidOperationException>(() => new TestViewModel().EndEdit());
        }

        [TestMethod]
        public void ThrowsIfCannotCancelEdit()
        {
            Assert.ThrowsException<InvalidOperationException>(() => new TestViewModel().CancelEdit());
        }

        [TestMethod]
        public void ThrowsIfCannotBeginEdit()
        {
            var viewModel = new TestViewModel();
            viewModel.BeginEdit();
            Assert.ThrowsException<InvalidOperationException>(() => viewModel.BeginEdit());
        }

        [TestMethod]
        public void RollbackValueTypes1()
        {
            var viewModel = new TestViewModel();
            Assert.IsNull(viewModel.Name);
            Assert.IsFalse(viewModel.IsChanged);
            Assert.IsFalse(viewModel.IsDirty);

            viewModel.Name = "Kain";
            Assert.AreEqual("Kain", viewModel.Name);
            Assert.IsTrue(viewModel.IsChanged);
            Assert.IsTrue(viewModel.IsDirty);

            viewModel.RejectChanges();
            Assert.IsNull(viewModel.Name);
            Assert.IsFalse(viewModel.IsChanged);
            Assert.IsTrue(viewModel.IsDirty);
        }

        [TestMethod]
        public void RollbackValueTypes2()
        {
            var viewModel = new TestViewModel();
            Assert.IsNull(viewModel.Name);
            Assert.IsNull(viewModel.Description);
            Assert.AreEqual(0, viewModel.Age);
            Assert.IsFalse(viewModel.IsChanged);
            Assert.IsFalse(viewModel.IsDirty);

            viewModel.Name = "Kain";
            viewModel.Description = "Refuses sacrifices.";
            viewModel.Age = 2000;
            Assert.AreEqual("Kain", viewModel.Name);
            Assert.AreEqual("Refuses sacrifices.", viewModel.Description);
            Assert.AreEqual(2000, viewModel.Age);
            Assert.IsTrue(viewModel.IsChanged);
            Assert.IsTrue(viewModel.IsDirty);
            viewModel.AcceptChanges();
            Assert.IsFalse(viewModel.IsChanged);
            Assert.IsTrue(viewModel.IsDirty);

            viewModel.Description = "Does not speak of conscience.";
            viewModel.Age = 3000;
            Assert.IsTrue(viewModel.IsChanged);
            Assert.IsTrue(viewModel.IsDirty);

            viewModel.RejectChanges();
            Assert.AreEqual("Kain", viewModel.Name);
            Assert.AreEqual("Refuses sacrifices.", viewModel.Description);
            Assert.AreEqual(3000, viewModel.Age);
            Assert.IsFalse(viewModel.IsChanged);
            Assert.IsTrue(viewModel.IsDirty);
        }

        [TestMethod]
        public void RollbackReferenceTypes1()
        {
            var dummy = new object();
            var viewModel = new TestViewModel();
            Assert.IsNull(viewModel.Dummy);
            Assert.IsFalse(viewModel.IsChanged);
            Assert.IsFalse(viewModel.IsDirty);

            viewModel.Dummy = dummy;
            Assert.AreEqual(dummy, viewModel.Dummy);
            Assert.IsTrue(viewModel.IsChanged);
            Assert.IsTrue(viewModel.IsDirty);

            viewModel.RejectChanges();
            Assert.IsNull(viewModel.Dummy);
            Assert.IsFalse(viewModel.IsChanged);
            Assert.IsTrue(viewModel.IsDirty);
        }

        [TestMethod]
        public void RollbackReferenceTypes2()
        {
            var dummy = new object();
            var viewModel = new TestViewModel();
            Assert.IsNull(viewModel.Dummy);
            Assert.IsFalse(viewModel.IsChanged);
            Assert.IsFalse(viewModel.IsDirty);

            viewModel.Dummy = dummy;
            Assert.AreEqual(dummy, viewModel.Dummy);
            Assert.IsTrue(viewModel.IsChanged);
            Assert.IsTrue(viewModel.IsDirty);
            viewModel.AcceptChanges();
            Assert.IsFalse(viewModel.IsChanged);
            Assert.IsTrue(viewModel.IsDirty);

            viewModel.Dummy = new object();
            Assert.AreNotEqual(dummy, viewModel.Dummy);
            Assert.IsTrue(viewModel.IsChanged);
            Assert.IsTrue(viewModel.IsDirty);

            viewModel.RejectChanges();
            Assert.AreEqual(dummy, viewModel.Dummy);
            Assert.IsFalse(viewModel.IsChanged);
            Assert.IsTrue(viewModel.IsDirty);
        }

        [TestMethod]
        public void RollbackChildViewModel1()
        {
            var viewModel = new TestViewModel();
            Assert.IsNull(viewModel.Dummy);
            Assert.IsFalse(viewModel.IsChanged);
            Assert.IsFalse(viewModel.IsDirty);

            var child = new TestViewModel();
            Assert.IsNull(viewModel.Dummy);
            Assert.IsFalse(viewModel.IsChanged);
            Assert.IsFalse(viewModel.IsDirty);

            child.Name = "Raziel";
            child.Description = "Is worthy.";
            viewModel.Name = "Kain";
            viewModel.Description = "Casts children into the abyss.";
            viewModel.Child = child;
            Assert.AreEqual(child, viewModel.Child);
            Assert.AreEqual(viewModel, child.ParentViewModel);
            Assert.IsTrue(child.IsChanged);
            Assert.IsTrue(child.IsDirty);
            Assert.IsTrue(viewModel.IsChanged);
            Assert.IsTrue(viewModel.IsDirty);

            child.RejectChanges();
            Assert.IsNull(child.Name);
            Assert.IsNull(child.Description);
            Assert.AreEqual(child, viewModel.Child);
            Assert.IsNull(child.ParentViewModel);
            Assert.AreEqual("Kain", viewModel.Name);
            Assert.AreEqual("Casts children into the abyss.", viewModel.Description);
            Assert.IsFalse(child.IsChanged);
            Assert.IsTrue(child.IsDirty);
            Assert.IsTrue(viewModel.IsChanged);
            Assert.IsTrue(viewModel.IsDirty);
        }

        [TestMethod]
        public void RollbackChildViewModel2()
        {
            var child = new TestViewModel
            {
                Name = "Raziel",
                Description = "Is a Time spanned soul."
            };
            var viewModel = new TestViewModel
            {
                Name = "Kain",
                Description = "Felt the full gravity of choice.",
                Child = child
            };

            viewModel.Name = "Kain";
            viewModel.Description = "Felt the full gravity of choice.";
            viewModel.Child = child;
            Assert.AreEqual(child, viewModel.Child);
            Assert.AreEqual(viewModel, child.ParentViewModel);
            Assert.IsTrue(child.IsChanged);
            Assert.IsTrue(child.IsDirty);
            Assert.IsTrue(viewModel.IsChanged);
            Assert.IsTrue(viewModel.IsDirty);

            viewModel.RejectChanges();
            Assert.IsNull(child.Name);
            Assert.IsNull(child.Description);
            Assert.IsNull(viewModel.Child);
            Assert.IsNull(child.ParentViewModel);
            Assert.IsNull(viewModel.Name);
            Assert.IsNull(viewModel.Description);
            Assert.IsFalse(child.IsChanged);
            Assert.IsTrue(child.IsDirty);
            Assert.IsFalse(viewModel.IsChanged);
            Assert.IsTrue(viewModel.IsDirty);

            child.Name = "Raziel";
            child.Description = "Is a Time spanned soul.";
            viewModel.Child = child;
            child.AcceptChanges();
            Assert.IsFalse(child.IsChanged);
            Assert.IsTrue(child.IsDirty);

            viewModel.RejectChanges();
            Assert.AreEqual("Raziel", child.Name);
            Assert.AreEqual("Is a Time spanned soul.", child.Description);
            Assert.IsNull(viewModel.Child);
            Assert.IsNull(child.ParentViewModel);
            Assert.IsFalse(child.IsChanged);
            Assert.IsTrue(child.IsDirty);
        }

        [TestMethod]
        public void RollbackCollection1()
        {
            var kain = new TestViewModel();
            Assert.IsNull(kain.Name);
            Assert.IsNotNull(kain.Children);
            Assert.IsFalse(kain.IsChanged);
            Assert.IsFalse(kain.IsDirty);

            var turel = new TestViewModel { Name = "Turel" };
            var dumah = new TestViewModel { Name = "Dumah" };
            var rahab = new TestViewModel { Name = "Rahab" };
            var zephon = new TestViewModel { Name = "Zephon" };
            var melchiah = new TestViewModel { Name = "Melchiah" };

            kain.Name = "Kain";
            kain.Description = "Invents a new kind of reward.";
            kain.Children.AddRange(new[] { turel, dumah, rahab, zephon, melchiah });
            Assert.IsTrue(kain.IsChanged);
            Assert.IsTrue(kain.IsDirty);
            Assert.AreEqual(kain, kain.Children[0].ParentViewModel);
            Assert.AreEqual(kain, kain.Children[1].ParentViewModel);
            Assert.AreEqual(kain, kain.Children[2].ParentViewModel);
            Assert.AreEqual(kain, kain.Children[3].ParentViewModel);
            Assert.AreEqual(kain, kain.Children[4].ParentViewModel);
            Assert.IsTrue(kain.Children[0].IsChanged);
            Assert.IsTrue(kain.Children[1].IsChanged);
            Assert.IsTrue(kain.Children[2].IsChanged);
            Assert.IsTrue(kain.Children[3].IsChanged);
            Assert.IsTrue(kain.Children[4].IsChanged);

            kain.RejectChanges();
            Assert.IsNull(kain.Name);
            Assert.IsNull(kain.Children);

            Assert.IsNull(turel.Name);
            Assert.IsNull(dumah.Name);
            Assert.IsNull(rahab.Name);
            Assert.IsNull(zephon.Name);
            Assert.IsNull(melchiah.Name);
            Assert.IsFalse(turel.IsChanged);
            Assert.IsFalse(dumah.IsChanged);
            Assert.IsFalse(rahab.IsChanged);
            Assert.IsFalse(zephon.IsChanged);
            Assert.IsFalse(melchiah.IsChanged);
        }

        [TestMethod]
        public void RollbackCollection2()
        {
            var kain = new TestViewModel();
            Assert.IsNull(kain.Name);
            Assert.IsNotNull(kain.Children);
            Assert.IsFalse(kain.IsChanged);
            Assert.IsFalse(kain.IsDirty);

            var turel = new TestViewModel { Name = "Turel" };
            var dumah = new TestViewModel { Name = "Dumah" };
            var rahab = new TestViewModel { Name = "Rahab" };
            var zephon = new TestViewModel { Name = "Zephon" };
            var melchiah = new TestViewModel { Name = "Melchiah" };

            kain.Name = "Kain";
            kain.Description = "Traps the essence of life in his abominations.";
            kain.Children.AddRange(new[] { turel, dumah, rahab, zephon, melchiah });

            kain.AcceptChanges();
            Assert.IsFalse(kain.IsChanged);
            Assert.IsTrue(kain.IsDirty);
            Assert.AreEqual(kain, kain.Children[0].ParentViewModel);
            Assert.AreEqual(kain, kain.Children[1].ParentViewModel);
            Assert.AreEqual(kain, kain.Children[2].ParentViewModel);
            Assert.AreEqual(kain, kain.Children[3].ParentViewModel);
            Assert.AreEqual(kain, kain.Children[4].ParentViewModel);
            Assert.IsFalse(kain.Children[0].IsChanged);
            Assert.IsFalse(kain.Children[1].IsChanged);
            Assert.IsFalse(kain.Children[2].IsChanged);
            Assert.IsFalse(kain.Children[3].IsChanged);
            Assert.IsFalse(kain.Children[4].IsChanged);

            kain.Children.Remove(zephon);
            Assert.IsNotNull(kain.Children);
            Assert.AreEqual(4, kain.Children.Count);
            Assert.IsNull(zephon.ParentViewModel);

            kain.RejectChanges();
            Assert.IsNotNull(kain.Children);
            Assert.AreEqual(5, kain.Children.Count);
            Assert.AreEqual(kain, zephon.ParentViewModel);
        }

        [TestMethod]
        public void RollbackCollection3()
        {
            var kain = new TestViewModel();
            Assert.IsNull(kain.Name);
            Assert.IsNotNull(kain.Children);
            Assert.IsFalse(kain.IsChanged);
            Assert.IsFalse(kain.IsDirty);

            var turel = new TestViewModel { Name = "Turel" };
            var dumah = new TestViewModel { Name = "Dumah" };
            var rahab = new TestViewModel { Name = "Rahab" };
            var zephon = new TestViewModel { Name = "Zephon" };
            var melchiah = new TestViewModel { Name = "Melchiah" };

            kain.Name = "Kain";
            kain.Description = "Does not like transgressions.";
            kain.Children.AddRange(new[] { turel, dumah, rahab, zephon, melchiah });

            turel.AcceptChanges();
            Assert.IsTrue(kain.IsChanged);
            Assert.IsTrue(kain.IsDirty);
            Assert.AreEqual(kain, kain.Children[0].ParentViewModel);
            Assert.AreEqual(kain, kain.Children[1].ParentViewModel);
            Assert.AreEqual(kain, kain.Children[2].ParentViewModel);
            Assert.AreEqual(kain, kain.Children[3].ParentViewModel);
            Assert.AreEqual(kain, kain.Children[4].ParentViewModel);
            Assert.IsFalse(kain.Children[0].IsChanged);
            Assert.IsTrue(kain.Children[1].IsChanged);
            Assert.IsTrue(kain.Children[2].IsChanged);
            Assert.IsTrue(kain.Children[3].IsChanged);
            Assert.IsTrue(kain.Children[4].IsChanged);

            turel.Name = "Turel Sarafan";
            Assert.IsTrue(turel.IsChanged);
            Assert.IsTrue(turel.IsDirty);

            kain.RejectChanges();
            Assert.IsNull(kain.Name);
            Assert.IsNull(kain.Children);
            Assert.IsNull(dumah.Name);
            Assert.IsNull(dumah.ParentViewModel);
            Assert.IsNull(rahab.Name);
            Assert.IsNull(rahab.ParentViewModel);
            Assert.IsNull(zephon.Name);
            Assert.IsNull(zephon.ParentViewModel);
            Assert.IsNull(melchiah.Name);
            Assert.IsNull(melchiah.ParentViewModel);

            Assert.AreEqual("Turel", turel.Name);
            Assert.AreEqual(kain, turel.ParentViewModel);
            Assert.IsFalse(turel.IsChanged);
            Assert.IsTrue(turel.IsDirty);
        }

        [TestMethod]
        public void SaveRedoCommands()
        {
            var raziel = new TestViewModel
            {
                Name = "Raziel",
                Description = "Has a dispute to settle."
            };
            raziel.AcceptChanges();
            Assert.IsFalse(raziel.IsChanged);
            Assert.IsTrue(raziel.IsDirty);

            Assert.IsFalse(raziel.Commands[typeof(SaveCommand)].CanExecute(raziel));
            Assert.IsFalse(raziel.Commands[typeof(UndoCommand)].CanExecute(raziel));

            raziel.Name = "Raziel Sarafan";
            Assert.IsTrue(raziel.Commands[typeof(SaveCommand)].CanExecute(raziel));
            Assert.IsTrue(raziel.Commands[typeof(UndoCommand)].CanExecute(raziel));

            raziel.Commands[typeof(UndoCommand)].Execute(raziel);
            Assert.AreEqual("Raziel", raziel.Name);
            Assert.IsFalse(raziel.IsChanged);
            Assert.IsTrue(raziel.IsDirty);
            Assert.IsFalse(raziel.Commands[typeof(SaveCommand)].CanExecute(raziel));
            Assert.IsFalse(raziel.Commands[typeof(UndoCommand)].CanExecute(raziel));

            raziel.Description = "Was spared from total dissolution.";
            Assert.IsTrue(raziel.IsChanged);
            Assert.IsTrue(raziel.IsDirty);
            Assert.IsTrue(raziel.Commands[typeof(SaveCommand)].CanExecute(raziel));
            Assert.IsTrue(raziel.Commands[typeof(UndoCommand)].CanExecute(raziel));

            raziel.Commands[typeof(SaveCommand)].Execute(raziel);
            Assert.AreEqual("Was spared from total dissolution.", raziel.Description);
            Assert.IsFalse(raziel.IsChanged);
            Assert.IsTrue(raziel.IsDirty);
            Assert.IsFalse(raziel.Commands[typeof(SaveCommand)].CanExecute(raziel));
            Assert.IsFalse(raziel.Commands[typeof(UndoCommand)].CanExecute(raziel));
        }

        [TestMethod]
        public void BeginEdit()
        {
            var raziel = new TestViewModel
            {
                Name = "Raziel",
            };
            raziel.AcceptChanges();

            Assert.IsFalse(raziel.IsEditing);
            Assert.IsFalse(raziel.IsChanged);
            Assert.IsTrue(raziel.IsDirty);

            raziel.Dummy = new object();
            Assert.IsFalse(raziel.IsEditing);
            Assert.IsTrue(raziel.IsChanged);
            Assert.IsTrue(raziel.IsDirty);

            raziel.BeginEdit();
            Assert.IsTrue(raziel.IsEditing);
            Assert.IsTrue(raziel.IsChanged);
            Assert.IsTrue(raziel.IsDirty);
        }

        [TestMethod]
        public void EndEdit()
        {
            var dummy = new object();
            var raziel = new TestViewModel
            {
                Name = "Raziel",
            };
            raziel.AcceptChanges();
            raziel.Dummy = dummy;

            raziel.BeginEdit();
            Assert.IsTrue(raziel.IsEditing);
            Assert.IsTrue(raziel.IsChanged);
            Assert.IsTrue(raziel.IsDirty);

            raziel.EndEdit();
            Assert.AreEqual("Raziel", raziel.Name);
            Assert.AreEqual(dummy, raziel.Dummy);
            Assert.IsFalse(raziel.IsEditing);
            Assert.IsFalse(raziel.IsChanged);
            Assert.IsTrue(raziel.IsDirty);
        }

        [TestMethod]
        public void CancelEdit()
        {
            var dummy = new object();
            var raziel = new TestViewModel
            {
                Name = "Raziel",
            };

            raziel.BeginEdit();
            raziel.Dummy = dummy;

            Assert.IsTrue(raziel.IsEditing);
            Assert.IsTrue(raziel.IsChanged);
            Assert.IsTrue(raziel.IsDirty);

            raziel.CancelEdit();
            Assert.AreEqual("Raziel", raziel.Name);
            Assert.IsNull(raziel.Dummy);
            Assert.IsFalse(raziel.IsEditing);
            Assert.IsFalse(raziel.IsChanged);
            Assert.IsTrue(raziel.IsDirty);
        }

        class TestViewModel : TrxViewModel
        {
            private string? _name;

            public string? Name
            {
                get => _name;
                set => SetProperty(ref _name, value);
            }

            private string? _description;

            public string? Description
            {
                get => _description;
                set => SetProperty(ref _description, value);
            }

            private int _age;

            internal int Age
            {
                get => _age;
                set => SetProperty(ref _age, value);
            }

            private object? _dummy;

            public object? Dummy
            {
                get => _dummy;
                set => SetProperty(ref _dummy, value);
            }

            private TestViewModel? _child;

            public TestViewModel? Child
            {
                get => _child;
                set => SetProperty(ref _child, value);
            }

            public ObservableViewModelCollection<TestViewModel> Children { get; set; }

            public TestViewModel()
            {
                Children = new ObservableViewModelCollection<TestViewModel>(this);
            }
        }

        class SaveCommand : ViewModelCommand<TestViewModel>
        {
            public override void Execute(TestViewModel parameter)
            {
                parameter.AcceptChanges();
            }

            public override bool CanExecute(TestViewModel parameter) => parameter.IsChanged;
        }

        class UndoCommand : ViewModelCommand<TestViewModel>
        {
            public override void Execute(TestViewModel parameter)
            {
                parameter.RejectChanges();
            }

            public override bool CanExecute(TestViewModel parameter) => parameter.IsChanged;
        }
    }
}