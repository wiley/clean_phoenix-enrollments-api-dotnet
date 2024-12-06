using System;
using System.ComponentModel.DataAnnotations;

namespace Enrollments.Domain.Params
{
    public class PaginationParams
    {
        [Range(0, int.MaxValue)]
        public int offset { get; set; } = 0;

        [Range(1, 100)]
        public int size { get; set; } = 20;
    }
}
