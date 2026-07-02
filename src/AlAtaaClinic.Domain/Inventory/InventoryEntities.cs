using AlAtaaClinic.Domain.Clinical;
using AlAtaaClinic.Domain.Common;
using AlAtaaClinic.Domain.Enums;

namespace AlAtaaClinic.Domain.Inventory;

public sealed class Medicine : AggregateRoot
{
    public string MedicineCode { get; set; } = string.Empty;
    public string GenericName { get; set; } = string.Empty;
    public string? TradeName { get; set; }
    public string Unit { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public List<StockBatch> StockBatches { get; set; } = [];
    public List<StockMovement> StockMovements { get; set; } = [];
}

public sealed class StockBatch : Entity<long>
{
    public long MedicineId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateOnly? ExpiryDate { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal? UnitCost { get; set; }

    public Medicine? Medicine { get; set; }
}

public sealed class StockMovement : Entity<long>
{
    public long MedicineId { get; set; }
    public long? StockBatchId { get; set; }
    public long? PrescriptionItemId { get; set; }
    public StockMovementType MovementType { get; set; }
    public decimal Quantity { get; set; }
    public DateTime MovementDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    public Medicine? Medicine { get; set; }
    public StockBatch? StockBatch { get; set; }
    public PrescriptionItem? PrescriptionItem { get; set; }
}
