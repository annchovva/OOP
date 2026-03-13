using FinancialSystem.Domain.Entities;
using System.Collections.Generic;

namespace FinancialSystem.Application.Interfaces
{
    public interface IEnterpriseService
    {
        // 1. Для клиента
        List<Enterprise> GetAllEnterprises();
        void SendJoinRequest(int userId, int enterpriseId);
        void SendSalaryPaymentRequest(int userId); // Только если уже сотрудник
        List<SalaryRequest> GetMyApprovedPayments(int userId); // Список выплат, готовых к получению
        bool ClaimSalary(int requestId, int targetAccountId); // Финальный шаг: получение на счет

        // 2. Для менеджера
        List<SalaryRequest> GetPendingJoinRequests();
        List<SalaryRequest> GetPendingPaymentRequests();
        void ApproveJoinRequest(int requestId);
        void ApprovePaymentRequest(int requestId, decimal amount);

        // Просмотр всех предприятий с сотрудниками (по ТЗ)
        List<Enterprise> GetEnterprisesWithEmployees();
    }
}

