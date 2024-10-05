namespace LGMS.Data.Model
{
    public class AttendanceRecord
    {
        public int Id { get; set; }
        public AttendanceId AttendanceId { get; set; }            
        public DateTime Date { get; set; }          
        public string? CheckIns { get; set; }  
        public string? CheckOuts { get; set; } 
        public AttendanceRecordStatus Status { get; set; }         
        public TimeSpan RequiredTime { get; set; }  
        public TimeSpan ActualTime { get; set; }    
        public int UnderHours { get; set; }  
        public int OverHours { get; set; }     
        public bool IsRecordOk { get; set; }  
    }
}
