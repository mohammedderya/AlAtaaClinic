using AlAtaaClinic.Domain.Inventory;

namespace AlAtaaClinic.Application.Features.Medicines;

public static class MedicineMapper
{
    public static MedicineDto ToDto(this Medicine medicine)
    {
        var quantityOnHand = medicine.StockBatches.Sum(batch => batch.QuantityOnHand);

        return new MedicineDto(
            medicine.Id,
            medicine.MedicineCode,
            medicine.GenericName,
            medicine.TradeName,
            medicine.Unit,
            medicine.IsActive,
            quantityOnHand);
    }

    public static MedicineListDto ToListDto(this Medicine medicine)
    {
        return new MedicineListDto(
            medicine.Id,
            medicine.MedicineCode,
            medicine.GenericName,
            medicine.TradeName,
            medicine.Unit,
            medicine.IsActive);
    }
}
