namespace Constellation.Application.DTOs;

using Core.Models.Families;
using System.Collections.Generic;

public class FamilyDetailsDto
{
    public List<string> StudentIds { get; set; } = new List<string>();
    public string FamilyId { get; set; }
    public List<Contact> Contacts { get; set; } = new();
    public string FamilyEmail { get; set; }
    public string AddressName { get; set; }
    public string AddressLine1 { get; set; }
    public string AddressLine2 { get; set; }
    public string AddressTown { get; set; }
    public string AddressState { get; set; }
    public string AddressPostCode { get; set; }

    public class Contact
    {
        public string SentralId { get; set; }
        public int Sequence { get; set; }
        public Parent.SentralReference SentralReference { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
    }
}