using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace LGMS.Data.Model
{
    public class Designation
    {
        public int Id { get; set; }

        public string Title { get; set; }
    }
}