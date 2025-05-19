// SPDX-License-Identifier: GPL-3.0-only
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using SoulsFormats;
using EldenRingTool;
using GeoGuesserBuilder.ViewModels;

namespace GeoGuesserBuilder.Services;

public class MapEditorService
{
    public string VanillaMapFilesDir = ".\\Resources\\vanilla\\map\\mapstudio";
    public string ModOutputDir = ".\\Resources\\ModEngine-2.1.0.0-win64\\geoguesser\\map\\MapStudio";
    public uint EventIDBase = 1056440000;  // generated using matt's spreadsheet
    public uint EntityIDBase = 1056442000; // ends with 2xxx (convention for a region entity id)
    public int RegionIDBase = 424242;      // i just made this up >:]

    private MSBE.Region.Message GenerateMessage(
        (float, float, float, float, uint) coords,
        uint mapStudioLayer,
        int regionID,
        uint enableEventFlagID,
        uint entityID)
    {
        MSBE.Region.Message message = new();
        message.MessageID = 7040;   // "Surely what you seek is somewhere close by..."
        // I copied the following parameters from the Chapel of Anticipation message
        message.Shape = new MSB.Shape.Sphere();
        message.Hidden = false;
        message.MessageSfxID = 31;
        message.NPCParamID = -1;
        message.CharaInitParamID = -1;
        message.AnimationID = -1;
        message.CharacterModelName = -1;
        message.MapID = -1;
        message.UnkE08 = 255;
        message.UnkS04 = 0;
        message.UnkS0C = -1;
        message.ItemLotParamID = -1;
        // Now use our unique information for this message
        message.Position = new Vector3(coords.Item1, coords.Item2, coords.Item3);
        message.Rotation = new Vector3(0, coords.Item4 * (float)180.0 / (float)3.141592, 0);
        message.MapStudioLayer = mapStudioLayer;
        message.RegionID = regionID;
        message.EnableEventFlagID = enableEventFlagID;
        message.EntityID = entityID;
        return message;
    }

    private string FormatMapName(string mapIDString)
    {
        // mapIDString is of the format "60 43 37 0" but we need it to be "m60_43_37_00"
        string[] mapIDComponents = mapIDString.Split(' ');
        string formattedMapName = "m";
        foreach(string mapIDComponent in mapIDComponents)
        {
            if (mapIDComponent.Length == 1)
            {
                formattedMapName += "0" + mapIDComponent + "_";
            }
            else
            {
                formattedMapName += mapIDComponent + "_"; 
            }
        }
        return formattedMapName[..^1]; // return everything except the final "_"
    }

    private async Task<MSBE> ReadMSBFromFile(string fileDir, string mapIDString)
    {
        string mapName = FormatMapName(mapIDString);
        string fileName = $"{fileDir}\\{mapName}.msb.dcx";
        if (!File.Exists(fileName))
        {
            throw new Exception($"File {fileName} does not exist!");
        }
        byte[] fileBytes = await File.ReadAllBytesAsync(fileName);
        return MSBE.Read(fileBytes);
    }

    private async Task WriteMSBToFile(string fileDir, string mapIDString, MSBE msb)
    {
        string mapName = FormatMapName(mapIDString);
        string fileName = $"{fileDir}\\{mapName}.msb.dcx";
        if (!Directory.Exists(fileDir))
        {
            Directory.CreateDirectory(fileDir);
        }
        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }
        byte[] modifiedFileBytes = msb.Write();
        await File.WriteAllBytesAsync(fileName, modifiedFileBytes);
    }
    
    public async void ModifyMaps(List<GGLocationViewModel> capturedLocations)
    {
        // We might need to add two messages to the same map, so organize by mapIDString.
        Dictionary<string, List<GGLocationViewModel>> locationsByMapIDString = new();
        for (int i = 0; i < capturedLocations.Count; i++)
        {
            var coords = capturedLocations[i].Coordinates;
            uint mapID = coords.Item5;
            string mapIDString = TeleportHelper.mapIDString(mapID);
            if (locationsByMapIDString.TryGetValue(mapIDString, out var mapLocations))
            {
                mapLocations.Add(capturedLocations[i]);
            } else
            {
                locationsByMapIDString[mapIDString] = new List<GGLocationViewModel>() { capturedLocations[i] };
            }
        }

        if (Directory.Exists(ModOutputDir))
            Directory.Delete(ModOutputDir, true);
        Directory.CreateDirectory(ModOutputDir);

        uint m = 0;
        foreach (string mapIDString in locationsByMapIDString.Keys)
        {
            // load the MSB file for the specified map
            MSBE msb = await ReadMSBFromFile(VanillaMapFilesDir, mapIDString);

            // read the mapstudiolayer from one of the entries in the map (they should all be the same)
            uint mapStudioLayer = msb.Regions.GetEntries()[0].MapStudioLayer;

            // iterate through each captured location in this map, generating a new message for each one.
            foreach (GGLocationViewModel vm in locationsByMapIDString[mapIDString])
            {
                uint enableEventFlagID = EventIDBase + m;
                uint entityID = EntityIDBase + m;
                int regionID = RegionIDBase + (int)m;
                m++;
                MSBE.Region.Message message = GenerateMessage(
                    vm.Coordinates,
                    mapStudioLayer,
                    regionID,
                    enableEventFlagID,
                    entityID);
                msb.Regions.Add(message);
            }

            await WriteMSBToFile(ModOutputDir, mapIDString, msb);
        }
    }
}
