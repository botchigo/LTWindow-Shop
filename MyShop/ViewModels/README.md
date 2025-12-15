# ViewModels Directory

## ?? C?u trúc MVVM Pattern

Thý m?c này ch?a các ViewModel classes theo mô h?nh **MVVM (Model-View-ViewModel)**.

## ?? Files

### 1. **ViewModelBase.cs**
Base class cho t?t c? ViewModels, implement `INotifyPropertyChanged`.

```csharp
public abstract class ViewModelBase : INotifyPropertyChanged
```

**Ch?c nãng:**
- ? Implement `INotifyPropertyChanged` ð? notify UI khi property thay ð?i
- ? `OnPropertyChanged()` - Raise PropertyChanged event
- ? `SetProperty<T>()` - Set property value và t? ð?ng raise event n?u giá tr? thay ð?i

**Cách s? d?ng:**
```csharp
public class MyViewModel : ViewModelBase
{
    private string _name;
    
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
}
```

---

### 2. **RelayCommand.cs**
Implementation c?a `ICommand` cho WinUI 3.

```csharp
public class AsyncRelayCommand : ICommand
public class AsyncRelayCommand<T> : ICommand
public class RelayCommand : ICommand
```

**Ch?c nãng:**
- ? `AsyncRelayCommand` - Command cho async operations không có parameter
- ? `AsyncRelayCommand<T>` - Command cho async operations có parameter
- ? `RelayCommand` - Command cho synchronous operations
- ? T? ð?ng disable button khi ðang execute (prevent double-click)

**Cách s? d?ng:**
```csharp
// Async command
LoginCommand = new AsyncRelayCommand(LoginAsync, CanLogin);

// Sync command
ClearCommand = new RelayCommand(Clear, CanClear);
```

---

### 3. **LoginViewModel.cs**
ViewModel cho màn h?nh ðãng nh?p.

```csharp
public class LoginViewModel : ViewModelBase
```

**Properties:**
- `Username` (string) - Username ngý?i dùng nh?p
- `Password` (string) - Password ngý?i dùng nh?p
- `RememberMe` (bool) - Tr?ng thái Remember Me
- `IsLoading` (bool) - Tr?ng thái ðang loading

**Commands:**
- `LoginCommand` - Th?c hi?n ðãng nh?p

**Events:**
- `LoginSuccess` - Event khi ðãng nh?p thành công
- `ErrorOccurred` - Event khi có l?i

**Cách s? d?ng:**
```csharp
var viewModel = new LoginViewModel();

// Ðãng k? events
viewModel.LoginSuccess += (s, e) => 
{
    Console.WriteLine($"Welcome {e.FullName}");
};

viewModel.ErrorOccurred += (s, e) => 
{
    ShowMessage(e.Title, e.Message);
};

// Set properties
viewModel.Username = "admin";
viewModel.Password = "123456";

// Execute command
viewModel.LoginCommand.Execute(null);
```

---

### 4. **SignupViewModel.cs**
ViewModel cho form ðãng k?.

```csharp
public class SignupViewModel : ViewModelBase
```

**Properties:**
- `Username` (string) - Username ngý?i dùng nh?p
- `Password` (string) - Password ngý?i dùng nh?p
- `FullName` (string) - H? tên ngý?i dùng
- `IsLoading` (bool) - Tr?ng thái ðang loading

**Commands:**
- `SignupCommand` - Th?c hi?n ðãng k?

**Events:**
- `SignupSuccess` - Event khi ðãng k? thành công
- `ErrorOccurred` - Event khi có l?i

**Cách s? d?ng:**
```csharp
var viewModel = new SignupViewModel(dbManager);

// Ðãng k? events
viewModel.SignupSuccess += (s, e) => 
{
    Console.WriteLine($"User {e.Username} registered!");
};

viewModel.ErrorOccurred += (s, e) => 
{
    ShowMessage(e.Title, e.Message);
};

// Set properties
viewModel.Username = "newuser";
viewModel.Password = "password123";
viewModel.FullName = "New User";

// Execute command
viewModel.SignupCommand.Execute(null);
```

---

## ?? MVVM Pattern Benefits

### ? **Separation of Concerns**
- **Model**: Database models (`AppUser`)
- **View**: XAML files (`MainWindow.xaml`)
- **ViewModel**: Business logic (`LoginViewModel`, `SignupViewModel`)

### ? **Testability**
ViewModels có th? test mà không c?n UI:
```csharp
[TestMethod]
public async Task LoginAsync_WithValidCredentials_ShouldSucceed()
{
    // Arrange
    var viewModel = new LoginViewModel();
    viewModel.Username = "admin";
    viewModel.Password = "123456";
    
    bool loginSucceeded = false;
    viewModel.LoginSuccess += (s, e) => loginSucceeded = true;
    
    // Act
    await viewModel.LoginCommand.Execute(null);
    
    // Assert
    Assert.IsTrue(loginSucceeded);
}
```

### ? **Reusability**
ViewModel có th? s? d?ng cho nhi?u Views khác nhau.

### ? **Maintainability**
Code ðý?c tách bi?t r? ràng, d? maintain và m? r?ng.

---

## ?? Flow Diagram

```
???????????????         ????????????????         ???????????????
?    View     ?????????>?  ViewModel   ?????????>?   Model     ?
?  (XAML)     ?<?????????  (Logic)     ?<????????? (Database)  ?
???????????????         ????????????????         ???????????????
     User              INotifyPropertyChanged       Data Access
     Input                  Commands                 Repository
```

---

## ?? Best Practices

1. **Không reference View trong ViewModel**
   - ? BAD: `viewModel.ShowDialog()`
   - ? GOOD: `viewModel.ErrorOccurred += ShowDialog`

2. **S? d?ng Events thay v? callbacks**
   - ViewModel raise events, View handle events

3. **Validate trong ViewModel**
   - `CanExecute` method ð? validate trý?c khi execute command

4. **Loading state**
   - Luôn set `IsLoading` khi execute async operations

5. **Exception handling**
   - Catch exceptions trong ViewModel và raise `ErrorOccurred` event

---

## ?? Next Steps

### Ð? m? r?ng thêm:

1. **Thêm Data Binding trong XAML:**
```xaml
<TextBox Text="{x:Bind ViewModel.Username, Mode=TwoWay}" />
<Button Command="{x:Bind ViewModel.LoginCommand}" />
```

2. **Thêm Dependency Injection:**
```csharp
services.AddTransient<LoginViewModel>();
services.AddTransient<SignupViewModel>();
```

3. **Thêm Navigation Service:**
```csharp
public interface INavigationService
{
    void Navigate<TViewModel>();
}
```

4. **Thêm Validation:**
```csharp
public class LoginViewModel : ViewModelBase, INotifyDataErrorInfo
{
    // Implement validation logic
}
```

---

## ?? Resources

- [MVVM Pattern](https://docs.microsoft.com/en-us/windows/uwp/data-binding/data-binding-and-mvvm)
- [INotifyPropertyChanged](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged)
- [ICommand](https://docs.microsoft.com/en-us/dotnet/api/system.windows.input.icommand)
- [WinUI 3 Data Binding](https://docs.microsoft.com/en-us/windows/apps/winui/winui3/)
