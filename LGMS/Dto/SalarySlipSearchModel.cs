﻿using LGMS.Interface;

namespace LGMS.Dto
{
    public class SalarySlipSearchModel : BaseSearchModel
    {
        public List<string> MachineNames { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
    }
}
