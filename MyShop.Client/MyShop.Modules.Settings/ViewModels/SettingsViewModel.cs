using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using MyShop.Core.Interfaces;
using MyShop.Infrastructure.Services;

namespace MyShop.Modules.Settings.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly ISettingsService _settingsService;

        // Danh sách các tùy chọn phân trang
        public ObservableCollection<int> PageSizeOptions { get; } = new() { 7, 14, 21 };

        [ObservableProperty]
        private int _selectedPageSize;

        [ObservableProperty]
        private bool _isRememberLastPage;

        public SettingsViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;

            // Load dữ liệu cũ lên UI
            SelectedPageSize = _settingsService.GetPageSize();
            IsRememberLastPage = _settingsService.GetIsRememberLastPageEnabled();
        }

        // Khi người dùng thay đổi ComboBox -> Lưu ngay
        partial void OnSelectedPageSizeChanged(int value)
        {
            _settingsService.SetPageSize(value);
        }

        // Khi người dùng bật tắt Toggle -> Lưu ngay
        partial void OnIsRememberLastPageChanged(bool value)
        {
            _settingsService.SetIsRememberLastPageEnabled(value);
        }
    }
}
