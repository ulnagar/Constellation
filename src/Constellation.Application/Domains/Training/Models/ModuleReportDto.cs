﻿namespace Constellation.Application.Domains.Training.Models;

public class ReportDto
{
    public byte[] FileData { get; set; }
    public string FileName { get; set; }
    public string FileType { get; set; }
}
