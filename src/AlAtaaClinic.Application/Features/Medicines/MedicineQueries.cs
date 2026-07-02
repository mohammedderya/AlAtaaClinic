using AlAtaaClinic.Application.Common.Messaging;
using AlAtaaClinic.Application.Common.Models;

namespace AlAtaaClinic.Application.Features.Medicines;

public sealed record GetMedicineByIdQuery(long Id) : IQuery<MedicineDto>;

public sealed record SearchMedicinesQuery(string? SearchText, PageRequest Page) : IQuery<PagedResult<MedicineListDto>>;
