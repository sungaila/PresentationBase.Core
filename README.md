# ![PresentationBase.Core Logo](https://raw.githubusercontent.com/sungaila/PresentationBase.Core/master/Icon_64.png) PresentationBase.Core

[![Azure DevOps builds (branch)](https://img.shields.io/azure-devops/build/sungaila/2dc19da0-58ad-4e78-b091-a473a1ad54a8/1/master?style=flat-square)](https://dev.azure.com/sungaila/PresentationBase.Core/_build/latest?definitionId=1&branchName=master)
[![Azure DevOps tests (branch)](https://img.shields.io/azure-devops/tests/sungaila/PresentationBase.Core/1/master?style=flat-square)](https://dev.azure.com/sungaila/PresentationBase.Core/_build/latest?definitionId=1&branchName=master)
[![SonarCloud Quality Gate](https://img.shields.io/sonar/quality_gate/sungaila_PresentationBase.Core?server=https%3A%2F%2Fsonarcloud.io&style=flat-square)](https://sonarcloud.io/dashboard?id=sungaila_PresentationBase.Core)
[![NuGet version](https://img.shields.io/nuget/v/PresentationBase.Core.svg?style=flat-square)](https://www.nuget.org/packages/PresentationBase.Core/)
[![NuGet downloads](https://img.shields.io/nuget/dt/PresentationBase.Core.svg?style=flat-square)](https://www.nuget.org/packages/PresentationBase.Core/)
[![GitHub license](https://img.shields.io/github/license/sungaila/PresentationBase.Core?style=flat-square)](https://github.com/sungaila/PresentationBase.Core/blob/master/LICENSE)

A lightweight MVVM implementation targeting **.NET Standard 2.0** and **.NET 6.0**.

It contains base implementations for *view models*, *commands*, *data transfer object* conversion and more.

## Examples
Take a look at the [Quick start in the wiki](https://github.com/sungaila/PresentationBase.Core/wiki). Here are some basic examples for using PresentationBase.Core:

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
