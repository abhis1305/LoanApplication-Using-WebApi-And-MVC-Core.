using LoanApplication.Application.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace LoanApplication.Application.Interfcae
{
    public interface ISanctionLetterService
    {
        Task<List<ApprovedLoanDto>> GetApprovedLoansAsync();

        Task<SanctionLetterDocumentDto> GenerateSanctionLetterAsync(int dealId);

        
        Task<SanctionLetterDocumentDto?> GetSanctionLetterAsync(int dealId);
    }
}
