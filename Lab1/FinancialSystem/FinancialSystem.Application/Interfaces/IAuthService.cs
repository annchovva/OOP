using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinancialSystem.Domain.Entities;

namespace FinancialSystem.Application.Interfaces
{
    public interface IAuthService
    {
        User? Login(string login, string password); 
        bool Register(string login, string password); 
    }
}

