<img src="https://raw.githubusercontent.com/sungaila/PresentationBase.Core/master/Icon.png" align="left" width="64" height="64" alt="PresentationBase.Core Logo">

# PresentationBase.Core
A lightweight MVVM implementation targeting both **.NET Framework** and **.NET Core**.

It contains base implementations for *view models*, *commands*, *data transfer object* conversion and more.

Feel free to grab it from [NuGet.org](https://www.nuget.org/packages/PresentationBase.Core) or to fork it for your own needs!

The following platform specific extensions exist:
- [<img src="https://raw.githubusercontent.com/sungaila/PresentationBase/master/Icon.png" align="center" width="24" height="24" alt="PresentationBase Logo"> PresentationBase (WPF)](https://www.nuget.org/packages/PresentationBase)

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