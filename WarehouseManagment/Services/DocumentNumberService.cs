using System.Data;
using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Services
{
    public class DocumentNumberService : IDocumentNumberService
    {
        private readonly ApplicationDbContext _dbContext;

        public DocumentNumberService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> GetNextNumberAsync(DocumentType type)
        {
            if (!Enum.IsDefined(typeof(DocumentType), type))
            {
                throw new InvalidOperationException("Invalid document type.");
            }

            var year = DateTime.Now.Year;

            if (_dbContext.Database.CurrentTransaction != null)
            {
                var number = await StageNextNumberAsync(type, year);
                return FormatNumber(type, year, number);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var number = await StageNextNumberAsync(type, year);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return FormatNumber(type, year, number);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<int> StageNextNumberAsync(DocumentType type, int year)
        {
            var sequence = await _dbContext.DocumentSequences
                .FromSqlInterpolated($"SELECT * FROM DocumentSequences WITH (UPDLOCK, HOLDLOCK) WHERE DocumentType = {type} AND [Year] = {year}")
                .FirstOrDefaultAsync(x => x.DocumentType == type && x.Year == year);

            if (sequence == null)
            {
                sequence = new DocumentSequence
                {
                    DocumentType = type,
                    Year = year,
                    LastNumber = 1,
                    UpdatedOn = DateTime.Now
                };

                await _dbContext.DocumentSequences.AddAsync(sequence);
            }
            else
            {
                sequence.LastNumber++;
                sequence.UpdatedOn = DateTime.Now;
            }

            return sequence.LastNumber;
        }

        private static string FormatNumber(DocumentType type, int year, int number)
        {
            return $"{GetPrefix(type)}-{year}-{number:D6}";
        }

        private static string GetPrefix(DocumentType type)
        {
            return type switch
            {
                DocumentType.GoodsReceipt => "GR",
                DocumentType.MaterialTransfer => "TR",
                DocumentType.StockAdjustment => "ADJ",
                DocumentType.ProductionOrder => "PO",
                DocumentType.ProductionMaterialTransfer => "PMT",
                DocumentType.ProductionMaterialConsumption => "PMC",
                DocumentType.FinishedGoodsReceipt => "FGR",
                _ => throw new InvalidOperationException("Invalid document type.")
            };
        }
    }
}
