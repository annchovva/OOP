using System;
using System.Security.Cryptography;
using System.Text;

namespace FinancialSystem.Infrastructure // или ваш неймспейс
{
    public static class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return string.Empty;

            using (var sha256 = SHA256.Create())
            {
                // Преобразуем строку пароля в массив байт
                var bytes = Encoding.UTF8.GetBytes(password);
                // Вычисляем хэш
                var hash = sha256.ComputeHash(bytes);
                // Превращаем байты в красивую строку (hex)
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}
