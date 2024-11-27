using System;
using System.Collections.Generic;

namespace TapHoa.Data;

public partial class Congthuc
{
    public int Mact { get; set; }

    public string? Ten { get; set; }

    public string? Video { get; set; }

    public virtual ICollection<Sanpham> Masps { get; set; } = new List<Sanpham>();
}
