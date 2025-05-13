using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoGuesserBuilder.ViewModels;

public class GGLocationViewModel: INotifyPropertyChanged
{
    public string Name { get; set; } = "";
    public (float, float, float, float, uint) Coordinates { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;
}
