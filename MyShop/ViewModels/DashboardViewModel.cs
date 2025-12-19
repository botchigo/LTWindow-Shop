using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Database.Repositories;
using MyShop.Services;

namespace MyShop.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly DatabaseManager _dbManager;
        private readonly DashboardRepository _dashboardRepo;

        [ObservableProperty]
        private string _userName = "User";

        [ObservableProperty]
        private string _welcomeText = "Welcome Back!";

        [ObservableProperty]
        private string _dateText = string.Empty;

        [ObservableProperty]
        private string _monthYearText = string.Empty;

        [ObservableProperty]
        private int _totalProducts;

        [ObservableProperty]
        private int _todayOrders;

        [ObservableProperty]
        private long _todayRevenue;

        [ObservableProperty]
        private int _lowStockCount;

        [ObservableProperty]
        private ObservableCollection<BestSellerProduct> _topProducts = new();

        [ObservableProperty]
        private ObservableCollection<LowStockProduct> _lowStockProducts = new();

        [ObservableProperty]
        private ObservableCollection<RecentOrder> _recentOrders = new();

        [ObservableProperty]
        private ObservableCollection<DailyRevenue> _monthlyRevenue = new();

        [ObservableProperty]
        private bool _isLoading;

        public DashboardViewModel(DatabaseManager dbManager)
        {
            _dbManager = dbManager ?? throw new ArgumentNullException(nameof(dbManager));
            _dashboardRepo = new DashboardRepository(_dbManager.Context);
            
            DateText = DateTime.Now.ToString("dddd, dd MMMM yyyy");
            MonthYearText = DateTime.Now.ToString("MMMM yyyy");
        }

        public void Initialize(string userName)
        {
            UserName = userName;
            WelcomeText = $"Xin chào, {userName}!";
            _ = LoadDashboardDataAsync();
        }

        [RelayCommand]
        private async Task LoadDashboardDataAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                Debug.WriteLine("[DashboardViewModel] Loading dashboard data...");

                var data = await _dashboardRepo.GetDashboardDataAsync();

                // Update statistics
                TotalProducts = data.TotalProducts;
                TodayOrders = data.TodayOrders;
                TodayRevenue = data.TodayRevenue;
                LowStockCount = data.LowStockProducts.Count;

                // Update collections
                TopProducts.Clear();
                foreach (var item in data.BestSellerProducts)
                {
                    TopProducts.Add(item);
                }

                LowStockProducts.Clear();
                foreach (var item in data.LowStockProducts)
                {
                    LowStockProducts.Add(item);
                }

                RecentOrders.Clear();
                foreach (var item in data.RecentOrders)
                {
                    RecentOrders.Add(item);
                }

                MonthlyRevenue.Clear();
                foreach (var item in data.MonthlyRevenue)
                {
                    MonthlyRevenue.Add(item);
                }

                Debug.WriteLine($"[DashboardViewModel] Loaded: {TotalProducts} products, {TodayOrders} orders, {LowStockCount} low stock");
                Debug.WriteLine($"[DashboardViewModel] Monthly revenue data points: {MonthlyRevenue.Count}");
                
                if (MonthlyRevenue.Count > 0)
                {
                    Debug.WriteLine($"[DashboardViewModel] First revenue: {MonthlyRevenue[0].Day:yyyy-MM-dd} = {MonthlyRevenue[0].TotalRevenue}");
                    Debug.WriteLine($"[DashboardViewModel] Last revenue: {MonthlyRevenue[^1].Day:yyyy-MM-dd} = {MonthlyRevenue[^1].TotalRevenue}");
                }
                else
                {
                    Debug.WriteLine("[DashboardViewModel] WARNING: No monthly revenue data!");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DashboardViewModel] Error: {ex.Message}");
                OnErrorOccurred("L?i t?i d? li?u", $"Không th? t?i d? li?u dashboard:\n{ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        #region Events

        public event EventHandler<ErrorEventArgs>? ErrorOccurred;
        public event EventHandler? LogoutRequested;

        private void OnErrorOccurred(string title, string message)
        {
            ErrorOccurred?.Invoke(this, new ErrorEventArgs
            {
                Title = title,
                Message = message
            });
        }

        public void RequestLogout()
        {
            LogoutRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
