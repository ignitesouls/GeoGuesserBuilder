// SPDX-License-Identifier: GPL-3.0-only
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoGuesserBuilder.Models;

public class GGProject
{
    public List<GGLocationModel> Locations { get; set; } = new();
}
