using System;
using System.Collections.Generic;

namespace SolRIA.SAFT.Parser.Models;

public class PagedResult<T>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalItems / (double)PageSize) : 0;
    public IReadOnlyList<T> Items { get; set; } = [];
}
