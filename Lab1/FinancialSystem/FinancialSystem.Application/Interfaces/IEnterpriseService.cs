using FinancialSystem.Domain.Entities;
using System.Collections.Generic;

namespace FinancialSystem.Application.Interfaces
{
    public interface IEnterpriseService
    {
        // для клиента
        List<Enterprise> GetAllEnterprises();
        void SendJoinRequest(int userId, int enterpriseId);
        void ResignFromEnterprise(int userId); 
        void SendSalaryPaymentRequest(int userId);
        List<SalaryRequest> GetMyApprovedPayments(int userId);
        bool ClaimSalary(int requestId, int targetAccountId);
        // для менеджера
        List<SalaryRequest> GetPendingJoinRequests();
        List<SalaryRequest> GetPendingPaymentRequests();
        void ApproveJoinRequest(int requestId);
        void ApprovePaymentRequest(int requestId, decimal amount);
        List<Enterprise> GetEnterprisesWithEmployees();
    }
}

