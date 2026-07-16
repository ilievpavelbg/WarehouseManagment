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
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var sequence = await _dbContext.DocumentSequences
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

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return $"{GetPrefix(type)}-{year}-{sequence.LastNumber:D6}";
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private static string GetPrefix(DocumentType type)
        {
            return type switch
            {
                DocumentType.GoodsReceipt => "GR",
                DocumentType.MaterialTransfer => "TR",
                DocumentType.StockAdjustment => "ADJ",
                _ => throw new InvalidOperationException("Invalid document type.")
            };
        }
    }
}