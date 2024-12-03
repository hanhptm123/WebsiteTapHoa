using System.Drawing.Printing;

namespace TapHoa.Models
{
    public class CartItem
    {
        public int Masanpham { get; set; }
        public string Hinhanh { get; set; }
        public string Tensanpham { get; set; }

        public double Dongia { get; set; }
        public int Soluong { get; set; }
        public double Thanhtien => Soluong * Dongia;
        public double Discount { get; set; }
        public double Giasaugiam => Discount > 0 ? Thanhtien * (1 - Discount / 100) : Thanhtien;


    }
}