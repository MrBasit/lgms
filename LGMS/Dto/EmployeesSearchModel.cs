using LGMS.Data.Model;
using LGMS.Enum;
using LGMS.Interface;

namespace LGMS.Dto
{
    public class EmployeesSearchModel:BaseSearchModel
    {
        public List<EmployeeStatus> Statuses { get; set; }
    }
}
