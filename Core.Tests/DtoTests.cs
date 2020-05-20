using Microsoft.VisualStudio.TestTools.UnitTesting;
using PresentationBase.DtoConverters;
using System;

namespace PresentationBase.Tests
{
    [TestClass]
    public class DtoTests
    {
        [TestMethod]
        public void Attributes()
        {
            var dtoAttr = DtoAttribute.GetDtoAttribute(typeof(DummyViewModel));
            Assert.IsNull(dtoAttr);

            dtoAttr = DtoAttribute.GetDtoAttribute(typeof(Dummy2ViewModel));
            Assert.IsNotNull(dtoAttr);
            Assert.IsNotNull(dtoAttr!.Type);
            Assert.AreEqual(dtoAttr.Type, typeof(string));

            dtoAttr = DtoAttribute.GetDtoAttribute(typeof(Dummy3ViewModel));
            Assert.IsNotNull(dtoAttr);
            Assert.AreEqual(dtoAttr!.Type, typeof(AwesomeTransferDataObject));

            var dtoPropAttr = DtoPropertyAttribute.GetDtoPropertyAttribute(typeof(Dummy3ViewModel).GetProperty(nameof(Dummy3ViewModel.NonExistent))!);
            Assert.IsNotNull(dtoPropAttr);
            Assert.AreEqual(dtoPropAttr!.PropertyName, "NotExistingProperty");
        }

        [TestMethod]
        public void ConvertAndBack()
        {
            var dto = new AwesomeTransferDataObject { PersonName = "John" };
            Assert.AreEqual("John", dto.PersonName);
            Assert.AreEqual(default(int), dto.PersonAge);

            var viewModel = dto.ToViewModel<AwesomeViewModel>();
            Assert.IsNotNull(viewModel);
            Assert.IsInstanceOfType(viewModel, typeof(AwesomeViewModel));

            if (viewModel.Name == "John")
                viewModel.Age = 33;
            Assert.AreEqual("John", viewModel.Name);
            Assert.AreEqual(33, viewModel.Age);

            var dto2 = viewModel.ToDto<AwesomeTransferDataObject>();
            Assert.IsNotNull(dto2);
            Assert.IsInstanceOfType(dto2, typeof(AwesomeTransferDataObject));
            Assert.AreEqual("John", dto2.PersonName);
            Assert.AreEqual(33, dto2.PersonAge);
        }

        [TestMethod]
        public void ToViewModelExceptions()
        {
            object? dto = null;
            Assert.ThrowsException<ArgumentNullException>(() => dto!.ToViewModel<AwesomeViewModel>());

            dto = new AwesomeTransferDataObject();
            Assert.ThrowsException<InvalidOperationException>(() => dto.ToViewModel<DummyViewModel>());
            Assert.ThrowsException<InvalidCastException>(() => dto.ToViewModel<Dummy2ViewModel>());
            Assert.ThrowsException<InvalidOperationException>(() => dto.ToViewModel<Dummy3ViewModel>());
        }

        [TestMethod]
        public void ToDtoExceptions()
        {
            ViewModel? viewModel = null;
            Assert.ThrowsException<ArgumentNullException>(() => viewModel!.ToDto<AwesomeTransferDataObject>());

            viewModel = new DummyViewModel();
            Assert.ThrowsException<InvalidOperationException>(() => viewModel.ToDto<AwesomeTransferDataObject>());

            viewModel = new Dummy2ViewModel();
            Assert.ThrowsException<InvalidCastException>(() => viewModel.ToDto<AwesomeTransferDataObject>());

            viewModel = new Dummy3ViewModel();
            Assert.ThrowsException<InvalidOperationException>(() => viewModel.ToDto<AwesomeTransferDataObject>());
        }

        class AwesomeTransferDataObject
        {
            public string? PersonName { get; set; }

            public int PersonAge { get; set; }
        }

        [Dto(typeof(AwesomeTransferDataObject))]
        class AwesomeViewModel : ViewModel
        {
            private string? _name;

            [DtoProperty(nameof(AwesomeTransferDataObject.PersonName))]
            public string? Name
            {
                get => _name;
                set => SetProperty(ref _name, value);
            }

            private int _age;

            [DtoProperty(nameof(AwesomeTransferDataObject.PersonAge))]
            public int Age
            {
                get => _age;
                set => SetProperty(ref _age, value);
            }
        }

        class DummyViewModel : ViewModel { }

        [Dto(typeof(string))]
        class Dummy2ViewModel : ViewModel { }

        [Dto(typeof(AwesomeTransferDataObject))]
        class Dummy3ViewModel : ViewModel
        {
            private int _nonExistent;

            [DtoProperty("NotExistingProperty")]
            public int NonExistent
            {
                get => _nonExistent;
                set => SetProperty(ref _nonExistent, value);
            }
        }
    }
}
