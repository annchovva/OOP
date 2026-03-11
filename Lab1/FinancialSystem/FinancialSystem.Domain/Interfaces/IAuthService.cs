using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinancialSystem.Domain.Entities;

namespace FinancialSystem.Domain.Interfaces
{
    public interface IAuthService
    {
        // Метод возвращает пользователя, если логин/пароль верны
        User? Login(string login, string password);

        // Регистрация нового клиента
        bool Register(string login, string password);
    }
}
