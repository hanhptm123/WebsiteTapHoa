﻿using System;
using System.Collections.Generic;

namespace TapHoa.Data;

public partial class Thuonghieu
{
    public int Mathuonghieu { get; set; }

    public string Tenthuonghieu { get; set; } = null!;

    public virtual ICollection<Sanpham> Sanphams { get; set; } = new List<Sanpham>();
}
