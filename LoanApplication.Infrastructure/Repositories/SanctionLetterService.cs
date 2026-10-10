using AutoMapper;
using LoanApp.Models;
using LoanApplication.Application.DTO;
using LoanApplication.Application.Interfcae;
using LoanApplication.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.Hosting;

namespace LoanApplication.Infrastructure.Repositories
{
    public class SanctionLetterService : ISanctionLetterService
    {
        private readonly AppDBContext db;
        private readonly IMapper mapper;
        private readonly IWebHostEnvironment env;

        public SanctionLetterService(AppDBContext db, IMapper mapper, IWebHostEnvironment env)
        {
            this.db = db;
            this.mapper = mapper;
            this.env = env;
        }

        public async Task<List<ApprovedLoanDto>> GetApprovedLoansAsync()
        {
            var loans = await db.LoanDeals
                .Where(x => x.CurrentStatus == "Approved")
                .ToListAsync();

            return mapper.Map<List<ApprovedLoanDto>>(loans);
        }


        // 2. Generate sanction letter PDF
        public async Task<SanctionLetterDocumentDto>
            GenerateSanctionLetterAsync(int dealId)
        {
            var deal = await db.LoanDeals
                .Include(x => x.Customer)
                .FirstOrDefaultAsync(x => x.DealId == dealId);

            if (deal == null)
                throw new KeyNotFoundException("Loan deal not found.");

            if (deal.CurrentStatus != "Approved")
                throw new InvalidOperationException("Only approved loans can have a sanction letter.");

            // Prevent duplicate sanction letters
            var existing = await db.SanctionLetters
                .FirstOrDefaultAsync(x => x.DealId == dealId);

            if (existing != null)
                throw new InvalidOperationException("A sanction letter already exists for this loan.");

            var customerName = deal.Customer == null
                ? "Customer"
                : $"{deal.Customer.FirstName} {deal.Customer.LastName}".Trim();

            // Generate PDF in memory
            var pdfBytes = Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .AlignCenter()
                        .Text("LOAN SANCTION LETTER")
                        .Bold()
                        .FontSize(20);

                    page.Content().PaddingVertical(25).Column(column =>
                    {
                        column.Spacing(12);

                        column.Item().Text(
                            $"Date: {DateTime.Now:dd-MM-yyyy}");

                        column.Item().Text(
                            $"Customer Name: {customerName}");

                        column.Item().Text(
                            $"Loan Deal ID: {deal.DealId}");

                        column.Item().Text(
                            $"Loan Type: {deal.LoanType}");

                        column.Item().Text(
                            $"Approved Amount: {deal.ApprovedAmount:N2}");

                        column.Item().Text(
                            $"Interest Rate: {deal.InterestRate}% per annum");

                        column.Item().Text(
                            $"Tenure: {deal.TenureMonths} months");

                        column.Item().Text(
                            $"Monthly EMI: {deal.EmiAmount:N2}");

                        column.Item().PaddingTop(20).Text(
                            "Your loan has been approved subject to the " +
                            "applicable terms and conditions of the lender.");

                        column.Item().PaddingTop(20)
                            .Text("Authorized Officer");
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text("Computer-generated sanction letter");
                });
            }).GeneratePdf();

            // Save PDF in LoanApplication.Api/wwwroot/Documents
            var folderPath = Path.Combine(
                env.ContentRootPath,"wwwroot","Documents");

            Directory.CreateDirectory(folderPath);

            var fileName = $"SanctionLetter_{deal.DealId}.pdf";
            var filePath = Path.Combine(folderPath, fileName);

            await File.WriteAllBytesAsync(filePath, pdfBytes);

            // Save sanction details only in database
            var sanctionLetter = new SanctionLetter
            {
                DealId = deal.DealId,
                LoanAmount = deal.ApprovedAmount,
                InterestRate = deal.InterestRate,
                TenureMonths = deal.TenureMonths,
                EmiAmount = deal.EmiAmount,
                CreatedAt = DateTime.Now
            };

            try
            {
                db.SanctionLetters.Add(sanctionLetter);
                await db.SaveChangesAsync();
            }
            catch
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);

                throw;
            }

            // FileName comes from your AutoMapper mapping
            return mapper.Map<SanctionLetterDocumentDto>(
                sanctionLetter);
        }

        // Get existing sanction letter details
        public async Task<SanctionLetterDocumentDto?>
            GetSanctionLetterAsync(int dealId)
        {
            var sanctionLetter = await db.SanctionLetters
                .FirstOrDefaultAsync(x => x.DealId == dealId);

            if (sanctionLetter == null)
                return null;

            return mapper.Map<SanctionLetterDocumentDto>(sanctionLetter);
        }
    }

}

