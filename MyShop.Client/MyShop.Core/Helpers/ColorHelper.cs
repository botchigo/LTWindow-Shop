using Microsoft.UI;
using System.Globalization;
using Windows.UI;

namespace MyShop.Core.Helpers
{
    public static class ColorHelper
    {
        /// <summary>
        /// Chuyển đổi mã màu Hex (dạng #RRGGBB hoặc #AARRGGBB) thành Windows.UI.Color
        /// </summary>
        public static Color FromHex(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                return Colors.Gray; // Màu mặc định nếu chuỗi rỗng

            hex = hex.Replace("#", string.Empty);

            byte a = 255;
            byte r = 0;
            byte g = 0;
            byte b = 0;

            if (hex.Length == 6) 
            {
                r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
                g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
                b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
            }
            else if (hex.Length == 8) 
            {
                a = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
                r = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
                g = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
                b = byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber);
            }
            else
            {
                return Colors.Gray;
            }

            return Color.FromArgb(a, r, g, b);
        }
    }
}
