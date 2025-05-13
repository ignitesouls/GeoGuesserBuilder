using GeoGuesserBuilder.Models;
using GeoGuesserBuilder.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace GeoGuesserBuilder.Converters;

public static class GGLocationConverter
{
    public static GGLocationModel ToModel(GGLocationViewModel vm) => new()
    {
        Name = vm.Name,
        X = vm.Coordinates.Item1,
        Y = vm.Coordinates.Item2,
        Z = vm.Coordinates.Item3,
        Rotation = vm.Coordinates.Item4,
        MapID = vm.Coordinates.Item5,

    };

    public static GGLocationViewModel ToViewModel(GGLocationModel model) => new()
    {
        Name = model.Name,
        Coordinates = (model.X, model.Y, model.Z, model.Rotation, model.MapID),
    };

    public static List<GGLocationModel> ToModelList(IEnumerable<GGLocationViewModel> vms) =>
        vms.Select(ToModel).ToList();

    public static List<GGLocationViewModel> ToViewModelList(IEnumerable<GGLocationModel> models) =>
        models.Select(ToViewModel).ToList();
}
