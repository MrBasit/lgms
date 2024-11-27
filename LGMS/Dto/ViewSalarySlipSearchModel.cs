namespace LGMS.Dto
{
    public class ViewSalarySlipSearchModel : BaseSearchModel
    {
        public List<string> MachineNames { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }

        public string? Mode { get; set; }
    }
}
