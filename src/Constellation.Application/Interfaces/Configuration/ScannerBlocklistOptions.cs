namespace Constellation.Application.Interfaces.Configuration;

using System;
using System.Collections.Generic;
using System.Text;

public class ScannerBlocklistOptions
{
    public const string SectionName = "Constellation:ScannerBlocklist";

    public bool Enabled { get; set; } = true;
    public List<string> PathFragments { get; set; } = new();
    public List<string> Extensions { get; set; } = new();
    public int ResponseStatusCode { get; set; } = 404;
}