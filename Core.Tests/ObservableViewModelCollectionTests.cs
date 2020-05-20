using Microsoft.VisualStudio.TestTools.UnitTesting;
using PresentationBase.DtoConverters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace PresentationBase.Tests
{
    [TestClass]
    public class ObservableViewModelCollectionTests
    {
        [TestMethod]
        public void CollectionManipulation()
        {
            var viewModel = new TestViewModel();
            Assert.IsNotNull(viewModel.Children);
            Assert.AreEqual(0, viewModel.Children!.Count);

            NotifyCollectionChangedEventArgs? args = null;
            int notifyCount = 0;
            viewModel.Children.CollectionChanged += (s, e) =>
            {
                Assert.AreEqual(viewModel.Children, s);
                Assert.IsNotNull(e);
                args = e;
                notifyCount++;
            };

            var child = new TestViewModel();
            viewModel.Children.Add(child);
            Assert.AreEqual(1, viewModel.Children.Count);
            Assert.AreSame(child, viewModel.Children[0]);
            CheckCollectionChangedArgs(ref args, ref notifyCount, NotifyCollectionChangedAction.Add, -1, 0, null, new List<TestViewModel> { child });

            var child2 = new TestViewModel();
            viewModel.Children.Add(child2);
            Assert.AreEqual(2, viewModel.Children.Count);
            Assert.AreSame(child2, viewModel.Children[1]);
            CheckCollectionChangedArgs(ref args, ref notifyCount, NotifyCollectionChangedAction.Add, -1, 1, null, new List<TestViewModel> { child2 });

            viewModel.Children.Remove(child);
            Assert.AreEqual(1, viewModel.Children.Count);
            Assert.AreSame(child2, viewModel.Children[0]);
            CheckCollectionChangedArgs(ref args, ref notifyCount, NotifyCollectionChangedAction.Remove, 0, -1, new List<TestViewModel> { child }, null);

            viewModel.Children.Add(child);
            viewModel.Children.RemoveAt(0);
            Assert.AreEqual(1, viewModel.Children.Count);
            Assert.AreSame(child, viewModel.Children[0]);
            Assert.IsTrue(viewModel.Children.Contains(child));
            Assert.IsFalse(viewModel.Children.Contains(child2));
            CheckCollectionChangedArgs(ref args, ref notifyCount, NotifyCollectionChangedAction.Remove, 0, -1, new List<TestViewModel> { child2 }, null, 2);

            var vm1 = new TestViewModel();
            var vm2 = new TestViewModel();
            var vm3 = new TestViewModel();
            viewModel.Children.AddRange(new List<TestViewModel> { vm1, vm2, vm3 });
            Assert.AreEqual(4, viewModel.Children.Count);
            Assert.AreSame(child, viewModel.Children[0]);
            CheckCollectionChangedArgs(ref args, ref notifyCount, NotifyCollectionChangedAction.Add, -1, 3, null, new List<TestViewModel> { vm3 }, 3);

            viewModel.Children.Insert(1, vm3);
            Assert.AreEqual(5, viewModel.Children.Count);
            Assert.AreSame(child, viewModel.Children[0]);
            Assert.AreSame(vm3, viewModel.Children[1]);
            CheckCollectionChangedArgs(ref args, ref notifyCount, NotifyCollectionChangedAction.Add, -1, 1, null, new List<TestViewModel> { vm3 });

            viewModel.Children.Replace(vm3, vm2, vm1);
            Assert.AreEqual(3, viewModel.Children.Count);
            Assert.AreSame(vm3, viewModel.Children[0]);
            Assert.AreSame(vm2, viewModel.Children[1]);
            Assert.AreSame(vm1, viewModel.Children[2]);
            CheckCollectionChangedArgs(ref args, ref notifyCount, NotifyCollectionChangedAction.Add, -1, 2, null, new List<TestViewModel> { vm1 }, 4);

            viewModel.Children.Clear();
            Assert.AreEqual(0, viewModel.Children.Count);
            Assert.IsFalse(viewModel.Children.Contains(vm3));
            Assert.IsFalse(viewModel.Children.Contains(vm2));
            Assert.IsFalse(viewModel.Children.Contains(vm1));
            CheckCollectionChangedArgs(ref args, ref notifyCount, NotifyCollectionChangedAction.Reset, -1, -1, (IEnumerable<TestViewModel>?)null, null);
        }

        private static void CheckCollectionChangedArgs<TViewModel>(ref NotifyCollectionChangedEventArgs? args, ref int notifyCount, NotifyCollectionChangedAction expectedAction, int expectedOldStartingIndex, int expectedNewStartingIndex, IEnumerable<TViewModel>? expectedOldItems, IEnumerable<TViewModel>? expectedNewItems, int expectedNotifyCount = 1)
            where TViewModel : ViewModel
        {
            Assert.IsNotNull(args);
            Assert.AreEqual(expectedAction, args!.Action);
            Assert.AreEqual(expectedOldStartingIndex, args.OldStartingIndex);
            Assert.AreEqual(expectedNewStartingIndex, args.NewStartingIndex);
            Assert.AreEqual(expectedNotifyCount, notifyCount);

            if (expectedOldItems == null)
            {
                Assert.IsNull(args.OldItems);
            }
            else
            {
                Assert.AreEqual(expectedOldItems.Count(), args.OldItems.Count);
                foreach (var item in expectedOldItems)
                {
                    Assert.AreEqual(args.OldItems.Cast<TViewModel>().Count(i => i == item), 1);
                }
            }

            if (expectedNewItems == null)
            {
                Assert.IsNull(args.NewItems);
            }
            else
            {
                Assert.AreEqual(expectedNewItems.Count(), args.NewItems.Count);
                foreach (var item in expectedNewItems)
                {
                    Assert.AreEqual(args.NewItems.Cast<TViewModel>().Count(i => i == item), 1);
                }
            }

            args = null;
            notifyCount = 0;
        }

        [TestMethod]
        public void Observe()
        {
            var viewModel = new TestViewModel();
            Assert.IsNotNull(viewModel.Children);
            Assert.AreEqual(0, viewModel.Children!.Count);

            bool observeRaised = false;
            bool observe2Raised = false;
            string? observedProperty = null;
            Action observeHandler = () =>
            {
                observeRaised = true;
            };
            Action<string> observeHandler2 = (name) =>
            {
                Assert.AreNotEqual("NotExisting", name);
                observe2Raised = true;
                observedProperty = name;
            };
            viewModel.Children.Observe(observeHandler, nameof(TestViewModel.Age), nameof(TestViewModel.FunLevel), "NotExisting");
            viewModel.Children.Observe(observeHandler2, nameof(TestViewModel.Age), nameof(TestViewModel.FunLevel), "NotExisting");

            Assert.IsFalse(viewModel.IsDirty);

            var child = new TestViewModel();
            viewModel.Children.Add(child);
            Assert.IsTrue(viewModel.IsDirty);
            Assert.IsFalse(observeRaised);
            Assert.IsFalse(observe2Raised);
            Assert.IsNull(observedProperty);
            viewModel.IsDirty = false;

            child.Age = 42;
            Assert.IsTrue(viewModel.IsDirty);
            CheckObservedProperty(ref observeRaised, ref observe2Raised, ref observedProperty, nameof(TestViewModel.Age));
            viewModel.IsDirty = false;

            child.FunLevel = 9001;
            Assert.IsTrue(viewModel.IsDirty);
            CheckObservedProperty(ref observeRaised, ref observe2Raised, ref observedProperty, nameof(TestViewModel.FunLevel));
            viewModel.IsDirty = false;
        }

        private static void CheckObservedProperty(ref bool observeRaised, ref bool observe2Raised, ref string? observedProperty, string expectedPropertyName)
        {
            Assert.IsTrue(observeRaised);
            Assert.IsTrue(observe2Raised);
            Assert.IsNotNull(observedProperty);
            Assert.AreEqual(observedProperty, expectedPropertyName);

            observeRaised = false;
            observe2Raised = false;
            observedProperty = null;
        }

        public class TestViewModel : ViewModel
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

            public ObservableViewModelCollection<TestViewModel>? Children { get; set; }

            public TestViewModel()
            {
                Children = new ObservableViewModelCollection<TestViewModel>(this);
            }
        }
    }
}
