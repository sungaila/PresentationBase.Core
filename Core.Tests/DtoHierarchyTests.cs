using Microsoft.VisualStudio.TestTools.UnitTesting;
using PresentationBase.DtoConverters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PresentationBase.Tests
{
    [TestClass]
    public class DtoHierarchyTests
    {
        [TestMethod]
        public void ConvertEmpty()
        {
            var dto = new CatDataTransferObject();
            var viewModel = dto.ToViewModel<CatViewModel>();
            Assert.IsNotNull(viewModel);
            Assert.IsNull(viewModel.Nickname);
            Assert.IsNull(viewModel.FavoriteFood);
            Assert.IsNull(viewModel.Fur);
            Assert.IsNull(viewModel.Paws);

            var reversedDto = viewModel.ToDto<CatDataTransferObject>();
            Assert.IsNotNull(reversedDto);
            Assert.IsNull(reversedDto.Nickname);
            Assert.IsNull(reversedDto.FavoriteFood);
            Assert.IsNull(reversedDto.Fur);
            Assert.IsNull(reversedDto.Paws);
        }

        [TestMethod]
        public void ConvertSimple()
        {
            var dto = new CatDataTransferObject { Nickname = "Whiskers", FavoriteFood = new List<string> { "Fish", "Mice" } };
            var viewModel = dto.ToViewModel<CatViewModel>();
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(dto.Nickname, viewModel.Nickname);
            Assert.AreEqual(dto.FavoriteFood, viewModel.FavoriteFood);
            Assert.IsNull(viewModel.Fur);
            Assert.IsNull(viewModel.Paws);

            var reversedDto = viewModel.ToDto<CatDataTransferObject>();
            Assert.IsNotNull(reversedDto);
            Assert.AreEqual(viewModel.Nickname, reversedDto.Nickname);
            Assert.AreEqual(viewModel.FavoriteFood, reversedDto.FavoriteFood);
            Assert.IsNull(reversedDto.Fur);
            Assert.IsNull(reversedDto.Paws);
        }

        [TestMethod]
        public void ConvertSingleRelationship()
        {
            var dto = new CatDataTransferObject
            {
                Fur = new FurDataTransferObject { Description = "quite soft" }
            };
            var viewModel = dto.ToViewModel<CatViewModel>();
            Assert.IsNotNull(viewModel);
            Assert.IsNull(viewModel.Nickname);
            Assert.IsNull(viewModel.FavoriteFood);
            Assert.IsNotNull(viewModel.Fur);
            Assert.AreEqual(viewModel, viewModel.Fur!.ParentViewModel);
            Assert.AreEqual(viewModel, viewModel.Fur.RootViewModel);
            Assert.AreEqual(dto.Fur.Description, viewModel.Fur.Description);
            Assert.IsNull(viewModel.Paws);

            var reversedDto = viewModel.ToDto<CatDataTransferObject>();
            Assert.IsNotNull(reversedDto);
            Assert.IsNull(reversedDto.Nickname);
            Assert.IsNull(reversedDto.FavoriteFood);
            Assert.IsNotNull(reversedDto.Fur);
            Assert.AreEqual(viewModel.Fur.Description, reversedDto.Fur!.Description);
            Assert.IsNull(reversedDto.Paws);
        }

        [TestMethod]
        public void ConvertObservableRelationship()
        {
            var dto = new CatDataTransferObject
            {
                Paws = new List<PawDataTransferObject>(new[]
                {
                    new PawDataTransferObject { Steps = 300 },
                    new PawDataTransferObject { Steps = 300 },
                    new PawDataTransferObject { Steps = 320 },
                    new PawDataTransferObject { Steps = 320 }
                })
            };
            var viewModel = dto.ToViewModel<CatViewModel>();
            Assert.IsNotNull(viewModel);
            Assert.IsNull(viewModel.Nickname);
            Assert.IsNull(viewModel.FavoriteFood);
            Assert.IsNull(viewModel.Fur);
            Assert.IsNotNull(viewModel.Paws);
            Assert.AreEqual(dto.Paws.Count, viewModel.Paws!.Count);
            Assert.AreEqual(dto.Paws.ElementAt(0).Steps, viewModel.Paws[0].Steps);
            Assert.AreEqual(dto.Paws.ElementAt(1).Steps, viewModel.Paws[1].Steps);
            Assert.AreEqual(dto.Paws.ElementAt(2).Steps, viewModel.Paws[2].Steps);
            Assert.AreEqual(dto.Paws.ElementAt(3).Steps, viewModel.Paws[3].Steps);

            var reversedDto = viewModel.ToDto<CatDataTransferObject>();
            Assert.IsNotNull(reversedDto);
            Assert.IsNull(reversedDto.Nickname);
            Assert.IsNull(reversedDto.FavoriteFood);
            Assert.IsNull(reversedDto.Fur);
            Assert.IsNotNull(reversedDto.Paws);
            Assert.AreEqual(viewModel.Paws.Count, reversedDto.Paws!.Count);
            Assert.AreEqual(viewModel.Paws[0].Steps, reversedDto.Paws.ElementAt(0).Steps);
            Assert.AreEqual(viewModel.Paws[1].Steps, reversedDto.Paws.ElementAt(1).Steps);
            Assert.AreEqual(viewModel.Paws[2].Steps, reversedDto.Paws.ElementAt(2).Steps);
            Assert.AreEqual(viewModel.Paws[3].Steps, reversedDto.Paws.ElementAt(3).Steps);
        }

        [TestMethod]
        public void CircularRelationshipsSingle()
        {
            var dto1 = new SelfReferencingDataTransferObject
            {
                Other = new SelfReferencingDataTransferObject
                {
                    Other = new SelfReferencingDataTransferObject()
                }
            };
            Assert.IsNotNull(dto1.ToViewModel<SelfReferencingViewModel>().ToDto<SelfReferencingDataTransferObject>());

            var dto2 = new SelfReferencingDataTransferObject();
            dto2.Other = new SelfReferencingDataTransferObject
            {
                Other = new SelfReferencingDataTransferObject
                {
                    Other = dto2
                }
            };

            try
            {
                dto2.ToViewModel<SelfReferencingViewModel>();
                Assert.Fail("Expected the convertion of cyclic relationships to fail.");
            }
            catch (Exception) { }

            var viewModel = new SelfReferencingViewModel();
            viewModel.Other = new SelfReferencingViewModel
            {
                Other = new SelfReferencingViewModel
                {
                    Other = viewModel
                }
            };

            try
            {
                viewModel.ToDto<SelfReferencingDataTransferObject>();
                Assert.Fail("Expected the conversion of cyclic relationships to fail.");
            }
            catch (Exception) { }
        }

        [TestMethod]
        public void CircularRelationshipsMultiple()
        {
            var dto1 = new SelfReferencingDataTransferObject
            {
                Others = new List<SelfReferencingDataTransferObject>(new[] {
                    new SelfReferencingDataTransferObject
                    {
                        Others = new List<SelfReferencingDataTransferObject>(new[] { new SelfReferencingDataTransferObject() })
                    }
                })
            };
            Assert.IsNotNull(dto1.ToViewModel<SelfReferencingViewModel>().ToDto<SelfReferencingDataTransferObject>());

            var dto2 = new SelfReferencingDataTransferObject();
            dto2.Other = new SelfReferencingDataTransferObject
            {
                Others = new List<SelfReferencingDataTransferObject>(new[] {
                    new SelfReferencingDataTransferObject
                    {
                        Others = new List<SelfReferencingDataTransferObject>(new[] { dto2 })
                    }
                })
            };

            try
            {
                dto2.ToViewModel<SelfReferencingViewModel>();
                Assert.Fail("Expected the conversion of cyclic relationships to fail.");
            }
            catch (Exception) { }

            var viewModel = new SelfReferencingViewModel();
            viewModel.Others = new ObservableViewModelCollection<SelfReferencingViewModel>(viewModel)
            {
                viewModel
            };

            try
            {
                viewModel.ToDto<SelfReferencingDataTransferObject>();
                Assert.Fail("Expected the convertion of cyclic relationships to fail.");
            }
            catch (Exception) { }
        }

        class CatDataTransferObject
        {
            public string? Nickname { get; set; }

            public IList<string>? FavoriteFood { get; set; }

            public FurDataTransferObject? Fur { get; set; }

            public List<PawDataTransferObject>? Paws { get; set; }
        }

        class FurDataTransferObject
        {
            public string? Description { get; set; }
        }

        class PawDataTransferObject
        {
            public int Steps { get; set; }
        }

        [Dto(typeof(CatDataTransferObject))]
        class CatViewModel : ViewModel
        {
            private string? _nickname;

            [DtoProperty]
            public string? Nickname
            {
                get => _nickname;
                set => SetProperty(ref _nickname, value);
            }

            private IList<string>? _favoriteFood;

            [DtoProperty]
            public IList<string>? FavoriteFood
            {
                get => _favoriteFood;
                set => SetProperty(ref _favoriteFood, value);
            }

            private FurViewModel? _fur;

            [DtoProperty]
            public FurViewModel? Fur
            {
                get => _fur;
                set => SetProperty(ref _fur, value);
            }

            [DtoProperty]
            public ObservableViewModelCollection<PawViewModel>? Paws { get; set; }
        }

        [Dto(typeof(FurDataTransferObject))]
        class FurViewModel : ViewModel
        {
            private string? _description;

            [DtoProperty]
            public string? Description
            {
                get => _description;
                set => SetProperty(ref _description, value);
            }
        }

        [Dto(typeof(PawDataTransferObject))]
        class PawViewModel : ViewModel
        {
            private int _steps;

            [DtoProperty]
            public int Steps
            {
                get => _steps;
                set => SetProperty(ref _steps, value);
            }
        }

        class SelfReferencingDataTransferObject
        {
            public SelfReferencingDataTransferObject? Other { get; set; }

            public List<SelfReferencingDataTransferObject>? Others { get; set; }
        }

        [Dto(typeof(SelfReferencingDataTransferObject))]
        class SelfReferencingViewModel : ViewModel
        {
            private SelfReferencingViewModel? _other;

            [DtoProperty]
            public SelfReferencingViewModel? Other
            {
                get => _other;
                set => SetProperty(ref _other, value);
            }

            [DtoProperty]
            public ObservableViewModelCollection<SelfReferencingViewModel>? Others { get; set; }
        }
    }
}
