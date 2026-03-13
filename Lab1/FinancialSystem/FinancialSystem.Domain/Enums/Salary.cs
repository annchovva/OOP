using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialSystem.Domain.Enums
{
    public enum SalaryRequestType { Join = 1, Payment = 2 }
    public enum SalaryRequestStatus { Pending = 1, Approved = 2, Rejected = 3, Completed = 4 }
}
