namespace KyoS.Api.Models.Records;

public record NotBilled(string ClientName, string CaseNo, string DOB, string MedicaidId, string Address, string City, string State, string Zip,
                        string InsuranceMemberId, string Diagnostics, string Date, string Service, string Setting,
                        int Units, decimal  Amount, string Supervisor, string StatusBill);
