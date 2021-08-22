using Microsoft.VisualStudio.TestTools.UnitTesting;
using PresentationBase.DtoConverters;
using System;
using System.Linq;

namespace PresentationBase.Tests
{
    [TestClass]
    public class DtoTests
    {
        [TestMethod]
        [Obsolete("GetDtoAttribute is deprecated and replaced by GetDtoAttributes.")]
        public void GetDtoAttribute()
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
            Assert.AreEqual("NotExistingProperty", dtoPropAttr!.PropertyName);
        }

        [TestMethod]
        public void GetDtoAttributes()
        {
            var dtoAttrs = DtoAttribute.GetDtoAttributes(typeof(DummyViewModel));
            Assert.IsNotNull(dtoAttrs);
            Assert.IsFalse(dtoAttrs.Any());

            dtoAttrs = DtoAttribute.GetDtoAttributes(typeof(Dummy2ViewModel));
            Assert.IsNotNull(dtoAttrs);
            Assert.IsTrue(dtoAttrs.Any());
            Assert.IsTrue(dtoAttrs.Count() == 1);
            Assert.IsNotNull(dtoAttrs.Single().Type);
            Assert.AreEqual(dtoAttrs.Single().Type, typeof(string));

            dtoAttrs = DtoAttribute.GetDtoAttributes(typeof(Dummy3ViewModel));
            Assert.IsNotNull(dtoAttrs);
            Assert.IsTrue(dtoAttrs.Any());
            Assert.IsTrue(dtoAttrs.Count() == 1);
            Assert.AreEqual(dtoAttrs.Single().Type, typeof(AwesomeTransferDataObject));

            var dtoPropAttrs = DtoPropertyAttribute.GetDtoPropertyAttributes(typeof(Dummy3ViewModel).GetProperty(nameof(Dummy3ViewModel.NonExistent))!);
            Assert.IsNotNull(dtoPropAttrs);
            Assert.IsTrue(dtoAttrs.Any());
            Assert.IsTrue(dtoAttrs.Count() == 1);
            Assert.AreEqual("NotExistingProperty", dtoPropAttrs.Single().PropertyName);
        }

        [TestMethod]
        public void GetDtoAttributesMultiple()
        {
            var dtoAttrs = DtoAttribute.GetDtoAttributes(typeof(Dummy4ViewModel));
            Assert.IsNotNull(dtoAttrs);
            Assert.IsTrue(dtoAttrs.Any());
            Assert.IsTrue(dtoAttrs.Count() == 2);

            var awesomeDtoAttr = dtoAttrs.FirstOrDefault(attr => attr.Type == typeof(AwesomeTransferDataObject));
            Assert.IsNotNull(awesomeDtoAttr);

            var averageDtoAttr = dtoAttrs.FirstOrDefault(attr => attr.Type == typeof(AverageTransferDataObject));
            Assert.IsNotNull(awesomeDtoAttr);

            var dtoPropAttrs = DtoPropertyAttribute.GetDtoPropertyAttributes(typeof(Dummy4ViewModel).GetProperty(nameof(Dummy4ViewModel.PersonAge))!);
            Assert.IsNotNull(dtoPropAttrs);
            Assert.IsTrue(dtoPropAttrs.Any());
            Assert.IsTrue(dtoPropAttrs.Count() == 1);
            Assert.IsNotNull(dtoPropAttrs.SingleOrDefault(attr => attr.Type == typeof(AwesomeTransferDataObject)));
        }

        [TestMethod]
        public void ConvertAndBack()
        {
            var dto = new AwesomeTransferDataObject { PersonName = "John" };
            Assert.AreEqual("John", dto.PersonName);
            Assert.AreEqual(default, dto.PersonAge);

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
        public void ConvertAndBackMultiple()
        {
            var dto = new AwesomeTransferDataObject { PersonName = "John" };
            Assert.AreEqual("John", dto.PersonName);
            Assert.AreEqual(default, dto.PersonAge);

            var viewModel = dto.ToViewModel<Dummy4ViewModel>();
            Assert.IsNotNull(viewModel);
            Assert.IsInstanceOfType(viewModel, typeof(Dummy4ViewModel));
            Assert.AreEqual(dto.PersonName, viewModel.Name);
            Assert.AreEqual(dto.PersonAge, viewModel.PersonAge);
            Assert.AreEqual(null, viewModel.TownName);

            var reversedDto = viewModel.ToDto<AwesomeTransferDataObject>();
            Assert.IsNotNull(reversedDto);
            Assert.IsInstanceOfType(reversedDto, typeof(AwesomeTransferDataObject));
            Assert.AreEqual(reversedDto.PersonName, viewModel.Name);
            Assert.AreEqual(reversedDto.PersonAge, viewModel.PersonAge);

            var dto2 = new AverageTransferDataObject { PersonName = "Urgot", TownName = "Zhaun" };
            var viewModel2 = dto2.ToViewModel<Dummy4ViewModel>();
            Assert.IsNotNull(viewModel2);
            Assert.IsInstanceOfType(viewModel2, typeof(Dummy4ViewModel));
            Assert.AreEqual(dto2.PersonName, viewModel2.Name);
            Assert.AreEqual(default, viewModel2.PersonAge);
            Assert.AreEqual(dto2.TownName, viewModel2.TownName);

            var reversedDto2 = viewModel2.ToDto<AverageTransferDataObject>();
            Assert.IsNotNull(reversedDto2);
            Assert.IsInstanceOfType(reversedDto2, typeof(AverageTransferDataObject));
            Assert.AreEqual(reversedDto2.PersonName, viewModel2.Name);
            Assert.AreEqual(reversedDto2.TownName, viewModel2.TownName);
        }

        [TestMethod]
        public void ConvertAndBackInheritance()
        {
            var dto = new AwesomeTransferDataObject { PersonName = "John" };
            Assert.AreEqual("John", dto.PersonName);
            Assert.AreEqual(default, dto.PersonAge);

            var viewModel = dto.ToViewModel<InheritedAwesomenessViewModel>();
            Assert.IsNotNull(viewModel);
            Assert.IsInstanceOfType(viewModel, typeof(InheritedAwesomenessViewModel));
            Assert.AreEqual(dto.PersonName, viewModel.Name);
            Assert.AreEqual(null, viewModel.TownName);

            var reversedDto = viewModel.ToDto<AwesomeTransferDataObject>();
            Assert.IsNotNull(reversedDto);
            Assert.IsInstanceOfType(reversedDto, typeof(AwesomeTransferDataObject));
            Assert.AreEqual(reversedDto.PersonName, viewModel.Name);

            var dto2 = new AverageTransferDataObject { PersonName = "Urgot", TownName = "Zhaun" };
            Assert.ThrowsException<InvalidOperationException>(() => dto2.ToViewModel<InheritedAwesomenessViewModel>());
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

        class AverageTransferDataObject
        {
            public string? PersonName { get; set; }

            public string? TownName { get; set; }
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

        [Dto(typeof(AverageTransferDataObject))]
        class InheritedAwesomenessViewModel : AwesomeViewModel
        {
            private string? _townName;

            [DtoProperty(nameof(AverageTransferDataObject.TownName), typeof(AverageTransferDataObject))]
            public string? TownName
            {
                get => _townName;
                set => SetProperty(ref _townName, value);
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

        [Dto(typeof(AwesomeTransferDataObject))]
        [Dto(typeof(AverageTransferDataObject))]
        class Dummy4ViewModel : ViewModel
        {
            private string? _name;

            [DtoProperty(nameof(AwesomeTransferDataObject.PersonName))]
            public string? Name
            {
                get => _name;
                set => SetProperty(ref _name, value);
            }

            private int _personAge;

            [DtoProperty(nameof(AwesomeTransferDataObject.PersonAge), typeof(AwesomeTransferDataObject))]
            public int PersonAge
            {
                get => _personAge;
                set => SetProperty(ref _personAge, value);
            }

            private string? _townName;

            [DtoProperty(nameof(AverageTransferDataObject.TownName), typeof(AverageTransferDataObject))]
            public string? TownName
            {
                get => _townName;
                set => SetProperty(ref _townName, value);
            }
        }
    }
}