// SPDX-License-Identifier: GPL-3.0-only
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Org.BouncyCastle.Asn1.X509;
using GeoGuesserBuilder.Models;

namespace GeoGuesserBuilder.Services;

public class ProjectService
{
    private const string DefaultSavePath = "autosave.json";

    public async Task SaveProjectAsync(GGProject project, string? path = null)
    {
        string json = JsonSerializer.Serialize(project, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(path ?? DefaultSavePath, json);
    }

    public async Task<GGProject?> LoadProjectAsync(string? path = null)
    {
        path ??= DefaultSavePath;
        if (!File.Exists(path)) return null;

        string json = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<GGProject>(json);
    }

    public void SaveProject(GGProject project, string? path = null)
    {
        string json = JsonSerializer.Serialize(project, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path ?? DefaultSavePath, json);
    }

    public GGProject? LoadProject(string? path = null)
    {
        path ??= DefaultSavePath;
        if (!File.Exists(path)) return null;

        string json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<GGProject>(json);
    }
}
