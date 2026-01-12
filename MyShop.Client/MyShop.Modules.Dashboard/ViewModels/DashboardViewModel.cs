using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Contract;
using MyShop.Core.Interfaces;
using MyShop.Infrastructure;
using StrawberryShake;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MyShop.Modules.Dashboard.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly IMyShopClient _client;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;

        // ---  Display Properties ---
        [ObservableProperty] private string _userName = "User";
        [ObservableProperty] private string _welcomeText = "Welcome Back!";
        [ObservableProperty] private string _dateText = "";

        // Stastistic
        [ObservableProperty] private int _totalProducts;
        [ObservableProperty] private int _todayOrders;
        [ObservableProperty] private decimal _todayRevenue; 
        [ObservableProperty] private int _lowStockCount;

        public ObservableCollection<IGetDashboardData_Top5BestSellers> TopProducts { get; } = new();
        public ObservableCollection<IGetDashboardData_Top5LowStock> LowStockProducts { get; } = new();
        public ObservableCollection<IGetDashboardData_Latest3Orders> RecentOrders { get; } = new();

        // Chart
        public ObservableCollection<IGetDashboardData_RevenueByDayCurrentMonth> MonthlyRevenue { get; } = new();

        [ObservableProperty] private bool _isLoading;

        public DashboardViewModel(IMyShopClient client, IDialogService dialogService, INavigationService navigationService)
        {
            _client = client;
            _dialogService = dialogService;
            _navigationService = navigationService;

            DateText = DateTime.Now.ToString("dddd, dd MMMM yyyy");
            _ = LoadDashboardDataAsync();
        }

        public void Initialize(string userName)
        {
            UserName = userName;
            WelcomeText = $"Xin chào, {userName}!";
        }

        [RelayCommand]
        public async Task LoadDashboardDataAsync()
        {
            if (IsLoading) return;
            IsLoading = true;

            try
            {
                var result = await _client.GetDashboardData.ExecuteAsync();

                if (result.IsErrorResult())
                {
                    await _dialogService.ShowErrorDialogAsync("Lỗi tải dữ liệu", result.Errors[0].Message);
                    return;
                }

                var data = result.Data;

                // 1. Statistic map
                TotalProducts = data.TotalProducts;
                TodayOrders = data.TotalOrdersToday;
                TodayRevenue = data.TotalRevenueToday;
                LowStockCount = data.Top5LowStock.Count;

                // 2. List map
                TopProducts.Clear();
                foreach (var p in data.Top5BestSellers) TopProducts.Add(p);

                LowStockProducts.Clear();
                foreach (var p in data.Top5LowStock) LowStockProducts.Add(p);

                RecentOrders.Clear();
                foreach (var o in data.Latest3Orders) RecentOrders.Add(o);

                // 3. Chart map
                MonthlyRevenue.Clear();
                foreach (var r in data.RevenueByDayCurrentMonth) MonthlyRevenue.Add(r);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorDialogAsync("Lỗi hệ thống", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
