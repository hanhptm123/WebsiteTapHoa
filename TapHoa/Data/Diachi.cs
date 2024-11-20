using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TapHoa.Data;

public partial class Diachi
{
    public int Madc { get; set; }

    public int? Makh { get; set; }
    [Required(ErrorMessage = "Tên người nhận không được để trống")]
    [MaxLength(50, ErrorMessage = "Tên người nhận không được dài quá 50 ký tự")]
    public string Tennguoinhan { get; set; } = null!;
    [Required(ErrorMessage = "Số điện thoại không được để trống")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Số điện thoại phải đúng 10 chữ số")]
    public string Sdt { get; set; } = null!;
    [Required(ErrorMessage = "Địa chỉ không được để trống")]
    [MaxLength(200, ErrorMessage = "Địa chỉ không được dài quá 200 ký tự")]
    public string Diachi1 { get; set; } = null!;

    public virtual Khachhang? MakhNavigation { get; set; }
}
