<img src="https://raw.githubusercontent.com/sungaila/PresentationBase.Core/master/Icon.png" align="left" width="64" height="64" alt="PresentationBase.Core Logo">

# PresentationBase.Core
[![Azure DevOps builds (branch)](https://img.shields.io/azure-devops/build/sungaila/2dc19da0-58ad-4e78-b091-a473a1ad54a8/1/master?style=flat-square)](https://dev.azure.com/sungaila/PresentationBase.Core/_build/latest?definitionId=1&branchName=master)
[![Azure DevOps tests (branch)](https://img.shields.io/azure-devops/tests/sungaila/PresentationBase.Core/1/master?style=flat-square)](https://dev.azure.com/sungaila/PresentationBase.Core/_build/latest?definitionId=1&branchName=master)
[![NuGet version](https://img.shields.io/nuget/v/PresentationBase.Core.svg?style=flat-square)](https://www.nuget.org/packages/PresentationBase.Core/)
[![NuGet downloads](https://img.shields.io/nuget/dt/PresentationBase.Core.svg?style=flat-square)](https://www.nuget.org/packages/PresentationBase.Core/)
[![GitHub license](https://img.shields.io/github/license/sungaila/PresentationBase.Core?style=flat-square)](https://github.com/sungaila/PresentationBase.Core/blob/master/LICENSE)

A lightweight MVVM implementation targeting both **.NET Framework 4.5** and **.NET Standard 2.0**.

It contains base implementations for *view models*, *commands*, *data transfer object* conversion and more.

The following platform specific extensions exist:
- [<img src="https://raw.githubusercontent.com/sungaila/PresentationBase/master/Icon.png" align="center" width="24" height="24" alt="PresentationBase Logo"> PresentationBase (WPF)](https://github.com/sungaila/PresentationBase)

## Examples
Here are some examples for using PresentationBase.Core in your project.

### ViewModels with bindable properties
```csharp
public class AwesomeViewModel : ViewModel
{
    private string _name;
  
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
}
```

### ... and with property validation
```csharp
public class AwesomeViewModel : ViewModel
{
    private string _name;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value, NameValidation);
    }

    private IEnumerable<string> NameValidation(string value)
    {
        if (string.IsNullOrEmpty(value))
            yield return "Name cannot be null or empty!";
        else if (value == "sungaila")
            yield return "Name cannot be stupid!";
    }
}
```

### ViewModel collections
```csharp
public class AwesomeViewModel : ViewModel
{
    public ObservableViewModelCollection<ChildViewModel> Children { get; }
    
    public AwesomeViewModel()
    {
        Children = new ObservableViewModelCollection<ChildViewModel>(this);
        Children.Add(new ChildViewModel { Nickname = "Blinky" });
        Children.Add(new ChildViewModel { Nickname = "Pinky" });
        Children.Add(new ChildViewModel { Nickname = "Inky" });
        Children.Add(new ChildViewModel { Nickname = "Clyde" });
    }
}
```

### Commands
Your command can be defined anywhere you want (as long as its assembly is referenced by the application). Please note that a parameterless constructor (or none at all) is needed.
```csharp
public class AlertCommand : ViewModelCommand<AwesomeViewModel>
{
    public override void Execute(AwesomeViewModel parameter)
    {
        System.Windows.MessageBox.Show("You just clicked that button.");
    }

    public override bool CanExecute(AwesomeViewModel parameter)
    {
        return parameter.Name != "John Doe";
    }
}
```

### ... and async commands
```csharp
public class AlertCommandAsync : ViewModelCommandAsync<AwesomeViewModel>
{
    protected override async Task ExecutionAsync(AwesomeViewModel parameter)
    {
        await Task.Run(() =>
        {
            System.Threading.Thread.Sleep(2000);
            System.Windows.MessageBox.Show("You clicked that button two seconds ago.");
        });
    }
}
```

### ViewModels ↔ Data Transfer Objects conversion
```csharp
// C# code of DTO class
public class AwesomeTransferDataObject
{
    public string PersonName { get; set; }

    public int PersonAge { get; set; }
}
```

```csharp
// C# code of your ViewModel class
[Dto(typeof(AwesomeTransferDataObject))]
public class AwesomeViewModel : ViewModel
{
    private string _name;

    [DtoProperty(nameof(AwesomeTransferDataObject.PersonName))]
    public string Name
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
```

```csharp
// C# code of the conversion
var dto = new AwesomeTransferDataObject { PersonName = "John" };
var viewModel = dto.ToViewModel<AwesomeViewModel>();
if (viewModel.Name == "John")
    viewModel.Age = 33;
var dto2 = viewModel.ToDto<AwesomeTransferDataObject>();
```

### ... and nested ViewModels
```csharp
// C# code of DTO class
public class NestedTransferDataObject
{
    public string Name { get; set; }

    public List<NestedTransferDataObject> Others { get; set; }
}
```

```csharp
// C# code of your ViewModel class
[Dto(typeof(NestedTransferDataObject))]
public class NestedViewModel : ViewModel
{
    private string _name;

    [DtoProperty]
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    [DtoProperty]
    public ObservableViewModelCollection<NestedViewModel> Others { get; }

    public NestedViewModel()
    {
        Others = new ObservableViewModelCollection<NestedViewModel>(this);
    }
}
```

```csharp
// C# code of the conversion
var dto = new NestedTransferDataObject
{
    Name = "Timmy",
    Others = new List<NestedTransferDataObject>(new[] {
        new NestedTransferDataObject { PersonName = "Bobby" }
    })
};
var viewModel = dto.ToViewModel<NestedViewModel>();
if (viewModel.Others.Single().Name == "Bobby")
    viewModel.Name = "Definitely not Timmy";
var dto2 = viewModel.ToDto<NestedTransferDataObject>();
```